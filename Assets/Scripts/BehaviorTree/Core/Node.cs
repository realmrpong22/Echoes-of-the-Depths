using UnityEngine;

namespace BehaviorTree
{
    public abstract class Node
    {
        public enum Status { Success, Failure, Running }

        public abstract Status Evaluate();

        // Reference to the AI agent for nodes that need it
        protected Transform transform;
        protected Rigidbody2D rb;
        protected Blackboard blackboard;

        public virtual void Initialize(Transform transform, Rigidbody2D rb, Blackboard blackboard)
        {
            this.transform = transform;
            this.rb = rb;
            this.blackboard = blackboard;
        }

        public virtual void Initialize(Transform transform, Rigidbody2D rb)
        {
            this.transform = transform;
            this.rb = rb;
        }
    }
}