using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.AI
{
    [ExecuteAlways]
    public class VisionGizmos : MonoBehaviour
    {
        public EnemyData data;

        void OnDrawGizmosSelected()
        {
            if (data == null) return;

            // Center and forward direction
            Vector3 origin = transform.position;
            Vector3 facingDir = transform.right * (transform.localScale.x > 0 ? 1f : -1f);
            float range = data.detectionRange;
            float halfAngle = data.viewAngle / 2f;

#if UNITY_EDITOR
            // Draw filled red cone
            Handles.color = new Color(1f, 0f, 0f, 0.25f); // semi-transparent red
            Handles.DrawSolidArc(
                origin,
                Vector3.forward,               // axis (since this is 2D)
                Quaternion.Euler(0, 0, -halfAngle) * facingDir, // start direction
                data.viewAngle,                // sweep angle
                range                          // radius
            );

            // Draw outline for clarity
            Handles.color = Color.red;
            Handles.DrawWireArc(origin, Vector3.forward,
                Quaternion.Euler(0, 0, -halfAngle) * facingDir,
                data.viewAngle, range);

            // Draw center line for direction
            Handles.DrawLine(origin, origin + facingDir * range);
#endif
        }
    }
}
