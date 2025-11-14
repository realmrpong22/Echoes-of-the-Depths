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

        public static Node BuildMeleePatrolTree(EnemyBT enemy, EnemyData data)
        {
            return new Selector(
                BuildDeathSequence(enemy),
                new ActionNode(() => Patrol(enemy, data))
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
                        new ConditionNode(() => HasDetectedOnce(enemy)),
                        new ActionNode(() => FlyChasePathfinding(enemy, data))
                    ),

                    new Sequence(
                        new ConditionNode(() => IsPlayerDetected(enemy, data)),
                        new ActionNode(() =>
                        {
                            var bb = GetBlackboard(enemy);
                            bb.SetValue("hasDetectedPlayer", true);
                            return Node.Status.Success;
                        })
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
            Rigidbody2D rb = enemy.GetRigidbody();
            LayerMask groundMask = enemy.GetGroundLayer();
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);
            if (player == null) return Node.Status.Failure;

            List<Vector3> path = bb.GetValue<List<Vector3>>("path");

            if (path == null || path.Count == 0)
            {
                path = AStarPathfinder.FindPath(enemy.transform.position, player.position);
                bb.SetValue("path", path);
                enemy.currentPath = path;

                if (path == null || path.Count == 0)
                {
                    rb.velocity = Vector2.zero;
                    return Node.Status.Running;
                }
            }

            Vector3 target = path[0];

            if (Physics2D.Linecast(enemy.transform.position, target, groundMask))
            {
                path = AStarPathfinder.FindPath(enemy.transform.position, player.position);
                bb.SetValue("path", path);
                enemy.currentPath = path;
                if (path == null || path.Count == 0)
                {
                    rb.velocity = Vector2.zero;
                    return Node.Status.Running;
                }
                target = path[0];
            }

            Vector2 dir = ((Vector2)target - (Vector2)enemy.transform.position).normalized;
            rb.velocity = Vector2.Lerp(rb.velocity, dir * data.moveSpeed, Time.deltaTime * 5f);
            enemy.FacePlayer();

            if (Vector2.Distance(enemy.transform.position, target) < 0.25f)
                path.RemoveAt(0);

            bb.SetValue("path", path);
            enemy.currentPath = path;
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

        static bool HasDetectedOnce(EnemyBT enemy)
        {
            var bb = GetBlackboard(enemy);
            return bb.GetValue<bool>("hasDetectedPlayer", false);
        }
        #endregion
    }
}