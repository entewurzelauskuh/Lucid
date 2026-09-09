namespace Lucid.Netcode
{
    /// <summary>
    /// docs/NETCODE.md §12, verbatim: the protocol-scoped identifier of every
    /// message, used in logs. Adding one appends to its hundred block and
    /// never renumbers.
    /// </summary>
    public enum Message : ushort
    {
        Hello = 1, SessionState = 2, RoleSelect = 3, ReadySet = 4, RoundInProgress = 5,
        RoundStart = 101, PhaseChanged = 102, SleeperStatus = 103, RoundEnded = 104, ReturnToLobby = 105,
        LatticeEvent = 201, HashReport = 202, DesyncNotice = 203, ResumeRequest = 204, ResumeSnapshot = 205,
        DreamReady = 301, Telemetry = 302, Explored = 303, TouchedExit = 304, Died = 305, ChicaneDelta = 306,
        WakeVerdict = 311, DeathVerdict = 312, TriggerFire = 313, EffectStart = 314, ViewSubscribers = 315,
        PossessionBegin = 316, PossessionEnd = 317,
        PlaceRequest = 401, PlaceReply = 402, PowerRequest = 403, PowerReply = 404, PossessRequest = 405,
        PossessReply = 406, ReleaseRequest = 407, BudgetState = 408, Markers = 409, ChicaneSummary = 410,
        SubscribeDream = 501, DreamView = 502, PossessInput = 503, PossessedHit = 504,
        ReplayRequest = 601, ReplayChunk = 602,
    }
}
