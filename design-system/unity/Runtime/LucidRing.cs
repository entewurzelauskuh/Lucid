// Lucid — the one element USS cannot express.
//
// Every ring and arc in the game is this class: the dawn timer's half arc, the
// health ring, a power cooldown, the budget trickle point, the crosshair's
// weak-point gauge. Colour, thickness and the track come from USS custom
// properties (see lucid-components.uss), so a contributor restyles them without
// touching C#.
//
// Usage from UXML:   <lucid:LucidRing class="timer-arc" progress="0.4" />
// Usage from C#:     ring.progress = 0.4f;   // 0..1, clamped
//
// Unity 6 / UI Toolkit. MIT.

using UnityEngine;
using UnityEngine.UIElements;

namespace Lucid.Runtime.UI
{
    [UxmlElement]
    public partial class LucidRing : VisualElement
    {
        // --- USS custom properties -------------------------------------------------
        static readonly CustomStyleProperty<Color> k_Track = new("--ring-track");
        static readonly CustomStyleProperty<Color> k_Fill  = new("--ring-fill");
        static readonly CustomStyleProperty<float> k_Width = new("--ring-width");

        Color m_Track = new Color(0.76f, 0.83f, 0.92f, 0.12f);
        Color m_Fill  = new Color(1f, 0.89f, 0.64f, 1f);
        float m_Width = 3f;

        // --- UXML attributes -------------------------------------------------------

        /// <summary>How much of the sweep is filled, 0..1.</summary>
        [UxmlAttribute]
        public float progress
        {
            get => m_Progress;
            set { m_Progress = Mathf.Clamp01(value); MarkDirtyRepaint(); }
        }
        float m_Progress;

        /// <summary>Degrees clockwise from twelve o'clock where the sweep begins.</summary>
        [UxmlAttribute]
        public float startAngle
        {
            get => m_StartAngle;
            set { m_StartAngle = value; MarkDirtyRepaint(); }
        }
        float m_StartAngle = -90f;

        /// <summary>Total degrees the ring spans. 360 for a ring, 180 for the dawn arc.</summary>
        [UxmlAttribute]
        public float sweepAngle
        {
            get => m_Sweep;
            set { m_Sweep = value; MarkDirtyRepaint(); }
        }
        float m_Sweep = 360f;

        /// <summary>Draw the fill from the end of the sweep backwards (cooldowns drain).</summary>
        [UxmlAttribute]
        public bool reverse
        {
            get => m_Reverse;
            set { m_Reverse = value; MarkDirtyRepaint(); }
        }
        bool m_Reverse;

        /// <summary>Round the fill's ends. Off for gauges a player reads a value off.</summary>
        [UxmlAttribute]
        public bool roundCaps
        {
            get => m_RoundCaps;
            set { m_RoundCaps = value; MarkDirtyRepaint(); }
        }
        bool m_RoundCaps = true;

        public LucidRing()
        {
            pickingMode = PickingMode.Ignore;   // a ring is never a hit target
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            var s = e.customStyle;
            if (s.TryGetValue(k_Track, out var track)) m_Track = track;
            if (s.TryGetValue(k_Fill,  out var fill))  m_Fill  = fill;
            if (s.TryGetValue(k_Width, out var width)) m_Width = width;
            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (r.width < 2f || r.height < 2f) return;

            var painter = ctx.painter2D;
            var centre = new Vector2(r.width * 0.5f, r.height * 0.5f);

            // A half arc is drawn in a box half as tall, so radius follows width.
            float radius = (Mathf.Approximately(m_Sweep, 360f)
                ? Mathf.Min(r.width, r.height) * 0.5f
                : r.width * 0.5f) - m_Width * 0.5f - 1f;
            if (radius <= 1f) return;

            painter.lineWidth = m_Width;
            painter.lineCap = m_RoundCaps ? LineCap.Round : LineCap.Butt;

            // Track: the whole sweep, always drawn, so the gauge has a frame.
            painter.strokeColor = m_Track;
            painter.BeginPath();
            painter.Arc(centre, radius,
                new Angle(m_StartAngle, AngleUnit.Degree),
                new Angle(m_StartAngle + m_Sweep, AngleUnit.Degree));
            painter.Stroke();

            if (m_Progress <= 0f) return;

            float filled = m_Sweep * m_Progress;
            float from = m_Reverse ? m_StartAngle + m_Sweep - filled : m_StartAngle;

            painter.strokeColor = m_Fill;
            painter.BeginPath();
            painter.Arc(centre, radius,
                new Angle(from, AngleUnit.Degree),
                new Angle(from + filled, AngleUnit.Degree));
            painter.Stroke();
        }
    }
}
