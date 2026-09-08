using Lucid.Runtime;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Flow
{
    /// <summary>
    /// The screen map of docs/UI.md §2, as far as M0 has it, held as a table.
    /// </summary>
    public sealed class FlowTableTests
    {
        [Test]
        public void TheOfflineLoopIsBootTitleSandboxTitle()
        {
            Assert.That(FlowTable.Allows(FlowState.Boot, FlowState.Title), Is.True);
            Assert.That(FlowTable.Allows(FlowState.Title, FlowState.Sandbox), Is.True);
            Assert.That(FlowTable.Allows(FlowState.Sandbox, FlowState.Title), Is.True);
        }

        [Test]
        public void NothingElseIsAllowed()
        {
            // Boot goes to the Title and nowhere else; nothing goes back to Boot;
            // and a state never transitions to itself — "Title → Title" would be
            // a reload nobody asked for.
            Assert.That(FlowTable.Allows(FlowState.Boot, FlowState.Sandbox), Is.False);
            Assert.That(FlowTable.Allows(FlowState.Title, FlowState.Boot), Is.False);
            Assert.That(FlowTable.Allows(FlowState.Sandbox, FlowState.Boot), Is.False);
            foreach (FlowState s in new[] { FlowState.Boot, FlowState.Title, FlowState.Sandbox })
                Assert.That(FlowTable.Allows(s, s), Is.False, s.ToString());
        }

        [Test]
        public void EveryStateButBootLivesInAScene()
        {
            Assert.That(FlowTable.SceneOf(FlowState.Boot), Is.Null, "Boot is the scene that is always there");
            Assert.That(FlowTable.SceneOf(FlowState.Title), Is.EqualTo("Title"));
            Assert.That(FlowTable.SceneOf(FlowState.Sandbox), Is.EqualTo("Dream"));
        }
    }
}
