using System.Runtime.CompilerServices;

// Tests exercise internals directly; the public surface is what the rest of
// the game consumes. See docs/CORE-API.md for the intended split.
[assembly: InternalsVisibleTo("Lucid.Tests.PlayMode")]

// CubeBuilder generates CubeDefinition, DreamPack, Connector and FogDoor from
// cube.spec.json (docs/SPEC.md §17). Their Configure methods are internal so
// nothing at runtime can rewrite generated content, but the builder has to
// reach them, and it lives in Lucid.Editor.
[assembly: InternalsVisibleTo("Lucid.Editor")]
[assembly: InternalsVisibleTo("Lucid.Tests.EditMode")]
