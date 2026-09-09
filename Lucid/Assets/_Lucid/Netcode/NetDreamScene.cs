using System;
using Lucid.Core;
using Lucid.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Lucid.Netcode
{
    /// <summary>
    /// The netcode's dev scene (docs/WORKPLAN.md §4, M0.8): one machine hosts
    /// as the Nightmare, another connects as a Sleeper, over Unity Transport.
    /// A Host / Connect panel, or <c>--host</c> and <c>--connect</c> on the
    /// command line; Multiplayer Play Mode's virtual player presses Connect.
    /// Not a screen of docs/UI.md — a tool, like the gauntlet.
    /// </summary>
    public sealed class NetDreamScene : MonoBehaviour
    {
        [SerializeField] NetworkManager _manager;
        [SerializeField] GameObject _roundSyncPrefab;
        [SerializeField] DreamInstance _dream;
        [SerializeField] LocalRound _round;
        [SerializeField] GameObject _nightmareRig;
        [SerializeField] GameObject _nightmareHud;
        [SerializeField] UIDocument _panel;
        [SerializeField] InputActionAsset _actions;

        RoundSync _sync;
        DreamClient _client;
        InputActionAsset _own;
        Hello _hello;
        Label _status;

        public bool IsHosting { get; private set; }
        public bool IsConnecting { get; private set; }
        public RoundSync Sync => _sync;
        public DreamClient Client => _client;

        internal void Configure(NetworkManager manager, GameObject roundSyncPrefab, DreamInstance dream, LocalRound round,
            GameObject nightmareRig, GameObject nightmareHud, UIDocument panel, InputActionAsset actions)
        {
            _manager = manager; _roundSyncPrefab = roundSyncPrefab; _dream = dream; _round = round;
            _nightmareRig = nightmareRig; _nightmareHud = nightmareHud; _panel = panel; _actions = actions;
        }

        void Start()
        {
            GetComponent<EmptyDream>().Ensure();
            if (_nightmareRig != null) _nightmareRig.SetActive(false);
            if (_nightmareHud != null) _nightmareHud.SetActive(false);

            var registry = new CubeRegistry();
            _dream.Pack.RegisterAll(registry);
            _hello = NetSession.LocalHello(registry, SystemInfo.deviceName);

            // Scenes are ours to load; NGO synchronises none. The one network
            // prefab is registered here if the editor's default list (which
            // NGO fills with every NetworkObject prefab on import) has not.
            _manager.NetworkConfig.EnableSceneManagement = false;
            if (!_manager.NetworkConfig.Prefabs.Contains(_roundSyncPrefab))
                _manager.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = _roundSyncPrefab });

            if (_actions != null) _own = Instantiate(_actions);

            if (_panel != null && _panel.rootVisualElement != null)
            {
                VisualElement root = _panel.rootVisualElement;
                _status = root.Q<Label>("status");
                var address = root.Q<TextField>("address");
                root.Q<Button>("host").clicked += () => Host();
                root.Q<Button>("connect").clicked += () => Connect(address != null ? address.value : "127.0.0.1");
            }

            DevArgs args = DevArgs.Parse(Environment.GetCommandLineArgs());
            if (args.Host) Host(args.Port);
            else if (args.Connect != null) Connect(args.Connect, args.Port);
        }

        /// <summary>The Nightmare's machine: the round is here, and every event leaves from here.</summary>
        public bool Host(ushort port = NetSession.DefaultPort)
        {
            if (IsHosting || IsConnecting) return false;
            if (!NetSession.Host(_manager, _hello, port)) { Say("could not host"); return false; }
            IsHosting = true;

            GameObject go = Instantiate(_roundSyncPrefab);
            _sync = go.GetComponent<RoundSync>();
            go.GetComponent<NetworkObject>().Spawn();
            _round.EventAppended += _sync.Broadcast;
            _manager.OnClientConnectedCallback += OnClientConnected;

            if (_nightmareRig != null)
            {
                _nightmareRig.SetActive(true);
                var input = _nightmareRig.GetComponent<NightmareInputSource>();
                if (input != null && _own != null) input.Bind(_own);
            }
            if (_nightmareHud != null) _nightmareHud.SetActive(true);
            HidePanel();
            Say($"hosting on {port}; waiting for a Sleeper");
            return true;
        }

        void OnClientConnected(ulong clientId)
        {
            if (clientId == _manager.LocalClientId) return;
            if (_sync.RoundStarted)
            {
                // A second Sleeper, or a late joiner: M1.3's lobby and M1.4's late join.
                Debug.Log($"{name}: client {clientId} connected after the round began; nothing for them until M1.3", this);
                return;
            }
            // A fresh round now, not the one that has been ticking since the
            // scene loaded: RoundStart stamps t = 0 and the head start begins
            // for both ends together. Anything the host built while waiting
            // goes with the old round — a Sleeper who joins mid-lattice is
            // M1.4's catch-up, not this scene's.
            _round.Restart(_round.Round.Settings);
            _sync.BeginRound(_round.Round, _manager.LocalClientId, new[] { clientId });
            Say($"round begun from the bedroom; Sleeper is client {clientId}");
        }

        /// <summary>A Sleeper's machine: the dream is built from what the host says, and a body walks it.</summary>
        public bool Connect(string address, ushort port = NetSession.DefaultPort)
        {
            if (IsHosting || IsConnecting) return false;
            if (!NetSession.Connect(_manager, _hello, address, port)) { Say($"could not connect to {address}"); return false; }
            IsConnecting = true;
            HidePanel();
            Say($"connecting to {address}:{port}");
            return true;
        }

        void Update()
        {
            if (IsConnecting && _sync == null)
            {
                // The host's sync arrives as a spawned object; nothing announces it.
                foreach (RoundSync s in FindObjectsByType<RoundSync>(FindObjectsSortMode.None))
                {
                    if (!s.IsSpawned || s.NetworkManager != _manager) continue;
                    _sync = s;
                    _client = new DreamClient(_sync, _dream, SpawnSleeper);
                    Say("connected; waiting for the round");
                    break;
                }
            }
            _client?.Tick(Time.deltaTime);
        }

        SleeperMotor SpawnSleeper(Vector3 feet, Vector3 facing)
        {
            SleeperMotor motor = SleeperRig.Create(feet, facing);
            motor.transform.SetParent(transform, true);
            if (_own != null) motor.gameObject.AddComponent<SleeperInputSource>().Bind(_own);
            Say("in the dream");
            return motor;
        }

        void HidePanel()
        {
            if (_panel != null && _panel.rootVisualElement != null)
            {
                VisualElement form = _panel.rootVisualElement.Q("form");
                if (form != null) form.EnableInClassList("netdev--hidden", true);
            }
        }

        void Say(string what)
        {
            Debug.Log($"{name}: {what}", this);
            if (_status != null) _status.text = what;
        }

        void OnDestroy()
        {
            if (_round != null && _sync != null) _round.EventAppended -= _sync.Broadcast;
            if (_manager != null) _manager.OnClientConnectedCallback -= OnClientConnected;
            _client?.Dispose();
            if (_own != null) Destroy(_own);
        }
    }
}
