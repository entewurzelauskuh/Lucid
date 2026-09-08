using System;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Which fog door is under the cursor. A ray through every collider,
    /// nearest first, and the first one that belongs to a door on a cube the
    /// slider has not cut away is the answer — a hidden cube's doors are still
    /// colliders, and without the skip the Nightmare would build on a room they
    /// cannot see.
    /// </summary>
    public static class DoorPicker
    {
        const float Reach = 400f;

        static readonly RaycastHit[] s_Hits = new RaycastHit[32];

        public static bool Pick(Camera camera, Vector2 screen, out ConnectorRef door, out FogDoor hit)
        {
            door = default;
            hit = null;
            if (camera == null) return false;

            Ray ray = camera.ScreenPointToRay(screen);
            int n = Physics.RaycastNonAlloc(ray, s_Hits, Reach, ~0, QueryTriggerInteraction.Collide);
            if (n == 0) return false;
            Array.Sort(s_Hits, 0, n, DistanceOrder.Instance);

            for (int i = 0; i < n; i++)
            {
                var found = s_Hits[i].collider.GetComponentInParent<FogDoor>();
                if (found == null) continue;
                var cube = found.GetComponentInParent<DreamCube>();
                if (cube == null || cube.IsCutAway) continue;

                ConnectorRef? which = cube.RefOf(found);
                if (which == null) continue;

                door = which.Value;
                hit = found;
                return true;
            }
            return false;
        }

        sealed class DistanceOrder : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly DistanceOrder Instance = new DistanceOrder();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }
}
