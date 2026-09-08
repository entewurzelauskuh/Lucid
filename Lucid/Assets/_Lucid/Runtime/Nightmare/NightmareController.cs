using System;
using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime.UI;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The Nightmare's hands (docs/UI.md §8): select a type, hover a door,
    /// rotate, confirm — and the camera, the layer slider and the ghost that
    /// tells them what Core will say before they click.
    /// </summary>
    /// <remarks>
    /// Everything a test needs is a method here — <see cref="Select"/>,
    /// <see cref="Hover"/>, <see cref="RotateGhost"/>, <see cref="Place"/> —
    /// and <see cref="Update"/> is the thin layer that turns input into those
    /// calls. The verdict shown on the ghost is Core's own, asked every frame
    /// the ghost stands, so a Sleeper walking into a room the Nightmare is
    /// aiming at turns the ghost red in the frame it would trap them.
    /// </remarks>
    public sealed class NightmareController : MonoBehaviour
    {
        /// <summary>Pixels of right-drag past which a release is an orbit, not a cancel.</summary>
        const float DragThreshold = 4f;

        [SerializeField] GodViewCamera _view;
        [SerializeField] LocalRound _round;
        [SerializeField] NightmareInputSource _input;
        [SerializeField] NightmareHud _hud;

        readonly List<CubeDefinition> _palette = new List<CubeDefinition>();
        PlacementGhost _ghost;
        float _dragged;
        bool _orbiting;

        public IReadOnlyList<CubeDefinition> Palette => _palette;
        public CubeDefinition Selected { get; private set; }
        public Rotation Rotation { get; private set; }
        public ConnectorRef? Hovered { get; private set; }
        public PlaceVerdict LastVerdict { get; private set; } = PlaceVerdict.Pass;
        public PlacementGhost Ghost => _ghost;
        public GodViewCamera View => _view;
        public LocalRound Round => _round;

        internal void Configure(GodViewCamera view, LocalRound round, NightmareInputSource input, NightmareHud hud)
        {
            _view = view;
            _round = round;
            _input = input;
            _hud = hud;
        }

        void Awake()
        {
            _ghost = PlacementGhost.Create(transform);
        }

        void Start()
        {
            // The palette is the pack's connectors, in id order, and nothing
            // else until M1 (docs/WORKPLAN.md §4: "palette (connectors only)").
            _palette.Clear();
            foreach (CubeDefinition d in PackOf(_round))
                if (d != null && d.Category == CubeCategory.Connector) _palette.Add(d);
            _palette.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            _round.Placed += OnPlaced;
            if (_hud != null) _hud.Bind(this);
        }

        void OnDestroy()
        {
            if (_round != null) _round.Placed -= OnPlaced;
        }

        static IEnumerable<CubeDefinition> PackOf(LocalRound round)
        {
            var pack = round != null && round.Dream != null ? round.Dream.Pack : null;
            return pack != null ? pack.Cubes : Array.Empty<CubeDefinition>();
        }

        // ---- what a test calls ---------------------------------------------------

        /// <summary>Pick a palette slot, 1-based as the hotkeys are; 0 or out of range clears.</summary>
        public void SelectSlot(int slot) => Select(slot >= 1 && slot <= _palette.Count ? _palette[slot - 1] : null);

        public void Select(CubeDefinition type)
        {
            Selected = type;
            if (type == null) ClearGhost();
            else Refresh();
        }

        public void RotateGhost()
        {
            Rotation = (Rotation)(((int)Rotation + 1) & 3);
            Refresh();
        }

        /// <summary>The door under the cursor, or null for none. The ghost follows.</summary>
        public void Hover(ConnectorRef? door)
        {
            Hovered = door;
            Refresh();
        }

        /// <summary>Confirm. Core decides; the ghost already showed what it would say.</summary>
        public PlaceVerdict Place()
        {
            if (Selected == null || Hovered == null) return new PlaceVerdict(PlaceError.NotADoor);
            PlaceVerdict verdict = _round.TryPlace(Request());
            LastVerdict = verdict;
            Refresh();
            return verdict;
        }

        /// <summary>Esc or right-click: drop the selection. True if there was one.</summary>
        public bool Cancel()
        {
            if (Selected == null) return false;
            Select(null);
            return true;
        }

        public PlaceRequest Request() =>
            new PlaceRequest(Hovered ?? default, Selected != null ? Selected.Id : "", Rotation, "*");

        /// <summary>Copy for the ghost's verdict, or null when it stands green.</summary>
        public string RejectionCopy()
        {
            if (LastVerdict.Ok) return null;
            string who = LastVerdict.TrappedSleeper >= 0 ? $"Sleeper {LastVerdict.TrappedSleeper + 1}" : null;
            int cost = Selected != null ? Selected.Cost : 0;
            return PlacementCopy.For(LastVerdict, who, _round.Round.Budget.Points, cost);
        }

        // ---- the ghost ---------------------------------------------------------------

        void Refresh()
        {
            if (Selected == null || Hovered == null)
            {
                ClearGhost();
                return;
            }

            ConnectorRef door = Hovered.Value;
            LastVerdict = _round.Validate(Request());
            _ghost.Show(door.Cube.Offset(door.Face), Rotation, LastVerdict.Ok);
            _hud?.ShowVerdict(this);
        }

        void ClearGhost()
        {
            _ghost.Hide();
            LastVerdict = PlaceVerdict.Pass;
            _hud?.ShowVerdict(this);
        }

        void OnPlaced(PlaceRequest _)
        {
            _view.ApplyCutaway();
            // The door just built on is Attached now; the ghost re-asks Core.
            Refresh();
        }

        // ---- input -------------------------------------------------------------------

        void Update()
        {
            if (_input == null) return;
            NightmareInput input = _input.Read();

            if (input.SelectSlot != 0) SelectSlot(input.SelectSlot);
            if (input.RotatePressed) RotateGhost();
            if (input.LayerUpPressed) { _view.LayerUp(); _hud?.ShowLayer(_view.Cutaway); }
            if (input.LayerDownPressed) { _view.LayerDown(); _hud?.ShowLayer(_view.Cutaway); }
            if (input.TopDownPressed) _view.ToggleTopDown();
            if (input.FocusPressed) _view.FocusStart();
            if (input.Zoom != 0f) _view.Zoom(Mathf.Sign(input.Zoom));

            // Right button: drag orbits, a click cancels. Which it was is known
            // only on release, by how far it travelled.
            if (input.OrbitHeld)
            {
                _dragged += input.Look.magnitude;
                if (_dragged > DragThreshold)
                {
                    _orbiting = true;
                    _view.Orbit(input.Look);
                }
            }
            if (input.OrbitReleased)
            {
                if (!_orbiting) Cancel();
                _orbiting = false;
                _dragged = 0f;
            }
            if (input.PanHeld) _view.Pan(-input.Look);
            if (input.Pan != Vector2.zero) _view.Pan(input.Pan * 12f * Time.deltaTime);

            // The door under the cursor, only while something is selected —
            // an idle cursor does not raycast the world every frame for nothing.
            if (Selected != null)
            {
                ConnectorRef? under = DoorPicker.Pick(_view.Camera, input.Point, out ConnectorRef door, out _)
                    ? door : (ConnectorRef?)null;
                if (!Nullable.Equals(under, Hovered)) Hover(under);
                if (input.PlacePressed && Hovered != null) Place();
            }
        }
    }
}
