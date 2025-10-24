using UnityEngine;
using BehaviorTree;

/// <summary>
/// Factory class for building behavior trees for different enemy types.
/// </summary>
public static class EnemyTreeBuilder
{
    /// <summary>
    /// Build behavior tree for Melee enemies (patrol, chase, melee attack).
    /// 
    /// Tree Structure:
    /// ROOT (Selector)
    /// ├─ Death Sequence (isDead? → Die)
    /// └─ Alive Sequence
    ///    ├─ Player Detected?
    ///    │  └─ Combat Selector
    ///    │     ├─ Attack Sequence (In Range? → Attack)
    ///    │     └─ Chase
    ///    └─ Patrol
    /// </summary>
    public static Node BuildMeleeTree(EnemyBT enemy, EnemyData data)
    {
        return new Selector(
            // Death Check (highest priority)
            BuildDeathSequence(enemy),

            // Alive Behavior
            new Selector(
                // Combat (if player detected)
                new Sequence(
                    new ConditionNode(() => IsPlayerDetected(enemy, data)),
                    new Selector(
                        // Try to attack if in range
                        new Sequence(
                            new ConditionNode(() => IsInAttackRange(enemy, data)),
                            new ActionNode(() => AttackMelee(enemy, data))
                        ),
                        // Otherwise chase
                        new ActionNode(() => Chase(enemy, data))
                    )
                ),

                // Patrol (fallback when no player)
                new ActionNode(() => Patrol(enemy, data))
            )
        );
    }

    /// <summary>
    /// Build behavior tree for Ranged enemies (stationary, shoot at player).
    /// 
    /// Tree Structure:
    /// ROOT (Selector)
    /// ├─ Death Sequence
    /// └─ Alive Sequence
    ///    ├─ Player Detected?
    ///    │  └─ Attack Sequence (In Range? → Shoot)
    ///    └─ Idle
    /// </summary>
    public static Node BuildRangedTree(EnemyBT enemy, EnemyData data)
    {
        return new Selector(
            BuildDeathSequence(enemy),

            new Selector(
                // Combat
                new Sequence(
                    new ConditionNode(() => IsPlayerDetected(enemy, data)),
                    new ConditionNode(() => IsInAttackRange(enemy, data)),
                    new ActionNode(() => ShootProjectile(enemy, data))
                ),

                // Idle
                new ActionNode(() => Idle(enemy))
            )
        );
    }

    /// <summary>
    /// Build behavior tree for Guardian enemies (similar to melee but more aggressive).
    /// TODO: Add jump/ranged stagger mechanics later
    /// </summary>
    public static Node BuildGuardianTree(EnemyBT enemy, EnemyData data)
    {
        // For now, use melee tree
        // Later can add special guardian behaviors
        return BuildMeleeTree(enemy, data);
    }

    /// <summary>
    /// Build behavior tree for Air enemies (flying patrol and swoop attacks).
    /// TODO: Implement flying movement patterns
    /// </summary>
    public static Node BuildAirTree(EnemyBT enemy, EnemyData data)
    {
        // Placeholder - similar to ranged for now
        return BuildRangedTree(enemy, data);
    }

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
        return enemy.GetComponent<EnemyBT>()
            .GetType()
            .GetField("blackboard", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(enemy) is Blackboard bb && bb.GetValue<bool>(BlackboardKeys.IsDead);
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
        float distance = bb.GetValue<float>(BlackboardKeys.PlayerDistance, float.MaxValue);
        return distance <= data.detectionRange;
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

    #region Actions

    static Node.Status Idle(EnemyBT enemy)
    {
        enemy.Stop();
        enemy.FacePlayer();
        return Node.Status.Running;
    }

    static Node.Status Patrol(EnemyBT enemy, EnemyData data)
    {
        var bb = GetBlackboard(enemy);

        // Check wait timer
        float waitTimer = bb.GetValue<float>(BlackboardKeys.PatrolWaitTimer);
        if (waitTimer > 0f)
        {
            enemy.Stop();
            return Node.Status.Running;
        }

        // Get patrol state
        Vector3 startPos = bb.GetValue<Vector3>(BlackboardKeys.PatrolStartPosition);
        bool movingRight = bb.GetValue<bool>(BlackboardKeys.PatrolMovingRight);

        // Move
        float direction = movingRight ? 1f : -1f;
        enemy.Move(direction);

        // Check if reached patrol limit
        float distanceFromStart = enemy.transform.position.x - startPos.x;

        if (movingRight && distanceFromStart >= data.patrolDistance)
        {
            // Reached right limit
            bb.SetValue(BlackboardKeys.PatrolMovingRight, false);
            bb.SetValue(BlackboardKeys.PatrolWaitTimer, data.patrolWaitTime);
        }
        else if (!movingRight && distanceFromStart <= -data.patrolDistance)
        {
            // Reached left limit
            bb.SetValue(BlackboardKeys.PatrolMovingRight, true);
            bb.SetValue(BlackboardKeys.PatrolWaitTimer, data.patrolWaitTime);
        }

        // Check for edge/wall
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

        // Move towards player
        float direction = player.position.x > enemy.transform.position.x ? 1f : -1f;
        enemy.Move(direction);

        return Node.Status.Running;
    }

    static Node.Status AttackMelee(EnemyBT enemy, EnemyData data)
    {
        enemy.Stop();
        enemy.FacePlayer();

        // Check if can attack
        if (!CanAttack(enemy))
        {
            return Node.Status.Running;
        }

        // Perform attack
        enemy.PerformMeleeAttack();

        return Node.Status.Success;
    }

    static Node.Status ShootProjectile(EnemyBT enemy, EnemyData data)
    {
        enemy.Stop();
        enemy.FacePlayer();

        // Check if can attack
        if (!CanAttack(enemy))
        {
            return Node.Status.Running;
        }

        // Shoot
        enemy.ShootProjectile();

        return Node.Status.Success;
    }

    #endregion

    #region Helper

    static Blackboard GetBlackboard(EnemyBT enemy)
    {
        // Use reflection to access private blackboard field
        var field = enemy.GetType().GetField("blackboard",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(enemy) as Blackboard;
    }

    #endregion
}