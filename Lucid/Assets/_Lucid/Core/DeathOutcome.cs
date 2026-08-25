namespace Lucid.Core
{
    public enum DeathOutcome : byte
    {
        LostLife = 0,
        Consumed = 1,

        /// <summary>
        /// The Sleeper was not in the dream. Not in docs/CORE-API.md §8's list,
        /// which assumes a live Sleeper; a death arriving just after a wake is
        /// ordinary on a 10 Hz link, so the host needs an answer that is not a
        /// lost life.
        /// </summary>
        Ignored = 2,
    }
}
