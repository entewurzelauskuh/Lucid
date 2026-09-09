using System.Runtime.CompilerServices;

// Tests exercise internals directly; the public surface is what the rest of
// the game consumes. See docs/NETCODE.md §13 for the intended split.
[assembly: InternalsVisibleTo("Lucid.Tests.PlayMode")]
[assembly: InternalsVisibleTo("Lucid.Tests.EditMode")]
[assembly: InternalsVisibleTo("Lucid.Editor")]
