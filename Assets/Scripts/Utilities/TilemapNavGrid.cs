using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEditor;

namespace Game.AI
{
    public class TilemapNavGrid : MonoBehaviour
    {
        public static TilemapNavGrid Instance { get; private set; }

        [Tooltip("Collision tilemap used for obstacle detection")]
        public Tilemap collisionTilemap;

        [Tooltip("How large each node step is (in tiles). Usually 1.")]
        public float cellSize = 1f;

        private HashSet<Vector2Int> blockedCells = new();
        private BoundsInt mapBounds;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildGrid();
        }

        public void BuildGrid()
        {
            if (collisionTilemap == null)
            {
                Debug.LogError("TilemapNavGrid: No collision tilemap assigned!");
                return;
            }

            blockedCells.Clear();
            mapBounds = collisionTilemap.cellBounds;

            foreach (var pos in mapBounds.allPositionsWithin)
            {
                if (collisionTilemap.HasTile(pos))
                    blockedCells.Add((Vector2Int)pos);
            }

            Debug.Log($"[TilemapNavGrid] Grid baked: {mapBounds.size.x}x{mapBounds.size.y}, {blockedCells.Count} blocked tiles");
        }

        public bool IsWalkable(Vector2Int cellPos)
        {
            if (!mapBounds.Contains(new Vector3Int(cellPos.x, cellPos.y, 0)))
                return false;

            return !blockedCells.Contains(cellPos);
        }


        public Vector3 CellToWorld(Vector2Int cell)
        {
            Vector3Int c = new(cell.x, cell.y, 0);
            return collisionTilemap.CellToWorld(c) + collisionTilemap.cellSize / 2;
        }

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            Vector3Int c = collisionTilemap.WorldToCell(worldPos);
            return new Vector2Int(c.x, c.y);
        }

    #if UNITY_EDITOR
        [Header("Gizmos (Editor Only)")]
        public bool showGrid = true;
        public bool showBlocked = true;
        public bool showLastPath = true;

        private List<Vector3> lastPath;

        public void SetLastPath(List<Vector3> path)
        {
            lastPath = path;
        }

        void OnDrawGizmos()
        {
            if (!showGrid || collisionTilemap == null)
                return;

            Vector3 cellSize = collisionTilemap.cellSize;
            Vector3 halfCell = cellSize / 2f;

            // Draw grid nodes
            foreach (var pos in mapBounds.allPositionsWithin)
            {
                Vector3 worldPos = collisionTilemap.CellToWorld(pos) + halfCell;

                bool isBlocked = blockedCells.Contains((Vector2Int)pos);
                Gizmos.color = isBlocked ? new Color(1f, 0f, 0f, 0.4f) : new Color(0f, 1f, 0f, 0.15f);

                if (showBlocked || !isBlocked)
                    Gizmos.DrawCube(worldPos, cellSize * 0.95f);
            }

            // Draw last computed path (blue)
            if (showLastPath && lastPath != null && lastPath.Count > 1)
            {
                Handles.color = Color.cyan;
                for (int i = 0; i < lastPath.Count - 1; i++)
                    Handles.DrawLine(lastPath[i], lastPath[i + 1]);
            }
        }
    #endif
    }
}
