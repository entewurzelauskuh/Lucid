// Lucid — the connector net.
//
// A cube has six faces, each a doorway or a wall. The net unfolds them into a
// cross: top, then west / north / east across the middle, then south, then
// bottom. A filled dot is a doorway; a dashed empty square is a wall.
//
// It is GENERATED from the same six-bit mask the cube data already carries, so
// a new cube type never needs a new icon asset. Mask order is
// [top, west, north, east, south, bottom] — "010100" is a Straight.
//
// Unity 6 / UI Toolkit. MIT.

using UnityEngine;
using UnityEngine.UIElements;

namespace Lucid.Runtime.UI
{
    [UxmlElement]
    public partial class LucidConnectorNet : VisualElement
    {
        static readonly CustomStyleProperty<Color> k_Ink = new("--net-ink");

        // Cell positions in the cross, in mask order.
        static readonly Vector2Int[] k_Cells =
        {
            new(1, 0),  // top
            new(0, 1),  // west
            new(1, 1),  // north
            new(2, 1),  // east
            new(1, 2),  // south
            new(1, 3)   // bottom
        };

        Color m_Ink = new Color(0.95f, 0.96f, 0.99f, 1f);

        /// <summary>Six characters, '1' = connector. Anything else counts as a wall.</summary>
        [UxmlAttribute]
        public string mask
        {
            get => m_Mask;
            set { m_Mask = string.IsNullOrEmpty(value) ? "000000" : value.PadRight(6, '0'); MarkDirtyRepaint(); }
        }
        string m_Mask = "010100";

        public LucidConnectorNet()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(e =>
            {
                if (e.customStyle.TryGetValue(k_Ink, out var ink)) m_Ink = ink;
                MarkDirtyRepaint();
            });
        }

        /// <summary>
        /// From Core's mask, whose order is North, East, South, West, Up, Down
        /// (<c>Lucid.Core.Face</c>). The net's own order is top, west, north,
        /// east, south, bottom; the reorder happens here and nowhere else.
        /// </summary>
        public void SetMask(Lucid.Core.FaceMask faces)
        {
            var f = new[]
            {
                Lucid.Core.Face.Up, Lucid.Core.Face.West, Lucid.Core.Face.North,
                Lucid.Core.Face.East, Lucid.Core.Face.South, Lucid.Core.Face.Down,
            };
            var chars = new char[6];
            for (int i = 0; i < 6; i++) chars[i] = Lucid.Core.Faces.Has(faces, f[i]) ? '1' : '0';
            mask = new string(chars);
        }

        void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (r.width < 6f || r.height < 8f) return;

            var painter = ctx.painter2D;

            // The net is 3 cells wide by 4 tall, with a 1.5 px gutter.
            const float gutter = 1.5f;
            float cell = Mathf.Min((r.width - gutter * 2f) / 3f, (r.height - gutter * 3f) / 4f);
            if (cell < 3f) return;

            float netW = cell * 3f + gutter * 2f;
            float netH = cell * 4f + gutter * 3f;
            var origin = new Vector2((r.width - netW) * 0.5f, (r.height - netH) * 0.5f);

            painter.lineWidth = Mathf.Max(1f, cell * 0.045f);
            painter.lineCap = LineCap.Butt;

            for (int i = 0; i < 6; i++)
            {
                bool on = m_Mask[i] == '1';
                var c = k_Cells[i];
                var p = origin + new Vector2(c.x * (cell + gutter), c.y * (cell + gutter));

                var ink = m_Ink;
                ink.a *= on ? 0.9f : 0.3f;
                painter.strokeColor = ink;

                // The square. Walls read as fainter; Painter2D has no dash, so the
                // weight difference plus the missing dot carries the distinction.
                painter.BeginPath();
                painter.MoveTo(p);
                painter.LineTo(p + new Vector2(cell, 0f));
                painter.LineTo(p + new Vector2(cell, cell));
                painter.LineTo(p + new Vector2(0f, cell));
                painter.ClosePath();
                painter.Stroke();

                if (!on) continue;

                // The doorway dot.
                painter.fillColor = m_Ink;
                painter.BeginPath();
                painter.Arc(p + new Vector2(cell * 0.5f, cell * 0.5f), cell * 0.22f,
                    new Angle(0f, AngleUnit.Degree), new Angle(360f, AngleUnit.Degree));
                painter.Fill();
            }
        }
    }
}
