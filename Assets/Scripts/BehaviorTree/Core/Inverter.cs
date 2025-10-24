namespace BehaviorTree
{
    public class Inverter : Node
    {
        private Node child;

        public Inverter(Node node)
        {
            child = node;
        }

        public override void Initialize(UnityEngine.Transform transform, UnityEngine.Rigidbody2D rb, Blackboard blackboard)
        {
            base.Initialize(transform, rb, blackboard);
            child.Initialize(transform, rb, blackboard);
        }

        public override Status Evaluate()
        {
            Status status = child.Evaluate();

            if (status == Status.Success)
                return Status.Failure;
            else if (status == Status.Failure)
                return Status.Success;

            return Status.Running;
        }
    }
}