using System.IO;
using Lucid.Editor.Scenes;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>
    /// The committed network prefab, as bytes on disk: loading it in an editor
    /// would compute the hash in memory and hide a zero in the file, and a
    /// zero in the file is a build that no editor host will accept.
    /// </summary>
    public sealed class NetPrefabTests
    {
        [Test]
        public void TheCommittedPrefabCarriesItsGlobalObjectIdHash()
        {
            string yaml = File.ReadAllText(NetPrefabs.RoundSyncPath);
            Assert.That(yaml, Does.Match(@"\n  GlobalObjectIdHash: [1-9]\d*\n"), "the prefab was committed before NGO hashed it, or has no NetworkObject");
            Assert.That(yaml, Does.Contain("_pack: {fileID:"), "the prefab carries no pack");
        }
    }
}
