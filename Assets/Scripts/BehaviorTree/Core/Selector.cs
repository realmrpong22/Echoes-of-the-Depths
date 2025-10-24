using System.Collections.Generic;

namespace BehaviorTree
{
    public class Selector : Node
    {
        private List<Node> children = new List<Node>();

        public Selector(params Node[] nodes)
        {
            children.AddRange(nodes);
        }

        public override void Initialize(UnityEngine.Transform transform, UnityEngine.Rigidbody2D rb, Blackboard blackboard)
        {
            base.Initialize(transform, rb, blackboard);
            foreach (Node child in children)
            {
                child.Initialize(transform, rb, blackboard);
            }
        }

        public override Status Evaluate()
        {
            foreach (Node child in children)
            {
                Status status = child.Evaluate();

                if (status == Status.Success)
                    return Status.Success;

                if (status == Status.Running)
                    return Status.Running;
            }

            return Status.Failure;
        }
    }
}