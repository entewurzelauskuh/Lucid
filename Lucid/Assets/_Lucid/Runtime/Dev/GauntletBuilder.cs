using System.Collections.Generic;
using UnityEngine;

namespace Lucid.Runtime.Dev
{
    /// <summary>
    /// Builds <see cref="GauntletLayout"/> out of boxes. Geometry only: no
    /// lights, no camera, no runner, so a PlayMode test pays for nothing it
    /// does not use.
    /// </summary>
    public static class GauntletBuilder
    {
        public static Gauntlet Build(IReadOnlyList<GauntletLane> lanes, Transform parent = null)
        {
            var root = new GameObject("Gauntlet");
            if (parent != null) root.transform.SetParent(parent, false);

            var built = new GauntletLane[lanes.Count];
            for (int i = 0; i < lanes.Count; i++)
            {
                built[i] = lanes[i];
                BuildLane(root.transform, i, lanes[i]);
            }

            BuildCatchFloor(root.transform, lanes);

            // Colliders created this frame are invisible to queries and sweeps
            // until the physics scene is told about them, and a test builds and
            // runs inside one frame.
            Physics.SyncTransforms();

            return new Gauntlet(root, built);
        }

        public static Gauntlet Build(Transform parent = null) =>
            Build(GauntletLayout.Standard, parent);

        static void BuildLane(Transform parent, int index, GauntletLane lane)
        {
            var root = new GameObject(lane.Name);
            root.transform.SetParent(parent, false);

            float x = GauntletLayout.LaneX(index);
            float t = GauntletLayout.SlabThickness;

            // The run-up, ending flush with the obstacle at z = 0.
            Box(root.transform, "RunUp",
                new Vector3(x, -t * 0.5f, Gauntlet.ObstacleZ - GauntletLayout.RunUp * 0.5f),
                new Vector3(GauntletLayout.LaneWidth, t, GauntletLayout.RunUp));

            if (lane.Obstacle == GauntletObstacle.Gap)
            {
                // Nothing between z = 0 and z = Size; the landing resumes after it.
                Box(root.transform, "Landing",
                    new Vector3(x, -t * 0.5f,
                        Gauntlet.ObstacleZ + lane.Size + GauntletLayout.Landing * 0.5f),
                    new Vector3(GauntletLayout.LaneWidth, t, GauntletLayout.Landing));
            }
            else
            {
                // A step whose face is at z = 0 and whose top is at y = Size.
                // It runs down past the run-up floor so there is no crawl under it.
                float height = lane.Size + t;
                Box(root.transform, "Ledge",
                    new Vector3(x, lane.Size - height * 0.5f,
                        Gauntlet.ObstacleZ + GauntletLayout.Landing * 0.5f),
                    new Vector3(GauntletLayout.LaneWidth, height, GauntletLayout.Landing));
            }
        }

        static void BuildCatchFloor(Transform parent, IReadOnlyList<GauntletLane> lanes)
        {
            float widest = 0f;
            foreach (var lane in lanes)
                if (lane.Obstacle == GauntletObstacle.Gap && lane.Size > widest) widest = lane.Size;

            float length = GauntletLayout.RunUp + widest + GauntletLayout.Landing;
            float width = Mathf.Max(1, lanes.Count) * GauntletLayout.LaneSpacing
                          + GauntletLayout.LaneWidth;

            // Falls have to land on something: a test that reads "fell" from a
            // position still dropping would be reading the frame count.
            Box(parent, "CatchFloor",
                new Vector3((width - GauntletLayout.LaneWidth - GauntletLayout.LaneSpacing) * 0.5f,
                    -GauntletLayout.CatchDepth,
                    Gauntlet.ObstacleZ - GauntletLayout.RunUp + length * 0.5f),
                new Vector3(width, GauntletLayout.SlabThickness, length));
        }

        static void Box(Transform parent, string name, Vector3 centre, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = centre;
            box.transform.localScale = size;
        }
    }
}
