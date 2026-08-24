using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Lucid.Tests.EditMode
{
    /// <summary>
    /// Guards the one structural rule that cannot be recovered from later:
    /// Lucid.Core must never gain a Unity dependency (CLAUDE.md rule 3,
    /// docs/CORE-API.md). The asmdef sets noEngineReferences, which makes the
    /// engine unavailable at compile time; this test fails loudly if someone
    /// clears that flag, which a compiler error alone would not explain.
    /// </summary>
    public sealed class AssemblyBoundaryTests
    {
        static Assembly Core =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(a => a.GetName().Name == "Lucid.Core");

        [Test]
        public void CoreAssemblyExists()
        {
            Assert.That(Core, Is.Not.Null,
                "Lucid.Core is not loaded. Does Assets/_Lucid/Core/Lucid.Core.asmdef still compile?");
        }

        [Test]
        public void CoreReferencesNoUnityAssemblies()
        {
            var offenders = Core.GetReferencedAssemblies()
                .Select(a => a.Name)
                .Where(n => n.StartsWith("UnityEngine", StringComparison.Ordinal)
                         || n.StartsWith("UnityEditor", StringComparison.Ordinal))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Lucid.Core is pure C# and must not reference Unity. Offending references: "
                + string.Join(", ", offenders));
        }
    }
}
