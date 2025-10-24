namespace BehaviorTree
{
    public class ConditionNode : Node
    {
        private System.Func<bool> condition;

        public ConditionNode(System.Func<bool> condition)
        {
            this.condition = condition;
        }

        public override Status Evaluate()
        {
            return condition() ? Status.Success : Status.Failure;
        }
    }
}