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
    /// calls. The verdict shown on the ghost is Core's own, asked again every
    /// frame the ghost stands — not only when the cursor moves — so the
    /// trickle granting a point, the clock reaching dawn, or a Sleeper walking
    /// into the room the Nightmare is aiming at all change the ghost in the
    /// frame Core's answer changes.
    /// </remarks>
    public sealed class NightmareController : MonoBehaviour
    {
        /// <summary>Pixels of right-drag past which a release is an orbit, not a cancel.</summary>
        const float DragThreshold = 4f;

        /// <summary>Arrow-key pan, in screen pixels per second, so it scales with the zoom like a drag.</summary>
        const float ArrowPanPixelsPerSecond = 300f;

        /// <summary>How much brighter a door the Nightmare can build on is drawn on the god view.</summary>
        public const float BuildableHighlight = 1.8f;

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

        void Start()
        {
            // The ghost lives in the dream's frame, where the cubes are laid
            // out — not under this rig, whose transform the camera writes
            // every pose change. Parented there, a ghost at the bedroom's door
            // sat eight metres down the view axis and swung with every orbit.
            _ghost = PlacementGhost.Create(_round.Dream.transform);

            // The palette is the pack's connectors, in id order, and nothing
            // else until M1 (docs/WORKPLAN.md §4: "palette (connectors only)").
            _palette.Clear();
            foreach (CubeDefinition d in PackOf(_round))
                if (d != null && d.Category == CubeCategory.Connector) _palette.Add(d);
            _palette.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            _round.Applied += OnApplied;
            if (_hud != null) _hud.Bind(this);
            ShowBuildable(true);
        }

        void OnDestroy()
        {
            if (_round != null) _round.Applied -= OnApplied;
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

        public void LayerUp()
        {
            _view.LayerUp();
            OnCutawayChanged();
        }

        public void LayerDown()
        {
            _view.LayerDown();
            OnCutawayChanged();
        }

        void OnCutawayChanged()
        {
            if (_hud != null) _hud.ShowLayer(_view.Cutaway);
            if (_buildableShown) ShowBuildable(true);
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
            // The door just built on is Attached now, so the same hover would
            // read "Not a door" for the one frame before the cursor finds the
            // cube standing there. The hover ends with the placement instead;
            // the next frame's pick starts it again wherever the cursor is.
            if (verdict.Ok) Hovered = null;
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
            // The Sandbox's Sleeper has no name, so §14's "{name}" is its seat.
            string who = LastVerdict.TrappedSleeper >= 0 ? LucidStrings.SleeperSeat(LastVerdict.TrappedSleeper + 1) : null;
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
            if (_ghost != null) _ghost.Show(door.Cube.Offset(door.Face), Rotation, LastVerdict.Ok);
            if (_hud != null) _hud.ShowVerdict(this);
        }

        void ClearGhost()
        {
            if (_ghost != null) _ghost.Hide();
            LastVerdict = PlaceVerdict.Pass;
            if (_hud != null) _hud.ShowVerdict(this);
        }

        void OnApplied()
        {
            _view.ApplyCutaway();
            if (_buildableShown) ShowBuildable(true);
            // Doors changed state under the ghost; it re-asks Core.
            Refresh();
        }

        // ---- the in-world overlay ------------------------------------------------------

        bool _buildableShown;

        /// <summary>
        /// docs/UI.md §8: "fog doors highlighted as buildable". Every fog and
        /// exit door on a cube the slider has not cut away is drawn brighter;
        /// attached and solid doors, and doors on hidden cubes, are not. Off
        /// when the Sleeper has the screen — the overlay is the god view's.
        /// </summary>
        public void ShowBuildable(bool on)
        {
            _buildableShown = on;
            if (_round == null || _round.Dream == null) return;

            foreach (KeyValuePair<Coord, DreamCube> pair in _round.Dream.Cubes)
            {
                DreamCube cube = pair.Value;
                if (cube == null) continue;
                foreach (KeyValuePair<Face, FogDoor> door in cube.Doors)
                {
                    if (door.Value == null) continue;
                    var visual = door.Value.GetComponent<FogDoorVisual>();
                    if (visual == null) continue;
                    bool buildable = on && !cube.IsCutAway && IsBuildable(door.Value.State);
                    visual.Highlight = buildable ? BuildableHighlight : 1f;
                    visual.Refresh();
                }
            }
        }

        /// <summary>The two states a placement request can name (docs/SPEC.md §7).</summary>
        public static bool IsBuildable(ConnectorState state) =>
            state == ConnectorState.Fog || state == ConnectorState.Exit;

        // ---- input -------------------------------------------------------------------

        void Update()
        {
            // Core's answer can change with nothing moving: a trickle point
            // arrives, the clock reaches dawn, a Sleeper walks in. A standing
            // ghost asks again.
            if (_ghost != null && _ghost.IsShown) Refresh();

            if (_input == null) return;
            NightmareInput input = _input.Read();

            // The HUD is opaque to the world: a click on a palette tile or a
            // scroll in the palette must not reach a door standing behind it.
            bool overHud = _hud != null && _hud.Covers(input.Point);

            if (input.SelectSlot != 0) SelectSlot(input.SelectSlot);
            if (input.RotatePressed) RotateGhost();
            if (input.LayerUpPressed) LayerUp();
            if (input.LayerDownPressed) LayerDown();
            if (input.TopDownPressed) _view.ToggleTopDown();
            if (input.FocusPressed) _view.FocusStart();
            if (input.Zoom != 0f && !overHud) _view.Zoom(Mathf.Sign(input.Zoom));

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
            if (input.Pan != Vector2.zero) _view.Pan(input.Pan * ArrowPanPixelsPerSecond * Time.deltaTime);

            // The door under the cursor, only while something is selected —
            // an idle cursor does not raycast the world every frame for nothing
            // — and only when there is a cursor: with no pointer device the
            // point reads as the screen's corner, and a hover set by a test
            // would be cleared by the next frame's pick of nothing.
            if (Selected != null && input.HasPointer)
            {
                ConnectorRef? under = !overHud && DoorPicker.Pick(_view.Camera, input.Point, out ConnectorRef door, out _)
                    ? door : (ConnectorRef?)null;
                if (!Nullable.Equals(under, Hovered)) Hover(under);
                if (input.PlacePressed && !overHud && Hovered != null) Place();
            }
        }
    }
}
