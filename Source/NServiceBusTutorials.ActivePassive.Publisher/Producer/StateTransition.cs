namespace NServiceBusTutorials.ActivePassive.Publisher.Producer
{
    internal class StateTransition
    {
        private readonly Command Command;

        private readonly ProcessState CurrentState;

        public StateTransition(ProcessState currentState, Command command)
        {
            CurrentState = currentState;
            Command = command;
        }

        protected bool Equals(StateTransition other)
        {
            return Equals(Command, other.Command) && CurrentState == other.CurrentState;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((StateTransition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Command.GetHashCode() * 397) ^ (int)CurrentState;
            }
        }
    }
}
