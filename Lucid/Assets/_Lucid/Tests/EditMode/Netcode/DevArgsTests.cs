using Lucid.Netcode;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    public sealed class DevArgsTests
    {
        [Test]
        public void TheThreeFlagsAndTheirAbsence()
        {
            Assert.That(DevArgs.Parse(new[] { "Lucid.exe" }).Any, Is.False);
            Assert.That(DevArgs.Parse(null).Port, Is.EqualTo(NetSession.DefaultPort));

            DevArgs host = DevArgs.Parse(new[] { "Lucid.exe", "--host", "--port", "7800" });
            Assert.That((host.Host, host.Connect, host.Port), Is.EqualTo((true, (string)null, (ushort)7800)));

            DevArgs client = DevArgs.Parse(new[] { "--connect", "192.168.1.20" });
            Assert.That((client.Host, client.Connect, client.Port), Is.EqualTo((false, "192.168.1.20", NetSession.DefaultPort)));

            Assert.That(DevArgs.Parse(new[] { "--connect" }).Connect, Is.Null, "a flag with no value is no address");
            Assert.That(DevArgs.Parse(new[] { "--port", "notanumber", "--host" }).Host, Is.True, "a bad port does not eat the next flag");
        }
    }
}
