// Lucid — the copy glossary, verbatim.
//
// docs/UI.md §14: "Strings are the rules made visible; keep them exact so every
// screen says the same thing." Every player-facing string in the UI comes from
// here. Do not inline a literal in a screen, do not reword per screen, and do
// not invent a new one without adding it to UI.md §14 first.
//
// MIT.

using System;

namespace Lucid.Runtime.UI
{
    public static class LucidStrings
    {
        // --- Phase ----------------------------------------------------------------
        public static string SleepersStirIn(TimeSpan t) => $"The Sleepers stir in {Clock(t)}";
        public const  string SleepersRunning = "The Sleepers are running";
        public static string DawnIn(TimeSpan t)         => $"Dawn in {Clock(t)}";
        public const  string Dawn = "Dawn.";

        // --- Sleeper events -------------------------------------------------------
        public static string WokeUp(string name)        => $"{name} woke up";
        public static string WasConsumed(string name)   => $"{name} was consumed";
        public static string LostALife(int moonsLeft)   => $"You lost a life — {moonsLeft} moons left";
        public static string YouWokeUp(TimeSpan at)     => $"You woke up — {Clock(at)}";
        public const  string Consumed = "Consumed";

        // --- Doors, Sleeper side --------------------------------------------------
        public const string ExitMoved   = "The exit moved";
        public const string DoorHardened = "A door hardened";

        // --- Doors, Nightmare side ------------------------------------------------
        public static string HardenedDoors(string name, int n, string cube) => $"{name} hardened {n} doors in the {cube}";
        public static string ReachedTheExit(string name) => $"{name} reached the exit";

        // --- Placement rejections -------------------------------------------------
        public const  string DoorIsSolid  = "Door is solid";
        public const  string DoesntFit    = "Doesn't fit here";
        public static string WouldTrap(string name)         => $"Would trap {name}";
        public static string NotEnoughBudget(int have, int cost) => $"Not enough budget ({have} / {cost})";
        public const  string NotADoor     = "Not a door";

        // --- Effects --------------------------------------------------------------
        public const string EffectDark     = "Dark";
        public const string EffectFog      = "Fog";
        public const string EffectMolasses = "Molasses — don't jump";

        // --- Possession -----------------------------------------------------------
        public static string Possessing(string mob, string dreamOwner) => $"Possessing the {mob} in {dreamOwner}'s dream — P to let go";
        public const  string YourBodyDied = "Your body died";
        public const  string BuildingPaused = "Building paused";

        // --- Lobby ----------------------------------------------------------------
        public const  string NobodyPickedNightmare = "Nobody picked Nightmare";
        public const  string NeedASleeper          = "Need at least one Sleeper";
        public static string WaitingToReady(string name) => $"Waiting for {name} to ready up";
        public static string TonightsNightmare(string name) => $"Tonight's Nightmare is… {name}";
        public static string WantToBeNightmare(int n) => $"{n} players want to be the Nightmare — one will be chosen at random.";
        public const  string FirstDreamBegins = "The first dream begins.";

        // --- Results (UI.md §9: exactly three titles) -----------------------------
        public const string ResultEveryoneWoke = "Everyone woke up";
        public const string ResultDawn         = "Dawn.";
        public const string ResultConsumed     = "Consumed";
        public static string WokeAt(TimeSpan t)     => $"Woke at {Clock(t)}";
        public static string ConsumedAt(TimeSpan t) => $"Consumed at {Clock(t)}";
        public const  string ConsumedByDawn = "Consumed by dawn";

        // --- Spectator ------------------------------------------------------------
        public const string YoureAwake = "You're awake. Watch the others.";

        // --- Edge flows -----------------------------------------------------------
        public const  string DreamCollapsed = "The dream collapsed";
        public const  string NightmareFled  = "The Nightmare fled";
        public const  string DreamWillCollapse = "The dream will collapse for all Sleepers.";
        public static string RoundInProgress(TimeSpan left) => $"Round in progress, {Clock(left)} left";
        public const  string SteamOffline = "Steam is offline";

        // --- Readouts ------------------------------------------------------------
        public static string Build(string version, string engine) => $"{version} · Unity {engine}";
        /// <summary>"1 per {n} s" from the interval in milliseconds: whole seconds, or one decimal when it is not whole.</summary>
        public static string Trickle(int intervalMs) =>
            intervalMs % 1000 == 0
                ? $"1 per {intervalMs / 1000} s"
                : $"1 per {(intervalMs / 1000f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)} s";
        /// <summary>A Sleeper with no name yet, by seat (docs/UI.md §14): the Sandbox's own.</summary>
        public static string SleeperSeat(int n) => $"Sleeper {n}";

        // --- The one clock format -------------------------------------------------
        // Always m:ss. Tabular numerals in USS, so a ticking value never shifts.
        public static string Clock(TimeSpan t)
        {
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            return $"{(int)t.TotalMinutes}:{t.Seconds:00}";
        }
    }
}
