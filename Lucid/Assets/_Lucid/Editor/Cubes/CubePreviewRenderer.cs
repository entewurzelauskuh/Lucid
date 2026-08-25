using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Renders a built cube from fixed camera positions. These are the images a
    /// reviewer actually looks at, so the angles are the same for every cube:
    /// two cubes side by side in a pull request should differ only where the
    /// cubes differ (docs/SPEC.md §17).
    /// </summary>
    public static class CubePreviewRenderer
    {
        const int Size = 512;

        /// <summary>Renders the spec's cameras and returns the paths written.</summary>
        public static List<string> Render(GameObject prefab, CubeSpec spec, string cubeFolder)
        {
            var written = new List<string>();
            string folder = Path.Combine(cubeFolder, "Previews");
            Directory.CreateDirectory(folder);

            // An isolated scene, so a preview shows the cube and nothing else.
            // Rendering into whatever scene happens to be open means anything
            // else in it — a leftover instance, the sample scene's props —
            // appears in the image and silently defeats the cut-away.
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject instance = null;
            GameObject rig = null;
            try
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                rig = new GameObject("preview-camera");
                EditorSceneManager.MoveGameObjectToScene(rig, scene);

                Camera camera = Configure(rig);
                // A camera only draws its own scene once it is told to.
                camera.scene = scene;
                foreach (PreviewCamera angle in spec.Preview?.EffectiveCameras
                                                ?? PreviewSpec.DefaultCameras)
                {
                    Place(camera, angle, spec);

                    // Without the cut-away, iso is the outside of a solid box
                    // and top is a plain grey ceiling. The interior is the part
                    // a reviewer needs to see.
                    List<Renderer> hidden = CutAway(instance, angle);
                    string path = Path.Combine(folder, angle.ToString().ToLowerInvariant() + ".png");
                    bool captured = Capture(camera, path);
                    foreach (Renderer r in hidden) r.enabled = true;

                    if (captured) written.Add(path.Replace('\\', '/'));
                }
            }
            finally
            {
                if (rig != null) Object.DestroyImmediate(rig);
                if (instance != null) Object.DestroyImmediate(instance);
                EditorSceneManager.ClosePreviewScene(scene);
            }

            return written;
        }

        /// <summary>
        /// Hides the shell pieces between the camera and the interior, and
        /// returns them so they can be restored. Names come from
        /// <see cref="ShellBuilder"/>; a piece it does not know about is left
        /// alone, which fails towards showing too much rather than too little.
        /// </summary>
        static List<Renderer> CutAway(GameObject instance, PreviewCamera angle)
        {
            string[] hide;
            string suffix = null;
            switch (angle)
            {
                // Iso looks down from the east and south, so those two walls
                // and the ceiling are what stands in the way.
                case PreviewCamera.Iso:
                    hide = new[] { "ceiling", "wall_east", "wall_south", "frame_south" };
                    break;

                // Straight down: only the ceiling is in the way, and without it
                // this is a floor plan.
                case PreviewCamera.Top:
                    // The floor goes too: from directly above it is coplanar
                    // with the wall tops and lights identically, so leaving it
                    // in gives one flat square. Without it the corridor reads
                    // as the gap it is.
                    //
                    // The lintels go with them. Each sits above its doorway, so
                    // from directly overhead they roof the openings and the plan
                    // reads as a sealed box with no way in.
                    hide = new[] { "ceiling", "floor" };
                    suffix = "_lintel";
                    break;

                // Standing in the doorway looking in; nothing is in the way.
                default:
                    return new List<Renderer>();
            }

            var hidden = new List<Renderer>();
            foreach (Renderer r in instance.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;

                string name = r.gameObject.name;
                bool match = suffix != null && name.EndsWith(suffix);
                if (!match)
                {
                    foreach (string prefix in hide)
                    {
                        if (name.StartsWith(prefix)) { match = true; break; }
                    }
                }

                if (!match) continue;
                r.enabled = false;
                hidden.Add(r);
            }
            return hidden;
        }

        /// <summary>
        /// A key light and a fill. Without them every surface returns the same
        /// value and a top-down view of a floor and its walls is one flat grey
        /// square — which is exactly what the first version produced.
        /// </summary>
        static void Light(GameObject rig)
        {
            var keyGo = new GameObject("key");
            keyGo.transform.SetParent(rig.transform, false);
            var key = keyGo.AddComponent<UnityEngine.Light>();
            key.type = LightType.Directional;
            key.intensity = 1.1f;
            key.color = new Color(1f, 0.97f, 0.92f);
            // Steep and off-axis, so horizontal and vertical surfaces separate
            // and the two wall directions do not read the same.
            keyGo.transform.rotation = Quaternion.Euler(52f, 34f, 0f);

            var fillGo = new GameObject("fill");
            fillGo.transform.SetParent(rig.transform, false);
            var fill = fillGo.AddComponent<UnityEngine.Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.35f;
            fill.color = new Color(0.75f, 0.8f, 1f);
            fillGo.transform.rotation = Quaternion.Euler(20f, 214f, 0f);
        }

        static Camera Configure(GameObject rig)
        {
            Light(rig);

            var camera = rig.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;

            // A flat mid grey: dark enough that pale shells read, light enough
            // that a silhouette does. The skin decides the real colours, so a
            // preview should not pretend to.
            camera.backgroundColor = new Color(0.22f, 0.23f, 0.26f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            camera.enabled = false;
            return camera;
        }

        /// <summary>
        /// The three standard angles. Iso shows the shape, entrance shows what
        /// a Sleeper sees walking in, top shows the layout.
        /// </summary>
        static void Place(Camera camera, PreviewCamera angle, CubeSpec spec)
        {
            var centre = new Vector3(0f, CubeGeometry.Size / 2f, 0f);

            switch (angle)
            {
                case PreviewCamera.Iso:
                    camera.orthographic = true;
                    camera.orthographicSize = 6.5f;
                    camera.transform.position = centre + new Vector3(11f, 9f, -11f);
                    camera.transform.LookAt(centre);
                    break;

                case PreviewCamera.Top:
                    camera.orthographic = true;
                    camera.orthographicSize = CubeGeometry.Half + 0.3f;
                    camera.transform.position = new Vector3(0f, 20f, 0f);
                    // Rolled so north is up, the way the Nightmare's god view
                    // reads the lattice (docs/UI.md §8).
                    camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    break;

                case PreviewCamera.Entrance:
                    // Eye height, just outside the first doorway the spec lists,
                    // looking in the way a Sleeper arrives.
                    camera.orthographic = false;
                    camera.fieldOfView = 70f;
                    Vector3 outside = EntranceCentre(spec);
                    camera.transform.position = outside + outside.normalized * 2f + Vector3.up * 1.7f;
                    camera.transform.LookAt(new Vector3(0f, 1.7f, 0f));
                    break;

                default:
                    goto case PreviewCamera.Iso;
            }
        }

        static Vector3 EntranceCentre(CubeSpec spec)
        {
            SpecFace face = spec.Connectors != null && spec.Connectors.Length > 0
                ? spec.Connectors[0]
                : SpecFace.North;
            Vector3 centre = CubeGeometry.Centre(CubeSpecMapping.ToFace(face));
            return centre.sqrMagnitude < 1e-4f ? new Vector3(0f, 0f, CubeGeometry.Half) : centre;
        }

        /// <summary>
        /// Returns false rather than throwing when there is no graphics device;
        /// a headless machine should still be able to build and validate cubes.
        /// </summary>
        static bool Capture(Camera camera, string path)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                return false;

            var target = new RenderTexture(Size, Size, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(Size, Size, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;

            try
            {
                camera.targetTexture = target;
                camera.Render();

                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, Size, Size), 0, 0);
                image.Apply();

                File.WriteAllBytes(path, image.EncodeToPNG());
                return true;
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                Object.DestroyImmediate(image);
                Object.DestroyImmediate(target);
            }
        }
    }
}
