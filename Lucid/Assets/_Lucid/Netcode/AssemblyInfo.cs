using System.Runtime.CompilerServices;

// Tests exercise internals directly; the public surface is what the rest of
// the game consumes. See docs/CORE-API.md for the intended split.
[assembly: InternalsVisibleTo("Lucid.Tests.PlayMode")]
