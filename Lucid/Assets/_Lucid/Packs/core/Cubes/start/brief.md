# Bedroom

Where the dream starts, and the only cube that breaks the rules.

The start cube is fixed, exempt from every placement rule, and cannot be edited
(`docs/SPEC.md` §7). It has exactly one connector — the door — which is the only
place in the game where a single-connector cube is legal, because the rule that
every other cube needs two exists to stop the Nightmare sealing the dream, and
the bedroom is not something the Nightmare can build on top of.

Its door is misted for the head start, then drops and the Sleepers run
(`docs/SPEC.md` §11). Sleepers spawn here and respawn here when they lose a life,
so it wants to feel like somewhere you were safe a moment ago.

- **One connector**, north.
- **Interior 6 m wide**, wider than a corridor: it is a room, not a passage, and
  the contrast is the first thing a Sleeper sees.
- **Cost 0**, and category `start`, which is what exempts it in `Lucid.Core`.

No props yet. What makes it a bedroom rather than a cell is the skin, and skins
arrive in M1.10.
