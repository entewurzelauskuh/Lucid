# T Junction

Three ways out. The first cube that makes a Sleeper choose.

A T is worth more to the Nightmare than its geometry suggests. The explored rule
means a Sleeper who reaches a branch end after it has turned grey kills that
branch for good, so the Nightmare needs spare branch ends kept out of reach —
and every spare branch costs budget (`docs/SPEC.md` §7). A T is the cheapest way
to buy one.

It is also where the yo-yo lives: with two branches near maximum depth the
Nightmare can extend whichever the leading Sleeper is not in. Watch this cube in
the M0 play-test; if the yo-yo dominates, the counter is exit hysteresis, which
is a one-line change in `Lucid.Core`.

- **Connectors** south, east and west: in from the bottom, out to either side.
- **Three intended paths**, because all three crossings are legitimate and the
  validator should measure each.

Cost 1, like every connector in `docs/SPEC.md` §8. A junction is worth more to
the Nightmare than a corridor, so pricing it higher is tempting — but §8 prices
the whole connector set at 1, and the place to change that is M0.10 with
play-test evidence behind it, not a brief.
