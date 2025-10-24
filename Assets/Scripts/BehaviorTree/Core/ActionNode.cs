namespace BehaviorTree
{
    public class ActionNode : Node
    {
        private System.Func<Status> action;

        public ActionNode(System.Func<Status> action)
        {
            this.action = action;
        }

        public override Status Evaluate()
        {
            return action();
        }
    }
}