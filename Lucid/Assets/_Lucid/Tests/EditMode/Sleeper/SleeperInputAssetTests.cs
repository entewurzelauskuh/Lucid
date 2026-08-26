using System.Linq;
using Lucid.Editor.Scenes;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Lucid.Tests.EditMode.Sleeper
{
    /// <summary>
    /// The <c>Sleeper</c> action map. M0.4 is keyboard and mouse only
    /// (docs/WORKPLAN.md §4); the gamepad SPEC §9 promises comes later, and
    /// this notices when it arrives without its own tests.
    /// </summary>
    public sealed class SleeperInputAssetTests
    {
        static InputActionAsset Asset =>
            AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                GauntletSceneBuilder.InputActionsPath);

        [Test]
        public void TheAssetIsWhereTheSceneBuilderLooksForIt()
        {
            Assert.That(Asset, Is.Not.Null,
                $"no InputActionAsset at {GauntletSceneBuilder.InputActionsPath}");
        }

        [Test]
        public void TheSleeperMapBindsTheWholeKit()
        {
            var map = Asset.FindActionMap(SleeperInputSource.MapName, throwIfNotFound: false);
            Assert.That(map, Is.Not.Null, $"no '{SleeperInputSource.MapName}' map");

            foreach (string name in new[] { "Move", "Look", "Jump", "Crouch" })
            {
                var action = map.FindAction(name);
                Assert.That(action, Is.Not.Null, $"the map has no '{name}' action");
                Assert.That(action.bindings.Count, Is.GreaterThan(0), $"'{name}' is unbound");
            }
        }

        [Test]
        public void EveryBindingIsKeyboardOrMouse()
        {
            var map = Asset.FindActionMap(SleeperInputSource.MapName, throwIfNotFound: false);
            Assert.That(map, Is.Not.Null);

            var strays = map.bindings
                .Where(b => !b.isComposite)
                .Select(b => b.path)
                .Where(p => !p.StartsWith("<Keyboard>") && !p.StartsWith("<Mouse>"))
                .ToArray();

            Assert.That(strays, Is.Empty,
                "M0.4 is keyboard and mouse only; unexpected: " + string.Join(", ", strays));
        }
    }
}
