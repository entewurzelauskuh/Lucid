using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The mist a fog door shows: a stack of quads across the doorway, each
    /// drifting at its own scale, driven from the door's state.
    /// </summary>
    /// <remarks>
    /// A stack rather than one quad because a single sheet of scrolling noise
    /// reads as a moving picture; several at different depths and scales read
    /// as something you cannot see through. It is also why the door is a
    /// volume 0.25 m deep rather than a plane.
    ///
    /// Each layer carries its own property block, which is what lets four
    /// quads of one shared material show four different states. The cost is
    /// that a property block puts a renderer outside the SRP Batcher: six
    /// sockets times four layers is 24 unbatched transparent draws per cube.
    /// Accepted because the mist animates continuously and per-state shared
    /// materials could not; worth revisiting if transparent fill becomes the
    /// budget rather than the draw count.
    ///
    /// Built in code, like the door's colliders and like
    /// <see cref="SleeperRig"/>: the geometry is entirely determined by
    /// <see cref="CubeMetrics"/>, so serialising it would only create
    /// something able to disagree with the doorway it fills. The mesh is
    /// generated for the same reason CLAUDE.md rule 5 pushes back on assets —
    /// four vertices are not worth a licence line.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FogDoor))]
    public sealed class FogDoorVisual : MonoBehaviour
    {
        public const string ShaderName = "Lucid/Mist";

        [SerializeField, Range(1, 8)] int _layers = 4;

        static Mesh _quad;
        static Material _material;

        FogDoor _door;
        Renderer[] _renderers;
        MaterialPropertyBlock _block;
        FogDoorLook _from, _to;
        ConnectorStateCache _rendered;

        /// <summary>The look currently on screen, after any transition blend.</summary>
        public FogDoorLook Shown { get; private set; }

        public int Layers => _layers;

        void Awake() => EnsureReady();

        void LateUpdate() => Refresh();

        /// <summary>
        /// Finds the door and builds the stack, once. Not left to Awake alone
        /// for the reason SleeperMotor learned the hard way: a component added
        /// in edit mode never gets one, so anything that depended on it was
        /// silently inert until play began.
        /// </summary>
        void EnsureReady()
        {
            if (_door != null) return;

            _door = GetComponent<FogDoor>();
            if (_door == null) return;

            Build();
            _from = _to = Shown = FogDoorLook.For(_door.State);
            _rendered = new ConnectorStateCache(_door.State);
            Apply();
        }

        /// <summary>
        /// Bring the mist up to date with the door. Public and side-effect
        /// free so a test can drive it without waiting for a frame, the same
        /// bargain <see cref="SleeperMotor.Tick"/> makes.
        /// </summary>
        public void Refresh()
        {
            EnsureReady();
            if (_door == null) return;

            if (!_rendered.Matches(_door.State))
            {
                // Blend from whatever is on screen, not from the state we were
                // in: a door interrupted mid-transition should carry on from
                // where it looks, not jump back.
                _from = Shown;
                _to = FogDoorLook.For(_door.State);
                _rendered = new ConnectorStateCache(_door.State);
            }

            Shown = FogDoorLook.Lerp(_from, _to, Mathf.Clamp01(_door.Progress));
            Apply();
        }

        void Build()
        {
            if (_renderers != null) return;

            _block = new MaterialPropertyBlock();
            _renderers = new Renderer[_layers];

            var root = new GameObject("Mist").transform;
            root.SetParent(transform, false);

            for (int i = 0; i < _layers; i++)
            {
                var go = new GameObject($"Layer{i}");
                go.transform.SetParent(root, false);

                // Spread across the door's depth so the stack has thickness.
                float t = _layers == 1 ? 0.5f : i / (float)(_layers - 1);
                // The same opening the collider fills, which for a vertical
                // connector is a 2.5 m square rather than a doorway.
                Vector3 size = FogDoor.OpeningSize(_door.Face);
                Vector3 centre = FogDoor.OpeningCentre(_door.Face);
                go.transform.localPosition = centre + new Vector3(
                    0f, 0f, Mathf.Lerp(-FogDoor.Depth / 2f, FogDoor.Depth / 2f, t));
                go.transform.localScale = new Vector3(size.x, size.y, 1f);

                go.AddComponent<MeshFilter>().sharedMesh = Quad();
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = MistMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                _renderers[i] = renderer;
            }
        }

        void Apply()
        {
            if (_renderers == null) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null) continue;

                // Each layer at its own scale, so the stack never lines up into
                // one picture. Deeper layers carry less of the density.
                float depth = _renderers.Length == 1 ? 1f : 1f - i / (float)_renderers.Length * 0.45f;

                renderer.GetPropertyBlock(_block);
                _block.SetColor("_Tint", Shown.Tint);
                _block.SetFloat("_Brightness", Shown.Brightness);
                _block.SetFloat("_Density", Shown.Density / _renderers.Length * 1.6f * depth);
                _block.SetFloat("_Base", Shown.Opacity);
                _block.SetFloat("_Scale", 1.6f + i * 0.7f);
                _block.SetFloat("_Drift", Shown.Drift * (1f + i * 0.35f));
                _block.SetFloat("_Dissolve", Shown.Dissolve);
                renderer.SetPropertyBlock(_block);
            }
        }

        static Material MistMaterial()
        {
            if (_material != null) return _material;

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogWarning($"{ShaderName} not found; fog doors will not render.");
                return null;
            }

            _material = new Material(shader) { name = "Mist (runtime)" };
            return _material;
        }

        static Mesh Quad()
        {
            if (_quad != null) return _quad;

            _quad = new Mesh { name = "Mist Quad" };
            _quad.SetVertices(new[]
            {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f,  0.5f, 0f), new Vector3(0.5f,  0.5f, 0f),
            });
            _quad.SetUVs(0, new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
            });
            _quad.SetTriangles(new[] { 0, 2, 1, 2, 3, 1 }, 0);
            _quad.RecalculateNormals();
            _quad.RecalculateBounds();
            return _quad;
        }

        /// <summary>
        /// Remembers a state without letting a default-constructed struct look
        /// like a real one.
        /// </summary>
        readonly struct ConnectorStateCache
        {
            readonly Lucid.Core.ConnectorState _state;
            readonly bool _set;

            public ConnectorStateCache(Lucid.Core.ConnectorState state)
            {
                _state = state;
                _set = true;
            }

            public bool Matches(Lucid.Core.ConnectorState state) => _set && _state == state;
        }
    }
}
