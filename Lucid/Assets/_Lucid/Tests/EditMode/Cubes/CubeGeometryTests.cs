using Lucid.Core;
using Lucid.Editor.Cubes;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The frame from docs/CUBE-SPEC.md §1. These numbers are what makes two
    /// cubes join, so they are pinned literally rather than derived.
    /// </summary>
    public sealed class CubeGeometryTests
    {
        [Test]
        public void TheCubeIsEightMetresWithDoorwaysTwoAndAHalfByThree()
        {
            Assert.That(CubeGeometry.Size, Is.EqualTo(8f));
            Assert.That(CubeGeometry.DoorWidth, Is.EqualTo(2.5f));
            Assert.That(CubeGeometry.DoorHeight, Is.EqualTo(3f));
            Assert.That(CubeGeometry.VerticalHole, Is.EqualTo(2.5f));
        }

        [Test]
        public void FaceCentresAreWhereTheGuideSaysTheyAre()
        {
            Assert.That(CubeGeometry.Centre(Face.North), Is.EqualTo(new Vector3(0, 0, 4)));
            Assert.That(CubeGeometry.Centre(Face.East), Is.EqualTo(new Vector3(4, 0, 0)));
            Assert.That(CubeGeometry.Centre(Face.South), Is.EqualTo(new Vector3(0, 0, -4)));
            Assert.That(CubeGeometry.Centre(Face.West), Is.EqualTo(new Vector3(-4, 0, 0)));
            Assert.That(CubeGeometry.Centre(Face.Up), Is.EqualTo(new Vector3(0, 8, 0)));
            Assert.That(CubeGeometry.Centre(Face.Down), Is.EqualTo(new Vector3(0, 0, 0)));
        }

        [Test]
        public void ANarrowerRoomThickensTheWallsRatherThanShrinkingTheCube()
        {
            // "A 4 m wide corridor inside the 8 m cube", not a 4 m cube
            // (docs/CUBE-SPEC.md §3).
            var corridor = new ShellSpec { Interior = new InteriorSpec { Width = 4f } };
            Assert.That(CubeGeometry.InteriorHalf(corridor), Is.EqualTo(2f));

            var plain = new ShellSpec();
            Assert.That(CubeGeometry.InteriorHalf(plain), Is.EqualTo(4f - CubeGeometry.DefaultThickness));
        }

        [Test]
        public void TheCeilingStopsShortOfTheTopSoTheCubeAboveHasRoom()
        {
            // Running the ceiling to y = 8 put it in the same volume as the
            // floor slab of the cube stacked above: duplicated solid boxes and
            // two overlapping colliders at every vertical join.
            var plain = new ShellSpec();
            Assert.That(CubeGeometry.CeilingTop(plain),
                Is.EqualTo(CubeGeometry.Size - CubeGeometry.DefaultThickness));

            // The cube occupies exactly its 8 m: floor at [-t, 0], ceiling top
            // at 8 - t, so a cube one layer up abuts rather than overlaps.
            float occupied = CubeGeometry.CeilingTop(plain) + CubeGeometry.DefaultThickness;
            Assert.That(occupied, Is.EqualTo(CubeGeometry.Size).Within(1e-4f));

            // And the default interior leaves the ceiling one thickness deep,
            // the same way it leaves a wall on each side.
            Assert.That(CubeGeometry.CeilingTop(plain) - CubeGeometry.InteriorHeight(plain),
                Is.EqualTo(CubeGeometry.DefaultThickness).Within(1e-4f));
        }

        [Test]
        public void TheLimitsLeaveRoomForAWallAndACeiling()
        {
            // The schema permits width and height up to 8, which would leave
            // no wall and no ceiling at all. These are the geometric limits it
            // cannot express, because they depend on thickness.
            var plain = new ShellSpec();
            Assert.That(CubeGeometry.MaxInteriorWidth(plain),
                Is.EqualTo(8f - 2f * CubeGeometry.DefaultThickness).Within(1e-4f));
            Assert.That(CubeGeometry.MaxInteriorHeight(plain),
                Is.EqualTo(8f - 2f * CubeGeometry.DefaultThickness).Within(1e-4f));

            var thick = new ShellSpec { Thickness = 1f };
            Assert.That(CubeGeometry.MaxInteriorWidth(thick), Is.EqualTo(6f).Within(1e-4f));
        }

    }
}
