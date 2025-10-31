using UnityEngine;
using BehaviorTree;
using Game.Core;
using Game.Player;
using System.Collections.Generic;

namespace Game.AI
{
    public static class EnemyTreeBuilder
    {
        #region Public Builders

        public static Node BuildMeleeTree(EnemyBT enemy, EnemyData data)
        {
            return new Selector(
                BuildDeathSequence(enemy),

                new Selector(
                    new Sequence(
                        new ConditionNode(() => IsPlayerDetected(enemy, data)),
                        new Selector(
                            new Sequence(
                                new ConditionNode(() => IsInAttackRange(enemy, data)),
                                new ActionNode(() => AttackMelee(enemy, data))
                            ),
                            new ActionNode(() => Chase(enemy, data))
                        )
                    ),

                    new ActionNode(() => Patrol(enemy, data))
                )
            );
        }

        public static Node BuildRangedTree(EnemyBT enemy, EnemyData data)
        {
            return new Selector(
                BuildDeathSequence(enemy),

                new Selector(

                    new Sequence(
                        new ConditionNode(() => IsPlayerDetected(enemy, data)),

                        new Selector(

                            new Sequence(
                                new ConditionNode(() => IsInAttackRange(enemy, data)),
                                new ConditionNode(() => CanAttack(enemy)),
                                new ActionNode(() =>
                                {
                                    enemy.Stop();
                                    enemy.FacePlayer();
                                    enemy.AimAndShoot(data.aimDelay);
                                    return Node.Status.Success;
                                })
                            ),

                            new ActionNode(() => MoveToPlayer(enemy, data))
                        )
                    ),

                    new ActionNode(() => Patrol(enemy, data))
                )
            );
        }



        public static Node BuildGuardianTree(EnemyBT enemy, EnemyData data)
        {
            return BuildMeleeTree(enemy, data);
        }

        public static Node BuildAirTree(EnemyBT enemy, EnemyData data)
        {
            return new Selector(
                BuildDeathSequence(enemy),

                new Selector(
                    new Sequence(
                        new ConditionNode(() => IsPlayerDetected(enemy, data)),
                        new Selector(
                            new Sequence(
                                new ConditionNode(() => IsInAttackRange(enemy, data)),
                                new ConditionNode(() => CanAttack(enemy)),
                                new ActionNode(() =>
                                {
                                    enemy.Stop();
                                    enemy.FacePlayer();
                                    enemy.AimAndShoot(data.aimDelay);
                                    return Node.Status.Success;
                                })
                            ),
                            new ActionNode(() => FlyChasePathfinding(enemy, data))
                        )
                    ),

                    new Sequence(
                        new ConditionNode(() => IsReturningToPatrol(enemy)),
                        new ActionNode(() => FlyReturnToPatrol(enemy, data))
                    ),

                    new ActionNode(() => FlyPatrol(enemy, data))
                )
            );
        }


        #endregion

        #region Death Sequence

        static Node BuildDeathSequence(EnemyBT enemy)
        {
            return new Sequence(
                new ConditionNode(() => IsDead(enemy)),
                new ActionNode(() => ExecuteDeath(enemy))
            );
        }

        static bool IsDead(EnemyBT enemy)
        {
            var field = enemy.GetType().GetField("blackboard",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field?.GetValue(enemy) is Blackboard bb)
            {
                return bb.GetValue<bool>(BlackboardKeys.IsDead);
            }
            return false;
        }

        static Node.Status ExecuteDeath(EnemyBT enemy)
        {
            enemy.Die();
            return Node.Status.Success;
        }

        #endregion

        #region Conditions

        static bool IsPlayerDetected(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            return bb.GetValue<bool>("playerDetected", false);
        }

        static bool IsInAttackRange(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            float distance = bb.GetValue<float>(BlackboardKeys.PlayerDistance, float.MaxValue);
            return distance <= data.attackRange;
        }

        static bool CanAttack(EnemyBT enemy)
        {
            var bb = GetBlackboard(enemy);
            float attackTimer = bb.GetValue<float>(BlackboardKeys.AttackTimer);
            bool isAttacking = bb.GetValue<bool>(BlackboardKeys.IsAttacking);
            return attackTimer <= 0f && !isAttacking;
        }

        #endregion

        #region Actions - Grounded Enemies

        static Node.Status Idle(EnemyBT enemy)
        {
            enemy.Stop();
            enemy.FacePlayer();
            return Node.Status.Running;
        }

        static Node.Status Patrol(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);

            float waitTimer = bb.GetValue<float>(BlackboardKeys.PatrolWaitTimer);
            if (waitTimer > 0f)
            {
                enemy.Stop();
                return Node.Status.Running;
            }

            Vector3 startPos = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition);
            bool movingRight = bb.GetValue<bool>(BlackboardKeys.PatrolMovingRight);

            float direction = movingRight ? 1f : -1f;
            enemy.Move(direction);

            float distanceFromStart = enemy.transform.position.x - startPos.x;

            if (movingRight && distanceFromStart >= data.patrolDistance)
            {
                bb.SetValue(BlackboardKeys.PatrolMovingRight, false);
                bb.SetValue(BlackboardKeys.PatrolWaitTimer, data.patrolWaitTime);
            }
            else if (!movingRight && distanceFromStart <= -data.patrolDistance)
            {
                bb.SetValue(BlackboardKeys.PatrolMovingRight, true);
                bb.SetValue(BlackboardKeys.PatrolWaitTimer, data.patrolWaitTime);
            }

            if (!enemy.IsGroundAhead() && waitTimer <= 0f)
            {
                bb.SetValue(BlackboardKeys.PatrolMovingRight, !movingRight);
                bb.SetValue(BlackboardKeys.PatrolWaitTimer, data.patrolWaitTime);
            }

            return Node.Status.Running;
        }

        static Node.Status Chase(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);

            if (player == null)
            {
                return Node.Status.Failure;
            }

            float direction = player.position.x > enemy.transform.position.x ? 1f : -1f;
            enemy.Move(direction);

            return Node.Status.Running;
        }

        static Node.Status AttackMelee(EnemyBT enemy, EnemyData data)
        {
            enemy.Stop();
            enemy.FacePlayer();

            if (!CanAttack(enemy))
            {
                return Node.Status.Running;
            }

            enemy.PerformMeleeAttack();

            return Node.Status.Success;
        }

        static Node.Status MoveToPlayer(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);
            if (player == null) return Node.Status.Failure;

            float distance = bb.GetValue<float>(BlackboardKeys.PlayerDistance);

            if (distance <= data.attackRange)
            {
                enemy.Stop();
                return Node.Status.Success;
            }

            float dir = player.position.x > enemy.transform.position.x ? 1f : -1f;

            if (!enemy.IsGroundAhead())
            {
                enemy.Stop();
                if (Mathf.Abs(distance) > data.attackRange)
                    enemy.FacePlayer();
                return Node.Status.Running;
            }

            enemy.Move(dir);
            enemy.FacePlayer();

            return Node.Status.Running;
        }



        #endregion

        #region Actions - Flying Enemies

        static Node.Status FlyPatrol(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);

            Vector3 startPos = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition);
            bool movingRight = bb.GetValue<bool>(BlackboardKeys.PatrolMovingRight);

            float direction = movingRight ? 1f : -1f;
            enemy.Move(direction);
            enemy.GetRigidbody().velocity = new Vector2(direction * data.moveSpeed, Mathf.Sin(Time.time * 1.5f) * 0.5f);

            float distance = enemy.transform.position.x - startPos.x;

            if (movingRight && distance >= data.patrolDistance)
                bb.SetValue(BlackboardKeys.PatrolMovingRight, false);
            else if (!movingRight && distance <= -data.patrolDistance)
                bb.SetValue(BlackboardKeys.PatrolMovingRight, true);

            return Node.Status.Running;
        }

        static Node.Status FlyChasePathfinding(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);
            if (player == null)
            {
                Debug.LogWarning($"[{enemy.name}] FlyChasePathfinding: Player null.");
                return Node.Status.Failure;
            }

            Rigidbody2D rb = enemy.GetRigidbody();
            LayerMask groundMask = enemy.GetGroundLayer();

            // --- Build or reuse path ---
            List<Vector3> path = bb.GetValue<List<Vector3>>("path");
            if (path == null || path.Count == 0)
            {
                path = AStarPathfinder.FindPath(enemy.transform.position, player.position);
                enemy.currentPath = path;
                bb.SetValue("path", path);

                Debug.Log($"[{enemy.name}] Chase: Created path to player ({path.Count} nodes). Start: {enemy.transform.position:F2}, Target: {player.position:F2}");

                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning($"[{enemy.name}] Chase: No valid path found to player!");
                    enemy.Stop();
                    return Node.Status.Failure;
                }
            }

            // --- Movement toward next node ---
            Vector3 target = path[0];
            Vector2 dir = ((Vector2)target - (Vector2)enemy.transform.position).normalized;

            // --- Recompute if blocked ---
            if (Physics2D.Linecast(enemy.transform.position, target, groundMask))
            {
                Debug.Log($"[{enemy.name}] Chase: Line blocked -> Recomputing path...");
                path = AStarPathfinder.FindPath(enemy.transform.position, player.position);
                bb.SetValue("path", path);
                enemy.currentPath = path;

                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning($"[{enemy.name}] Chase: Repath failed, stopping.");
                    rb.velocity = Vector2.zero;
                    return Node.Status.Failure;
                }

                target = path[0];
                dir = ((Vector2)target - (Vector2)enemy.transform.position).normalized;
            }

            // --- Apply movement ---
            rb.velocity = Vector2.Lerp(rb.velocity, dir * data.moveSpeed, Time.deltaTime * 8f);
            enemy.FacePlayer();

            // --- Node reached? ---
            if (Vector2.Distance(enemy.transform.position, target) < 0.25f)
            {
                Debug.Log($"[{enemy.name}] Chase: Node reached ({target:F2}). Remaining: {path.Count - 1}");
                path.RemoveAt(0);
            }

            bb.SetValue("path", path);
            enemy.currentPath = path;

            // --- Check detection ---
            bool playerDetected = bb.GetValue<bool>("playerDetected", false);
            if (!playerDetected)
            {
                Debug.Log($"[{enemy.name}] Chase: Lost player! Building return path to patrol...");

                bb.TryRemove("path");

                Vector3 patrolStart = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition) + Vector3.up * 0.5f;
                var returnPath = AStarPathfinder.FindPath(enemy.transform.position, patrolStart);

                if (returnPath != null && returnPath.Count > 0)
                {
                    bb.SetValue(BlackboardKeys.ReturnPath, returnPath);
                    bb.SetValue(BlackboardKeys.ReturningToPatrol, true);
                    enemy.currentPath = returnPath;
                    Debug.Log($"[{enemy.name}] Chase: New return path built ({returnPath.Count} nodes). First node: {returnPath[0]:F2}");
                }
                else
                {
                    Debug.LogWarning($"[{enemy.name}] Chase: Failed to build return path!");
                    bb.SetValue(BlackboardKeys.ReturningToPatrol, false);
                }

                rb.velocity = Vector2.zero;
                return Node.Status.Success; // End chase immediately
            }

            return Node.Status.Running;
        }

        static Node.Status SwoopAttack(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);
            if (player == null) return Node.Status.Failure;

            Vector2 dir = (player.position - enemy.transform.position).normalized;
            enemy.GetRigidbody().velocity = dir * (data.moveSpeed * 1.5f);

            if (enemy.enemyData != null)
                AudioManager.Instance?.PlaySFX(enemy.enemyData.attackSFX);

            if (enemy.GetAnimator() != null)
                enemy.GetAnimator().SetTrigger("Attack");

            float dist = Vector2.Distance(enemy.transform.position, player.position);
            if (dist < data.attackRange * 0.8f)
            {
                var target = player.GetComponent<IDamageable>();
                if (target != null)
                    target.TakeDamage(data.attackDamage);
            }

            return Node.Status.Success;
        }

        static Node.Status FlyReturnToPatrol(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Rigidbody2D rb = enemy.GetRigidbody();
            LayerMask groundMask = enemy.GetGroundLayer();

            List<Vector3> path = bb.GetValue<List<Vector3>>(BlackboardKeys.ReturnPath);
            if (path == null || path.Count == 0)
            {
                Vector3 patrolStart = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition) + Vector3.up * 0.5f;
                path = AStarPathfinder.FindPath(enemy.transform.position, patrolStart);

                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning($"[{enemy.name}] Return: Could not find path back to patrol area!");
                    rb.velocity = Vector2.zero;
                    bb.SetValue(BlackboardKeys.ReturningToPatrol, false);
                    return Node.Status.Failure;
                }

                bb.SetValue(BlackboardKeys.ReturnPath, path);
                enemy.currentPath = path;
                Debug.Log($"[{enemy.name}] Return: Path to patrol built ({path.Count} nodes). Target: {patrolStart:F2}");
            }

            Vector3 target = path[0];
            Vector2 dir = ((Vector2)target - (Vector2)enemy.transform.position).normalized;

            // --- Line blocked? Repath ---
            if (Physics2D.Linecast(enemy.transform.position, target, groundMask))
            {
                Debug.Log($"[{enemy.name}] Return: Path blocked -> Recomputing...");
                Vector3 patrolStart = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition) + Vector3.up * 0.5f;
                path = AStarPathfinder.FindPath(enemy.transform.position, patrolStart);
                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning($"[{enemy.name}] Return: Repath failed. Stopping.");
                    rb.velocity = Vector2.zero;
                    bb.SetValue(BlackboardKeys.ReturningToPatrol, false);
                    return Node.Status.Failure;
                }

                bb.SetValue(BlackboardKeys.ReturnPath, path);
                enemy.currentPath = path;
                target = path[0];
                dir = ((Vector2)target - (Vector2)enemy.transform.position).normalized;
            }

            // --- Move along path ---
            rb.velocity = Vector2.Lerp(rb.velocity, dir * data.moveSpeed, Time.deltaTime * 8f);
            enemy.FacePlayer();

            // --- Node reached ---
            if (Vector2.Distance(enemy.transform.position, target) < 0.25f)
            {
                Debug.Log($"[{enemy.name}] Return: Node reached ({target:F2}). Remaining: {path.Count - 1}");
                path.RemoveAt(0);
            }

            bb.SetValue(BlackboardKeys.ReturnPath, path);
            enemy.currentPath = path;

            // --- Completed path ---
            if (path.Count == 0)
            {
                rb.velocity = Vector2.zero;
                bb.SetValue(BlackboardKeys.ReturningToPatrol, false);
                bb.TryRemove(BlackboardKeys.ReturnPath);
                Debug.Log($"[{enemy.name}] Return: Arrived at patrol origin. Switching to idle.");
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }


        #endregion

        #region Helpers

        static Blackboard GetBlackboard(EnemyBT enemy)
        {
            var field = enemy.GetType().GetField("blackboard",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(enemy) as Blackboard;
        }

        static bool IsReturningToPatrol(EnemyBT enemy)
        {
            var bb = GetBlackboard(enemy);
            return bb.GetValue<bool>("returningToPatrol", false);
        }

        #endregion
    }
}