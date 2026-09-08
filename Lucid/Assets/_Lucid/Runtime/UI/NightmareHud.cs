using System;
using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lucid.Runtime.UI
{
    /// <summary>
    /// The Nightmare's chrome for M0 (docs/UI.md §16): the palette, the budget
    /// with its trickle ring, the dawn timer with its phase banner, the layer
    /// buttons and the rejection label. Every number is read off Core each
    /// frame; every string is §14's.
    /// </summary>
    /// <remarks>
    /// Nothing here animates a value. The budget steps when Core steps it,
    /// the timer digits are exactly the clock, and the rejection label appears
    /// in the same frame as the red ghost — the design system's never-animate
    /// list, which is docs/UI.md §1.6 applied to this screen.
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public sealed class NightmareHud : MonoBehaviour
    {
        /// <summary>Last 30 s: the arc pulses, the digits do not (guide §6).</summary>
        const int UrgentMs = 30_000;

        UIDocument _document;
        NightmareController _controller;

        VisualElement _cards;
        Label _budgetValue, _budgetRate, _dawnValue, _phase, _layerValue, _rejectionText;
        LucidRing _trickle, _dawnArc;
        VisualElement _rejection;
        readonly List<VisualElement> _tiles = new List<VisualElement>();

        public VisualElement Root => _document != null ? _document.rootVisualElement : null;

        void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            VisualElement root = _document.rootVisualElement;
            if (root == null)
                throw new InvalidOperationException($"{name}: the UIDocument has no root; is a PanelSettings assigned?");

            _cards = root.Q("cube-cards");
            _budgetValue = root.Q<Label>("budget-value");
            _budgetRate = root.Q<Label>("budget-rate");
            _trickle = root.Q<LucidRing>("trickle");
            _dawnArc = root.Q<LucidRing>("dawn-arc");
            _dawnValue = root.Q<Label>("dawn-value");
            _phase = root.Q<Label>("phase");
            _layerValue = root.Q<Label>("layer-value");
            _rejection = root.Q("rejection");
            _rejectionText = root.Q<Label>("rejection-reason");

            root.Q<Button>("layer-up").clicked += () => { if (_controller != null) _controller.LayerUp(); };
            root.Q<Button>("layer-down").clicked += () => { if (_controller != null) _controller.LayerDown(); };

            // A UIDocument drops its tree on disable and clones the UXML again
            // on enable, so everything built into the old tree is gone. Bind
            // runs once, from the controller's Start; a return from the
            // Sleeper's side needs the palette built again.
            if (_controller != null) Bind(_controller);
        }

        /// <summary>
        /// Whether a screen point — Input System convention, origin bottom-left —
        /// lands on the chrome rather than the world. The root and the
        /// read-only clusters ignore picking, so only the docks and buttons
        /// answer.
        /// </summary>
        public bool Covers(Vector2 screenPoint) => Under(screenPoint) != null;

        /// <summary>The chrome element under a screen point, or null for the world.</summary>
        public VisualElement Under(Vector2 screenPoint)
        {
            VisualElement root = Root;
            if (root == null || root.panel == null) return null;
            var topLeft = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            Vector2 local = RuntimePanelUtils.ScreenToPanel(root.panel, topLeft);
            return root.panel.Pick(local);
        }

        /// <summary>Builds the palette from the controller's list and starts following its state.</summary>
        public void Bind(NightmareController controller)
        {
            _controller = controller;
            _cards.Clear();
            _tiles.Clear();

            for (int i = 0; i < controller.Palette.Count; i++)
            {
                CubeDefinition type = controller.Palette[i];
                int slot = i + 1;
                var tile = new VisualElement();
                tile.AddToClassList("palette-tile");
                tile.name = $"tile-{type.Id}";

                var net = new LucidConnectorNet();
                net.AddToClassList("connector-net");
                net.AddToClassList("palette-tile__net");
                net.SetMask(type.Connectors);
                tile.Add(net);

                var body = new VisualElement();
                body.AddToClassList("palette-tile__body");
                var nameLabel = new Label(type.DisplayName);
                nameLabel.AddToClassList("palette-tile__name");
                body.Add(nameLabel);

                var row = new VisualElement();
                row.AddToClassList("palette-tile__row");
                var cost = new Label(type.Cost.ToString());
                cost.name = "cost";
                cost.AddToClassList("cost-badge");
                cost.AddToClassList("tabular");
                row.Add(cost);
                var hotkey = new Label(slot.ToString());
                hotkey.AddToClassList("btn__hotkey");
                row.Add(hotkey);
                body.Add(row);
                tile.Add(body);

                int captured = slot;
                tile.RegisterCallback<ClickEvent>(_ => controller.SelectSlot(captured));
                _cards.Add(tile);
                _tiles.Add(tile);
            }

            ShowLayer(controller.View.Cutaway);
            ShowVerdict(controller);
        }

        void Update()
        {
            if (_controller == null || _controller.Round == null || _controller.Round.Round == null) return;
            Round round = _controller.Round.Round;

            // Budget: the number steps, the ring fills toward the next point.
            // The intervals are clamped to a millisecond: zero is legal to
            // Core and would be a NaN ring here.
            int points = round.Budget.Points;
            int trickleMs = Math.Max(1, round.Settings.TrickleIntervalMs);
            _budgetValue.text = points.ToString();
            _budgetRate.text = LucidStrings.Trickle(trickleMs);
            _trickle.progress = 1f - (float)round.Budget.MsUntilNextPoint / trickleMs;

            for (int i = 0; i < _tiles.Count; i++)
            {
                CubeDefinition type = _controller.Palette[i];
                bool poor = !round.Budget.CanAfford(type.Cost);
                _tiles[i].EnableInClassList("palette-tile--unaffordable", poor);
                _tiles[i].Q<Label>("cost").EnableInClassList("cost-badge--over", poor);
                _tiles[i].EnableInClassList("palette-tile--selected", _controller.Selected == type);
            }

            // Dawn: the arc is the whole round, the digits the time left.
            int remaining = Math.Max(0, round.Settings.RoundLengthMs - round.ClockMs);
            _dawnArc.progress = (float)round.ClockMs / Math.Max(1, round.Settings.RoundLengthMs);
            _dawnArc.EnableInClassList("timer-arc--urgent", remaining <= UrgentMs && round.Phase != Phase.Dawn);
            _dawnValue.text = LucidStrings.Clock(TimeSpan.FromMilliseconds(remaining));
            _phase.text = PhaseCopy(round, remaining);

            if (_rejection.ClassListContains("rejection--hidden") == false && _controller.Ghost.IsShown)
                PlaceRejection();
        }

        static string PhaseCopy(Round round, int remainingMs)
        {
            switch (round.Phase)
            {
                case Phase.HeadStart:
                    return LucidStrings.SleepersStirIn(TimeSpan.FromMilliseconds(Math.Max(0, round.Settings.HeadStartMs - round.ClockMs)));
                case Phase.Dawn:
                    return LucidStrings.Dawn;
                default:
                    return remainingMs <= UrgentMs
                        ? LucidStrings.DawnIn(TimeSpan.FromMilliseconds(remainingMs))
                        : LucidStrings.SleepersRunning;
            }
        }

        public void ShowLayer(LayerCutaway cutaway) => _layerValue.text = cutaway.Layer.ToString();

        /// <summary>The ghost's verdict: the label with §14's reason, or nothing.</summary>
        public void ShowVerdict(NightmareController controller)
        {
            string copy = controller.RejectionCopy();
            bool show = copy != null && controller.Ghost.IsShown;
            _rejection.EnableInClassList("rejection--hidden", !show);
            if (show)
            {
                _rejectionText.text = copy;
                PlaceRejection();
            }
        }

        /// <summary>Rides the ghost: its world position, projected into the panel.</summary>
        void PlaceRejection()
        {
            Camera cam = _controller.View.Camera;
            Vector3 world = _controller.Ghost.transform.position + Vector3.up * (CubeMetrics.Half + 1f);
            Vector2 panel = RuntimePanelUtils.CameraTransformWorldToPanel(_rejection.panel, world, cam);
            _rejection.style.left = panel.x;
            _rejection.style.top = panel.y;
        }

        /// <summary>The rejection label's text, for a test to read.</summary>
        public string RejectionText => _rejection.ClassListContains("rejection--hidden") ? null : _rejectionText.text;
    }
}
