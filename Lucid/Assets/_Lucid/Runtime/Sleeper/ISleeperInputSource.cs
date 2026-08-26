namespace Lucid.Runtime
{
    /// <summary>
    /// Where a <see cref="SleeperMotor"/> gets its intent. Implemented by
    /// <see cref="SleeperInputSource"/> for a human, and by the PlayMode tests
    /// for a scripted run.
    /// </summary>
    public interface ISleeperInputSource
    {
        SleeperInput Read();
    }
}
