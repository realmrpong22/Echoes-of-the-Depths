using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Blackboard
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
        }

        public T GetValue<T>(string key)
        {
            if (data.ContainsKey(key))
            {
                return (T)data[key];
            }
            return default(T);
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            if (data.ContainsKey(key))
            {
                return (T)data[key];
            }
            return defaultValue;
        }

        public bool HasValue(string key)
        {
            return data.ContainsKey(key);
        }

        public void RemoveValue(string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }
        }

        public void Clear()
        {
            data.Clear();
        }

        public void DebugPrint()
        {
            Debug.Log("=== Blackboard Contents ===");
            foreach (var kvp in data)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
    }

    public static class BlackboardKeys
    {
        public const string Player = "player";
        public const string PlayerPosition = "playerPosition";
        public const string PlayerDistance = "playerDistance";

        public const string PatrolStartPosition = "patrolStartPos";
        public const string PatrolMovingRight = "patrolMovingRight";
        public const string PatrolWaitTimer = "patrolWaitTimer";

        public const string AttackTimer = "attackTimer";
        public const string IsAttacking = "isAttacking";
        public const string LastAttackTime = "lastAttackTime";

        public const string IsDead = "isDead";
        public const string CurrentHealth = "currentHealth";
        public const string AggroTarget = "aggroTarget";
    }
}