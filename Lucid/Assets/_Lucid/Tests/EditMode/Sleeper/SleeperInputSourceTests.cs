using Lucid.Editor.Scenes;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lucid.Tests.EditMode.Sleeper
{
    /// <summary>
    /// The committed action asset, driving a real <see cref="SleeperInputSource"/>
    /// from a virtual keyboard. The gauntlet runs script input straight into
    /// <see cref="SleeperMotor.Tick"/>, which is what the M0.4 acceptance asks
    /// for but leaves the keyboard-and-mouse half of the deliverable
    /// unexercised: nothing else joins Sleeper.inputactions to a Sleeper.
    /// </summary>
    /// <remarks>
    /// <see cref="InputTestFixture"/> is doing real work here, not ceremony.
    /// The Input System ships set to ProcessEventsInDynamicUpdate, so actions
    /// are driven by the player loop, which does not run in EditMode: without
    /// the fixture the device state updates — the key really does read as
    /// pressed — while every action stays at zero.
    /// </remarks>
    public sealed class SleeperInputSourceTests : InputTestFixture
    {
        Keyboard _keyboard;
        GameObject _body;
        SleeperInputSource _source;

        [SetUp]
        public void BindASleeper()
        {
            _keyboard = InputSystem.AddDevice<Keyboard>();

            _body = new GameObject("Sleeper");
            _source = _body.AddComponent<SleeperInputSource>();
            _source.Bind(AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                GauntletSceneBuilder.InputActionsPath));
        }

        [TearDown]
        public void DestroyTheSleeper() => Object.DestroyImmediate(_body);

        [Test]
        public void NothingHeldIsNoIntent()
        {
            var input = _source.Read();

            Assert.That(input.Move, Is.EqualTo(Vector2.zero));
            Assert.That(input.Crouch, Is.False);
            Assert.That(input.JumpPressed, Is.False);
        }

        [Test]
        public void WalksOnTheWasdComposite()
        {
            Press(_keyboard.wKey);
            Assert.That(_source.Read().Move.y, Is.EqualTo(1f).Within(1e-3f));
            Release(_keyboard.wKey);

            Press(_keyboard.sKey);
            Assert.That(_source.Read().Move.y, Is.EqualTo(-1f).Within(1e-3f));
            Release(_keyboard.sKey);

            Press(_keyboard.dKey);
            Assert.That(_source.Read().Move.x, Is.EqualTo(1f).Within(1e-3f));
            Release(_keyboard.dKey);

            Press(_keyboard.aKey);
            Assert.That(_source.Read().Move.x, Is.EqualTo(-1f).Within(1e-3f));
        }

        [Test]
        public void CrouchIsHeldRatherThanToggled()
        {
            Press(_keyboard.leftCtrlKey);
            Assert.That(_source.Read().Crouch, Is.True);

            Release(_keyboard.leftCtrlKey);
            Assert.That(_source.Read().Crouch, Is.False, "crouch stuck on after release");
        }

        [Test]
        public void JumpFiresOnTheEdgeAndNotWhileHeld()
        {
            // SPEC §9 grants one jump from the ground and no double jump, so a
            // held key must not read as a new press every frame.
            Press(_keyboard.spaceKey);
            Assert.That(_source.Read().JumpPressed, Is.True);

            InputSystem.Update();
            Assert.That(_source.Read().JumpPressed, Is.False, "jump repeated while held");
        }

        [Test]
        public void LookComesFromTheMouseDelta()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            Set(mouse.delta, new Vector2(12f, -7f));

            var look = _source.Read().Look;

            Assert.That(look.x, Is.EqualTo(12f).Within(1e-3f));
            Assert.That(look.y, Is.EqualTo(-7f).Within(1e-3f));
        }

        [Test]
        public void AnUnboundSourceIsInertRatherThanBroken()
        {
            // A rig whose asset was never assigned should stand still, not
            // throw on the first frame of the round.
            var bare = new GameObject("Unbound");
            try
            {
                var input = bare.AddComponent<SleeperInputSource>().Read();
                Assert.That(input.Move, Is.EqualTo(Vector2.zero));
                Assert.That(input.JumpPressed, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(bare);
            }
        }
    }
}
