using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Sleeper
{
    /// <summary>
    /// Mouse look: yaw on the body, pitch on the eye, clamped. Half of "first
    /// person" (docs/WORKPLAN.md §4) and pure arithmetic, so it is checked here
    /// rather than in PlayMode.
    /// </summary>
    public sealed class SleeperLookTests
    {
        GameObject _body;
        SleeperLook _look;
        Transform _eye;

        [SetUp]
        public void SetUp()
        {
            _body = new GameObject("Body");
            _eye = new GameObject("Eye").transform;
            _eye.SetParent(_body.transform, false);
            _look = _body.AddComponent<SleeperLook>();
            _look.Bind(_eye);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_body);

        [Test]
        public void SidewaysTurnsTheBodyAndNotTheEye()
        {
            _look.Tick(new Vector2(100f, 0f));

            Assert.That(_body.transform.eulerAngles.y,
                Is.EqualTo(100f * _look.DegreesPerCount).Within(1e-3f));
            Assert.That(_look.Pitch, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(_eye.localRotation.eulerAngles.x, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void PushingTheMouseForwardLooksUp()
        {
            _look.Tick(new Vector2(0f, 100f));

            // Negative pitch is up, and the body must not roll or turn with it.
            Assert.That(_look.Pitch, Is.EqualTo(-100f * _look.DegreesPerCount).Within(1e-3f));
            Assert.That(_body.transform.eulerAngles.y, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void PitchStopsShortOfStraightUpAndStraightDown()
        {
            for (int i = 0; i < 50; i++) _look.Tick(new Vector2(0f, 10000f));
            Assert.That(_look.Pitch, Is.EqualTo(-_look.PitchLimit).Within(1e-3f));

            for (int i = 0; i < 100; i++) _look.Tick(new Vector2(0f, -10000f));
            Assert.That(_look.Pitch, Is.EqualTo(_look.PitchLimit).Within(1e-3f));
        }

        [Test]
        public void YawKeepsAccumulatingWherePitchDoesNot()
        {
            // Turning right for ever is fine; tipping over backwards is not.
            for (int i = 0; i < 10; i++) _look.Tick(new Vector2(500f, 0f));

            float expected = Mathf.Repeat(10f * 500f * _look.DegreesPerCount, 360f);
            Assert.That(_body.transform.eulerAngles.y, Is.EqualTo(expected).Within(1e-2f));
        }

        [Test]
        public void AStillMouseMovesNothing()
        {
            _look.Tick(new Vector2(250f, 250f));
            float yaw = _body.transform.eulerAngles.y;
            float pitch = _look.Pitch;

            _look.Tick(Vector2.zero);

            Assert.That(_body.transform.eulerAngles.y, Is.EqualTo(yaw).Within(1e-4f));
            Assert.That(_look.Pitch, Is.EqualTo(pitch).Within(1e-4f));
        }
    }
}
