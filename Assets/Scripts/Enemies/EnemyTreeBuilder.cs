using UnityEngine;
using BehaviorTree;
using Game.Core;
using Game.Player;

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
                                    if (enemy != null)
                                    {
                                        enemy.AimAndShoot(data.aimDelay);
                                        return Node.Status.Success;
                                    }
                                    return Node.Status.Failure;
                                })
                            ),

                            new ActionNode(() =>
                            {
                                enemy.FacePlayer();
                                return Node.Status.Running;
                            })
                        )
                    ),

                    new ActionNode(() => Idle(enemy))
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
                                new ActionNode(() =>
                                {
                                    SwoopAttack(enemy, data);
                                    return Node.Status.Success;
                                })
                            ),

                            new ActionNode(() => FlyChase(enemy, data))
                        )
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

        static Node.Status ShootProjectile(EnemyBT enemy, EnemyData data)
        {
            enemy.Stop();
            enemy.FacePlayer();

            if (!CanAttack(enemy))
            {
                return Node.Status.Running;
            }

            enemy.ShootProjectile();

            return Node.Status.Success;
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

            float distance = enemy.transform.position.x - startPos.x;

            if (movingRight && distance >= data.patrolDistance)
            {
                bb.SetValue(BlackboardKeys.PatrolMovingRight, false);
            }
            else if (!movingRight && distance <= -data.patrolDistance)
            {
                bb.SetValue(BlackboardKeys.PatrolMovingRight, true);
            }

            return Node.Status.Running;
        }

        static Node.Status FlyChase(EnemyBT enemy, EnemyData data)
        {
            var bb = GetBlackboard(enemy);
            Transform player = bb.GetValue<Transform>(BlackboardKeys.Player);
            if (player == null) return Node.Status.Failure;

            Vector2 dir = (player.position - enemy.transform.position).normalized;
            enemy.GetRigidbody().velocity = dir * data.moveSpeed;

            enemy.FacePlayer();

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

        #endregion

        #region Helpers

        static Blackboard GetBlackboard(EnemyBT enemy)
        {
            var field = enemy.GetType().GetField("blackboard",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(enemy) as Blackboard;
        }

        #endregion
    }
}