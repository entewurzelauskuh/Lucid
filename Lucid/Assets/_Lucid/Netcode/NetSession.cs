using System;
using Lucid.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Lucid.Netcode
{
    /// <summary>
    /// The developer's door into a session (docs/SPEC.md §14, docs/NETCODE.md
    /// §1): Unity Transport on an address and a port, a Hello in the
    /// connection data, and approval on the host. M1.2 adds the Steam
    /// transport beside it; this stays for contributors and for LAN builds.
    /// </summary>
    public static class NetSession
    {
        public const ushort DefaultPort = 7777;
        public const int Capacity = 5;   // one Nightmare and four Sleepers

        /// <summary>What this build says of itself: its version and its content.</summary>
        public static Hello LocalHello(CubeRegistry registry, string displayName, ulong steamId = 0) =>
            new Hello(Application.version, registry.ContentHash(), steamId, displayName, Hello.CurrentProtocol);

        public static UnityTransport Transport(NetworkManager manager)
        {
            var utp = manager.GetComponent<UnityTransport>();
            if (utp == null) utp = manager.gameObject.AddComponent<UnityTransport>();
            manager.NetworkConfig.NetworkTransport = utp;
            return utp;
        }

        /// <summary>
        /// Arms the host's approval (§2): every Hello is judged against what
        /// this build expects, and the seats it has left.
        /// </summary>
        public static void ArmApproval(NetworkManager manager, Hello local)
        {
            manager.NetworkConfig.ConnectionApproval = true;
            manager.ConnectionApprovalCallback = (request, response) =>
            {
                // The host approves itself: its own Hello is by definition the expectation.
                bool self = request.ClientNetworkId == NetworkManager.ServerClientId;
                Decision d = self
                    ? Decision.Yes
                    : Approval.Decide(Hello.FromBytes(request.Payload),
                        new Expectation(local.BuildId, local.ContentHash, Capacity, manager.ConnectedClientsIds.Count));
                response.Approved = d.Approved;
                response.Reason = d.Reason;
                response.CreatePlayerObject = false;
                response.Pending = false;
            };
        }

        /// <summary>
        /// A client's side of §2: its Hello in the connection data, and
        /// approval switched on — NGO's connection request carries the payload
        /// only when both ends agree that it does, and a client without the
        /// flag is refused for a config mismatch before the host ever reads a
        /// byte of it.
        /// </summary>
        public static void PrepareClient(NetworkManager manager, Hello local)
        {
            manager.NetworkConfig.ConnectionApproval = true;
            manager.NetworkConfig.ConnectionData = local.ToBytes();
        }

        public static bool Host(NetworkManager manager, Hello local, ushort port = DefaultPort)
        {
            // forceOverrideCommandLineArgs: UTP reads -ip and -port itself and
            // would otherwise win over --connect and --port.
            Transport(manager).SetConnectionData(true, "0.0.0.0", port, "0.0.0.0");
            manager.NetworkConfig.ConnectionData = local.ToBytes();
            ArmApproval(manager, local);
            return manager.StartHost();
        }

        public static bool Connect(NetworkManager manager, Hello local, string address, ushort port = DefaultPort)
        {
            if (string.IsNullOrEmpty(address)) throw new ArgumentException("an address to connect to", nameof(address));
            Transport(manager).SetConnectionData(true, address, port);
            PrepareClient(manager, local);
            return manager.StartClient();
        }
    }

    /// <summary>
    /// The command line a dev build reads: <c>--host</c>, or <c>--connect &lt;address&gt;</c>,
    /// with <c>--port &lt;n&gt;</c> for either. Permanent (docs/DECISIONS.md): it is how a
    /// contributor and a LAN play-test get in without Steam, and M1.2's
    /// transport switch sits beside it rather than replacing it.
    /// </summary>
    public sealed record DevArgs(bool Host, string Connect, ushort Port)
    {
        public bool Any => Host || Connect != null;

        public static DevArgs Parse(string[] args)
        {
            bool host = false; string connect = null; ushort port = NetSession.DefaultPort;
            if (args == null) return new DevArgs(false, null, port);
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--host": host = true; break;
                    case "--connect": if (i + 1 < args.Length) connect = args[++i]; break;
                    case "--port": if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort p)) { port = p; i++; } break;
                }
            }
            return new DevArgs(host, connect, port);
        }
    }
}
