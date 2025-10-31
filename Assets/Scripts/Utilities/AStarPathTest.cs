using UnityEngine;
using System.Collections.Generic;

namespace Game.AI
{
    public class AStarPathTest : MonoBehaviour
    {
        public Transform startPoint;
        public Transform endPoint;
        private List<Vector3> path;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (TilemapNavGrid.Instance == null)
                {
                    Debug.LogError("NavGrid not found!");
                    return;
                }

                path = AStarPathfinder.FindPath(startPoint.position, endPoint.position);
                Debug.Log($"Path generated with {path.Count} nodes.");
            }

            // Optional: visualize path in play mode
            if (path != null && path.Count > 1)
            {
                for (int i = 0; i < path.Count - 1; i++)
                    Debug.DrawLine(path[i], path[i + 1], Color.cyan);
            }
        }
    }
}
