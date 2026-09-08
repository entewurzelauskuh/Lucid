using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lucid.Runtime
{
    /// <summary>
    /// The Sandbox (docs/UI.md §12): build as the Nightmare with nothing to
    /// spend or count down, F5 to drop into the lattice as a Sleeper, F5 again
    /// to come back to the god view, Esc to leave.
    /// </summary>
    /// <remarks>
    /// The Sleeper is created at run time rather than saved in the scene
    /// because its spawn point comes from the lattice, which does not exist
    /// until <see cref="EmptyDream"/> has built it. Reaching an exit in the
    /// Sandbox is not waking — there is no round to end and no results to
    /// show — so the Sleeper is put back in the bedroom to try the next
    /// route, and §14's line for it goes to the log (`docs/DECISIONS.md`).
    /// </remarks>
    public enum SandboxMode { Nightmare, Sleeper }

    [RequireComponent(typeof(EmptyDream))]
    public sealed class SandboxScene : MonoBehaviour
    {
        /// <summary>
        /// Not "UI": that is the map name the Input System's InputForUI bridge
        /// looks for on the project-wide asset to drive UI Toolkit, and a map
        /// by that name with only Back in it would, the day this asset became
        /// project-wide, silently take every click away from the Title.
        /// </summary>
        public const string BackMap = "Flow";
        public const string BackAction = "Back";
        public const string SwitchAction = "Switch";

        [SerializeField] InputActionAsset _actions;
        [SerializeField] GameObject _nightmareRig;
        [SerializeField] GameObject _nightmareHud;
        [SerializeField] SandboxMode _startIn = SandboxMode.Nightmare;

        InputActionAsset _own;
        InputAction _back;
        InputAction _switch;
        SleeperMotor _sleeper;
        DreamInstance _dream;

        public SleeperMotor Sleeper => _sleeper;

        /// <summary>Which of the two views drives the camera and the input right now.</summary>
        public SandboxMode Mode { get; private set; }

        public NightmareController Nightmare =>
            _nightmareRig != null ? _nightmareRig.GetComponent<NightmareController>() : null;

        /// <summary>The Back action as bound, for a test to inspect each link.</summary>
        internal InputAction BackInput => _back;

        /// <summary>The Switch action as bound, likewise.</summary>
        internal InputAction SwitchInput => _switch;

        /// <summary>How often a Sleeper has reached an exit here; each time they are back in the bedroom.</summary>
        public int ExitsReached { get; private set; }

        internal void Configure(InputActionAsset actions, GameObject nightmareRig, GameObject nightmareHud)
        {
            _actions = actions;
            _nightmareRig = nightmareRig;
            _nightmareHud = nightmareHud;
        }

        void Start()
        {
            _dream = GetComponent<EmptyDream>().Ensure();
            _dream.TouchedExit += OnReachedExit;

            if (_actions != null)
            {
                // A copy of the asset for this scene's lifetime, not the shared
                // asset itself. An InputActionAsset carries its resolved state
                // with it, so a map enabled here would still be enabled — and
                // still bound to whatever devices it last saw — after this
                // scene is gone. The dev scenes bind the shared asset directly
                // and that state was what the Esc test tripped over when they
                // ran first. A copy is born after everything else and dies
                // with the scene.
                _own = Instantiate(_actions);

                _back = _own.FindActionMap(BackMap, throwIfNotFound: false)
                    ?.FindAction(BackAction, throwIfNotFound: false);
                if (_back != null)
                {
                    _back.performed += OnBack;
                    _back.Enable();
                }

                _switch = _own.FindActionMap(BackMap, throwIfNotFound: false)
                    ?.FindAction(SwitchAction, throwIfNotFound: false);
                if (_switch != null)
                {
                    _switch.performed += OnSwitch;
                    _switch.Enable();
                }

                var nightmareInput = _nightmareRig != null ? _nightmareRig.GetComponent<NightmareInputSource>() : null;
                if (nightmareInput != null) nightmareInput.Bind(_own);
            }

            Enter(_startIn);
        }

        /// <summary>
        /// The Nightmare's side: the god view drives the camera, the HUD is up,
        /// and there is no Sleeper in the dream. docs/UI.md §12's default.
        /// </summary>
        public void EnterNightmare() => Enter(SandboxMode.Nightmare);

        /// <summary>The Sleeper's side: a body in the bedroom with the movement kit.</summary>
        public void EnterSleeper() => Enter(SandboxMode.Sleeper);

        /// <summary>F5: the other side (docs/UI.md §12).</summary>
        public void Toggle() => Enter(Mode == SandboxMode.Nightmare ? SandboxMode.Sleeper : SandboxMode.Nightmare);

        void Enter(SandboxMode mode)
        {
            Mode = mode;
            bool nightmare = mode == SandboxMode.Nightmare;

            // The god view's overlay — a selection in hand, the buildable
            // highlights — is the Nightmare's and never the Sleeper's.
            NightmareController controller = Nightmare;
            if (controller != null && !nightmare)
            {
                controller.Cancel();
                controller.ShowBuildable(false);
            }

            if (_nightmareRig != null) _nightmareRig.SetActive(nightmare);
            if (_nightmareHud != null) _nightmareHud.SetActive(nightmare);
            if (controller != null && nightmare) controller.ShowBuildable(true);

            if (nightmare)
            {
                if (_sleeper != null)
                {
                    Destroy(_sleeper.gameObject);
                    GetComponent<LocalRound>().SleeperLeft();
                }
                _sleeper = null;
            }
            else if (_sleeper == null)
            {
                DreamInstance dream = GetComponent<EmptyDream>().Dream;
                _sleeper = SleeperRig.Create(dream.SpawnPoint, dream.SpawnFacing);
                _sleeper.transform.SetParent(transform, true);
                if (_own != null) _sleeper.gameObject.AddComponent<SleeperInputSource>().Bind(_own);
            }
        }

        void OnDestroy()
        {
            if (_dream != null) _dream.TouchedExit -= OnReachedExit;
            if (_back != null)
            {
                _back.performed -= OnBack;
                _back.Disable();
            }
            if (_switch != null)
            {
                _switch.performed -= OnSwitch;
                _switch.Disable();
            }
            if (_own != null) Destroy(_own);
        }

        void OnBack(InputAction.CallbackContext _) => Back();

        void OnSwitch(InputAction.CallbackContext _)
        {
            if (Services.Current != null && Services.Current.Flow.IsTransitioning) return;
            Toggle();
        }

        /// <summary>
        /// The Sleeper walked into a white door. Not adjudicated as waking —
        /// Core would mark them Awake and the round over, and a Sandbox has
        /// neither — but back to the bedroom, so the next route can be tried
        /// without leaving the dream.
        /// </summary>
        void OnReachedExit(Lucid.Core.ConnectorRef door)
        {
            if (_sleeper == null) return;
            ExitsReached++;
            Debug.Log($"{name}: {Lucid.Runtime.UI.LucidStrings.ReachedTheExit(Lucid.Runtime.UI.LucidStrings.SleeperSeat(LocalRound.LocalSleeper + 1))} at {door}");
            _dream.Respawn(_sleeper);
        }

        /// <summary>
        /// Esc. With a cube selected it drops the selection (docs/UI.md §8:
        /// "Esc with no ghost active" is what opens the menu); otherwise it is
        /// the way back to the Title.
        /// </summary>
        public void Back()
        {
            NightmareController nightmare = Nightmare;
            if (Mode == SandboxMode.Nightmare && nightmare != null && nightmare.Cancel()) return;
            Leave();
        }

        /// <summary>Back to the Title. Public so a test can press Esc without a keyboard.</summary>
        public void Leave()
        {
            Services services = Services.Current;
            if (services == null)
                throw new InvalidOperationException(
                    $"{name}: no Services — the Dream scene was loaded without the Boot scene");
            if (!services.Flow.IsTransitioning) services.Flow.Go(FlowState.Title);
        }
    }
}
