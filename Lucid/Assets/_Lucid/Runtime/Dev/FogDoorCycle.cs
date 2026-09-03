using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime.Dev
{
    /// <summary>
    /// Walks a fog door through a legal run of states so the transitions can
    /// be watched rather than only measured.
    /// </summary>
    /// <remarks>
    /// The M0.5 acceptance asks for the transitions to be verified "in a test
    /// scene and by a PlayMode test". A scene of four doors frozen one per
    /// state settles what each state looks like and says nothing about what
    /// happens between them, which is the half a person is better at judging
    /// than a test is.
    ///
    /// The run has to end by starting over, and no door un-hardens in a real
    /// dream: Solid and Attached are terminal (docs/SPEC.md §7). So the loop
    /// closes with <see cref="FogDoor.Initialise"/>, which is a fresh door
    /// rather than a transition — the same thing that happens when a cube is
    /// built from an event log.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FogDoor))]
    public sealed class FogDoorCycle : MonoBehaviour
    {
        [SerializeField] float _dwellSeconds = 1.6f;
        [SerializeField] List<ConnectorState> _run = new List<ConnectorState>
        {
            ConnectorState.Fog,
            ConnectorState.Exit,
            ConnectorState.Fog,
            ConnectorState.Solid,
        };

        FogDoor _door;
        float _elapsed;
        int _index;

        public IReadOnlyList<ConnectorState> Run => _run;

        void Awake()
        {
            _door = GetComponent<FogDoor>();
            if (_run.Count > 0) _door.Initialise(_run[0]);
        }

        void Update()
        {
            if (_door == null || _run.Count < 2) return;

            _elapsed += Time.deltaTime;
            if (_elapsed < _dwellSeconds) return;
            _elapsed = 0f;

            _index++;
            if (_index >= _run.Count)
            {
                // Back to the beginning as a new door, not as a transition.
                _index = 0;
                _door.Initialise(_run[0]);
                return;
            }

            _door.SetState(_run[_index]);
        }

        internal void Configure(float dwellSeconds, params ConnectorState[] run)
        {
            _dwellSeconds = dwellSeconds;
            _run = new List<ConnectorState>(run);
        }
    }
}
