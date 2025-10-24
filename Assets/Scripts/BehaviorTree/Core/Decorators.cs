namespace BehaviorTree
{
    public class Succeeder : Node
    {
        private Node child;

        public Succeeder(Node node)
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
            child.Evaluate();
            return Status.Success;
        }
    }

    public class Repeater : Node
    {
        private Node child;

        public Repeater(Node node)
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
            child.Evaluate();
            return Status.Running;
        }
    }

    public class UntilFail : Node
    {
        private Node child;

        public UntilFail(Node node)
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

            if (status == Status.Failure)
                return Status.Success;

            return Status.Running;
        }
    }
}