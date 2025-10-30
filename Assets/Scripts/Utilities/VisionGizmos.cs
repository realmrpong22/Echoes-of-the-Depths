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

            Vector3 origin = transform.position;
            Vector3 facingDir = transform.right * (transform.localScale.x > 0 ? 1f : -1f);
            float halfAngle = data.viewAngle / 2f;

#if UNITY_EDITOR
            // --- DETECTION RANGE (YELLOW) ---
            Handles.color = new Color(1f, 1f, 0f, 0.15f); // semi-transparent yellow fill
            Handles.DrawSolidArc(
                origin,
                Vector3.forward,
                Quaternion.Euler(0, 0, -halfAngle) * facingDir,
                data.viewAngle,
                data.detectionRange
            );

            Handles.color = Color.yellow;
            Handles.DrawWireArc(
                origin,
                Vector3.forward,
                Quaternion.Euler(0, 0, -halfAngle) * facingDir,
                data.viewAngle,
                data.detectionRange
            );

            // --- ATTACK RANGE (RED) ---
            Handles.color = new Color(1f, 0f, 0f, 0.5f); // semi-transparent red fill
            Handles.DrawSolidArc(
                origin,
                Vector3.forward,
                Quaternion.Euler(0, 0, -halfAngle) * facingDir,
                data.viewAngle,
                data.attackRange
            );

            Handles.color = Color.red;
            Handles.DrawWireArc(
                origin,
                Vector3.forward,
                Quaternion.Euler(0, 0, -halfAngle) * facingDir,
                data.viewAngle,
                data.attackRange
            );

            // Direction line
            Handles.color = Color.white;
            Handles.DrawLine(origin, origin + facingDir * data.detectionRange);
#endif
        }
    }
}
