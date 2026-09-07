using System.Linq;
using Lucid.Editor.Scenes;
using NUnit.Framework;
using UnityEditor;

namespace Lucid.Tests.EditMode.Flow
{
    /// <summary>
    /// The build list the flow loads scenes from. Boot must be first — a
    /// player build starts on scene 0, and without Boot there are no Services,
    /// so the first click on the Title throws — and no two scenes may share a
    /// name, because the flow loads by name and takes the first match.
    /// </summary>
    public sealed class BuildListTests
    {
        [Test]
        public void BootIsFirstAndTheFlowScenesFollow()
        {
            string[] enabled = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            Assert.That(enabled, Is.EqualTo(FlowSceneBuilders.BuildList),
                "the committed EditorBuildSettings does not match the generator; run tools/build-scenes.sh");
            Assert.That(enabled[0], Is.EqualTo(FlowSceneBuilders.BootPath));
        }

        [Test]
        public void NoTwoScenesShareAName()
        {
            string[] names = FlowSceneBuilders.BuildList
                .Select(System.IO.Path.GetFileNameWithoutExtension).ToArray();
            Assert.That(names, Is.Unique);
        }
    }
}
