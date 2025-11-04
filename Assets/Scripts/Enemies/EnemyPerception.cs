using UnityEngine;
using BehaviorTree;

namespace Game.AI
{
    public class EnemyPerception
    {
        private readonly Transform enemyTransform;
        private readonly EnemyData data;
        private readonly LayerMask visionBlockers;

        public EnemyPerception(Transform enemyTransform, EnemyData data, LayerMask visionBlockers)
        {
            this.enemyTransform = enemyTransform;
            this.data = data;
            this.visionBlockers = visionBlockers;
        }

        public void UpdatePerception(Blackboard bb, Transform player)
        {
            if (player == null) return;

            Vector2 toPlayer = player.position - enemyTransform.position;
            float distance = toPlayer.magnitude;
            bb.SetValue(BlackboardKeys.PlayerDistance, distance);
            bb.SetValue(BlackboardKeys.PlayerPosition, player.position);

            bool detected = false;

            if (distance <= data.detectionRange)
            {
                Vector2 facingDir = enemyTransform.right * (enemyTransform.localScale.x > 0 ? 1f : -1f);
                float angle = Vector2.Angle(facingDir, toPlayer.normalized);

                if (angle <= data.viewAngle * 0.5f)
                {
                    if (data.ignoreVisionBlockers)
                    {
                        detected = true;
                    }
                    else if (!Physics2D.Raycast(enemyTransform.position, toPlayer.normalized, distance, visionBlockers))
                    {
                        detected = true;
                    }
                }
            }

            bb.SetValue("playerDetected", detected);
            //if (detected)
                //Debug.Log($"[{enemyTransform.name}] Perception: Player detected (dist={distance:F1})");
            //else
                //Debug.Log($"[{enemyTransform.name}] Perception: Player lost or out of sight (dist={distance:F1})");
        }
    }
}
