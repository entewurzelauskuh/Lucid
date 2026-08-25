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
        public void ARoomNarrowerThanADoorwayIsImpossible()
        {
            // Cubes join at the standard positions whatever their interior, so
            // the doorway cannot be wider than the room it opens into.
            Assert.That(CubeGeometry.DoorwayFits(
                new ShellSpec { Interior = new InteriorSpec { Width = 4f } }), Is.True);
            Assert.That(CubeGeometry.DoorwayFits(
                new ShellSpec { Interior = new InteriorSpec { Width = 2.5f } }), Is.False);
        }
    }
}
