using System;
using System.Collections.Generic;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, "Registry".</summary>
    public sealed class CubeRegistryTests
    {
        [Test]
        public void RejectsANonStartTypeWithOneConnector()
        {
            var reg = new CubeRegistry();
            // A one-connector room would be a dead end the Nightmare could seal
            // the dream with (docs/SPEC.md §7).
            Assert.That(() => reg.Register(
                    new CubeType("core.deadend", "core", CubeCategory.Chicane, FaceMask.North, false, 1)),
                Throws.ArgumentException);
        }

        [Test]
        public void RejectsATypeWithNoConnectors()
        {
            var reg = new CubeRegistry();
            Assert.That(() => reg.Register(
                    new CubeType("core.sealed", "core", CubeCategory.Chicane, FaceMask.None, false, 1)),
                Throws.ArgumentException);
        }

        [Test]
        public void AcceptsTheStartTypeWithExactlyOneConnector()
        {
            var reg = new CubeRegistry();
            Assert.That(() => reg.Register(
                    new CubeType("core.start", "core", CubeCategory.Start, FaceMask.North, false, 0)),
                Throws.Nothing);
            Assert.That(reg.Get("core.start").Category, Is.EqualTo(CubeCategory.Start));
        }

        [Test]
        public void RejectsAStartTypeWithMoreThanOneConnector()
        {
            var reg = new CubeRegistry();
            Assert.That(() => reg.Register(new CubeType(
                    "core.start", "core", CubeCategory.Start,
                    FaceMask.North | FaceMask.South, false, 0)),
                Throws.ArgumentException);
        }

        [Test]
        public void RejectsADuplicateId()
        {
            CubeRegistry reg = TestLattice.Registry();
            Assert.That(() => reg.Register(new CubeType(
                    TestLattice.Straight, "core", CubeCategory.Connector,
                    FaceMask.North | FaceMask.South, false, 1)),
                Throws.ArgumentException);
        }

        [Test]
        public void GetThrowsForAnUnknownId()
        {
            CubeRegistry reg = TestLattice.Registry();
            Assert.That(() => reg.Get("core.nope"), Throws.TypeOf<KeyNotFoundException>());
            Assert.That(reg.TryGet("core.nope", out _), Is.False);
            Assert.That(reg.Contains(TestLattice.Straight), Is.True);
        }

        [Test]
        public void ContentHashFollowsRegistrationOrder()
        {
            // Type indices on the wire are registration order, so a registry
            // built in a different order is a different registry
            // (docs/NETCODE.md §4, §5).
            var a = new CubeRegistry();
            a.Register(new CubeType("x", "core", CubeCategory.Connector, FaceMask.North | FaceMask.South, false, 1));
            a.Register(new CubeType("y", "core", CubeCategory.Connector, FaceMask.North | FaceMask.East, false, 1));

            var b = new CubeRegistry();
            b.Register(new CubeType("y", "core", CubeCategory.Connector, FaceMask.North | FaceMask.East, false, 1));
            b.Register(new CubeType("x", "core", CubeCategory.Connector, FaceMask.North | FaceMask.South, false, 1));

            Assert.That(a.ContentHash(), Is.Not.EqualTo(b.ContentHash()));
            Assert.That(a.ContentHash(), Is.EqualTo(a.ContentHash()));
        }

        [Test]
        public void LatticeRefusesToStartOnANonStartCube()
        {
            CubeRegistry reg = TestLattice.Registry();
            Assert.That(() => Lattice.New(reg, TestLattice.Straight, Rotation.R0),
                Throws.ArgumentException);
        }
    }
}
