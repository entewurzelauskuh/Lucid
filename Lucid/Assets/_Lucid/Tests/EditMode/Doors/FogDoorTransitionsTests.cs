using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Doors
{
    /// <summary>
    /// docs/SPEC.md §7's transition list, every pair of it. Four states make
    /// sixteen ordered pairs, and all sixteen are named here rather than
    /// sampled, because the rules that matter are the ones the list leaves
    /// out.
    /// </summary>
    public sealed class FogDoorTransitionsTests
    {
        static FogDoorTransition For(ConnectorState from, ConnectorState to) =>
            FogDoorTransitions.For(from, to);

        [Test]
        public void AStateThatDidNotChangePlaysNothing()
        {
            foreach (ConnectorState state in System.Enum.GetValues(typeof(ConnectorState)))
                Assert.That(For(state, state), Is.EqualTo(FogDoorTransition.None), state.ToString());
        }

        [Test]
        public void TheExitMovesBothWays()
        {
            // "Fog ↔ Exit whenever the depth ranking changes."
            Assert.That(For(ConnectorState.Fog, ConnectorState.Exit),
                Is.EqualTo(FogDoorTransition.Kindle));
            Assert.That(For(ConnectorState.Exit, ConnectorState.Fog),
                Is.EqualTo(FogDoorTransition.Dim));
        }

        [Test]
        public void BuildingOnADoorDissolvesItWhicheverItWas()
        {
            // "Fog or Exit → Attached when the Nightmare builds on it."
            Assert.That(For(ConnectorState.Fog, ConnectorState.Attached),
                Is.EqualTo(FogDoorTransition.Dissolve));
            Assert.That(For(ConnectorState.Exit, ConnectorState.Attached),
                Is.EqualTo(FogDoorTransition.Dissolve));
        }

        [Test]
        public void ExploringHardensFog()
        {
            Assert.That(For(ConnectorState.Fog, ConnectorState.Solid),
                Is.EqualTo(FogDoorTransition.Condense));
        }

        [Test]
        public void AnExitNeverHardens()
        {
            // The rule that stops a Sleeper sealing their own way out: entering
            // a cube solidifies its fog doors, and if the exit were among them
            // the deepest door would close behind the person heading for it.
            Assert.That(For(ConnectorState.Exit, ConnectorState.Solid),
                Is.EqualTo(FogDoorTransition.Forbidden));
        }

        [Test]
        public void SolidIsForGood()
        {
            // "The Nightmare has lost those connectors for good."
            foreach (ConnectorState to in new[]
                     { ConnectorState.Fog, ConnectorState.Exit, ConnectorState.Attached })
                Assert.That(For(ConnectorState.Solid, to),
                    Is.EqualTo(FogDoorTransition.Forbidden), $"Solid → {to}");
        }

        [Test]
        public void AnAttachedDoorHasNoMistLeftToChange()
        {
            foreach (ConnectorState to in new[]
                     { ConnectorState.Fog, ConnectorState.Exit, ConnectorState.Solid })
                Assert.That(For(ConnectorState.Attached, to),
                    Is.EqualTo(FogDoorTransition.Forbidden), $"Attached → {to}");
        }

        [Test]
        public void EverySixteenPairsIsAccountedFor()
        {
            // Guards the table against a state being added to Core without
            // anyone deciding what the door should do about it.
            var seen = new List<string>();
            foreach (ConnectorState from in System.Enum.GetValues(typeof(ConnectorState)))
            foreach (ConnectorState to in System.Enum.GetValues(typeof(ConnectorState)))
                seen.Add($"{from}->{to}={For(from, to)}");

            Assert.That(seen, Has.Count.EqualTo(16),
                "ConnectorState gained a value; docs/SPEC.md §7 needs a row for it");
        }

        [Test]
        public void OnlyAnOpeningOrTheLightLetsASleeperThrough()
        {
            Assert.That(FogDoorTransitions.IsPassable(ConnectorState.Attached), Is.True);
            Assert.That(FogDoorTransitions.IsPassable(ConnectorState.Exit), Is.True);
            Assert.That(FogDoorTransitions.IsPassable(ConnectorState.Fog), Is.False);
            Assert.That(FogDoorTransitions.IsPassable(ConnectorState.Solid), Is.False);
        }

        [Test]
        public void OnlyTheLightWakes()
        {
            // Attached is the interesting one: it is passable, like Exit, so a
            // check that asked "did the Sleeper pass through?" would wake
            // people walking between ordinary rooms.
            Assert.That(FogDoorTransitions.Wakes(ConnectorState.Exit), Is.True);
            foreach (ConnectorState state in new[]
                     { ConnectorState.Attached, ConnectorState.Fog, ConnectorState.Solid })
                Assert.That(FogDoorTransitions.Wakes(state), Is.False, state.ToString());
        }
    }
}
