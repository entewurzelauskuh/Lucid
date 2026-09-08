using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The cube the Nightmare is about to place: a translucent box where it
    /// would stand, green when Core says yes and red when it says no
    /// (docs/UI.md §8). The reason travels separately, in the HUD.
    /// </summary>
    /// <remarks>
    /// A box and not the cube's own prefab, so that a rejected ghost never
    /// looks like a room — the acceptance for M0.7 is that every rejection
    /// reads, and a red prefab would read as a red room. Colour changes in
    /// the same frame as the verdict: the motion contract's never-animate
    /// list has the rejection on it.
    /// </remarks>
    public sealed class PlacementGhost : MonoBehaviour
    {
        // Not USS tokens: this is world geometry, outside the overlay. The red
        // is --danger-500; the green is the mockups' ghost, which the tokens do
        // not name because nothing in the overlay is ever that colour.
        static readonly Color k_Ok = new Color(0.31f, 0.82f, 0.54f, 0.32f);
        static readonly Color k_No = new Color(0.85f, 0.27f, 0.23f, 0.32f);

        /// <summary>A little smaller than the cube, so it sits inside the room it would fill.</summary>
        const float Shrink = 0.4f;

        MeshRenderer _renderer;
        MaterialPropertyBlock _block;

        public bool IsShown { get; private set; }
        public Coord Target { get; private set; }
        public bool Ok { get; private set; }

        public static PlacementGhost Create(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Ghost";
            Destroy(go.GetComponent<Collider>());   // never in the way of a pick
            go.transform.SetParent(parent, false);
            var ghost = go.AddComponent<PlacementGhost>();
            ghost.Hide();
            return ghost;
        }

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            var material = new Material(shader) { name = "Ghost" };
            // URP's transparent surface, set the way the shader GUI would.
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _renderer.material = material;
            _block = new MaterialPropertyBlock();

            float side = CubeMetrics.Size - Shrink;
            transform.localScale = new Vector3(side, side, side);
        }

        public void Show(Coord target, Rotation rotation, bool ok)
        {
            Target = target;
            Ok = ok;
            IsShown = true;
            transform.localPosition = DreamSpace.Origin(target) + new Vector3(0f, CubeMetrics.Half, 0f);
            transform.localRotation = DreamSpace.Orientation(rotation);
            _block.SetColor("_BaseColor", ok ? k_Ok : k_No);
            _renderer.SetPropertyBlock(_block);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            IsShown = false;
            gameObject.SetActive(false);
        }
    }
}
