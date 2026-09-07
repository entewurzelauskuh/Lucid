using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucid.Runtime
{
    /// <summary>
    /// Owns every transition between screens. Scenes never load each other:
    /// they ask this, and this consults <see cref="FlowTable"/>.
    /// </summary>
    /// <remarks>
    /// Lives in the Boot scene, which is never unloaded, so it outlives every
    /// screen. A transition unloads the scene the current state lives in and
    /// loads the next one additively, both asynchronously, and reports
    /// <see cref="IsTransitioning"/> meanwhile — the loading state of M0.6b's
    /// acceptance is that flag rather than a fourth <see cref="FlowState"/>,
    /// because nothing is ever *in* loading; it is between two things.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class GameFlow : MonoBehaviour
    {
        public FlowState State { get; private set; } = FlowState.Boot;

        /// <summary>True from <see cref="Go"/> until the new scene is active.</summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>Raised once the new state's scene is loaded and active.</summary>
        public event Action<FlowState> Entered;

        /// <summary>
        /// Where a state's scene comes from. The table, except in the one test
        /// that needs a scene which does not exist.
        /// </summary>
        internal Func<FlowState, string> SceneOf = FlowTable.SceneOf;

        /// <summary>
        /// Moves to <paramref name="to"/>, or throws if the table does not
        /// allow it from here. Refusing loudly rather than ignoring: a screen
        /// asking for a transition the table lacks is a bug in the screen, and
        /// a silent no-op would leave it showing while the player waits.
        /// </summary>
        public void Go(FlowState to)
        {
            if (IsTransitioning)
                throw new InvalidOperationException(
                    $"cannot go to {to}: still on the way from {State}");
            if (!FlowTable.Allows(State, to))
                throw new InvalidOperationException(
                    $"the flow table has no transition {State} → {to} (docs/UI.md §2)");

            StartCoroutine(Transition(to));
        }

        IEnumerator Transition(FlowState to)
        {
            IsTransitioning = true;
            FlowState from = State;

            // try/finally rather than a throw on the failure path: a throw from
            // a coroutine is logged and the coroutine ends, and the first draft
            // left IsTransitioning true for ever after a scene failed to load —
            // every later Go then refused with "still on the way", which was
            // not even the true reason.
            try
            {
                string leaving = SceneOf(from);
                if (leaving != null)
                {
                    Scene old = SceneManager.GetSceneByName(leaving);
                    if (old.IsValid() && old.isLoaded)
                        yield return SceneManager.UnloadSceneAsync(old);
                }

                string arriving = SceneOf(to);
                if (arriving != null)
                {
                    yield return SceneManager.LoadSceneAsync(arriving, LoadSceneMode.Additive);
                    Scene fresh = SceneManager.GetSceneByName(arriving);
                    if (!fresh.IsValid() || !fresh.isLoaded)
                    {
                        Debug.LogError(
                            $"{name}: scene '{arriving}' for {to} did not load; is it in the build " +
                            $"settings? Staying on {from}.", this);
                        yield break;
                    }
                    SceneManager.SetActiveScene(fresh);
                }

                State = to;
            }
            finally
            {
                IsTransitioning = false;
            }

            Entered?.Invoke(to);
        }
    }
}
