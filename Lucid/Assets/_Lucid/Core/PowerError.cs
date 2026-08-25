namespace Lucid.Core
{
    /// <summary>Why a power was refused (docs/CORE-API.md §9).</summary>
    public enum PowerError : byte
    {
        None = 0,
        OnCooldown = 1,
        NotEnoughBudget = 2,
        NoSuchDream = 3,
        Possessed = 4,
        Disabled = 5,
    }
}
