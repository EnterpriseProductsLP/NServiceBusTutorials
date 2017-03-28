namespace NServiceBusTutorials.ActivePassive.Publisher.Producer
{
    internal class StateTransition
    {
        private readonly Command _command;

        private readonly State _currentState;

        public StateTransition(State currentState, Command command)
        {
            _currentState = currentState;
            _command = command;
        }

        protected bool Equals(StateTransition other)
        {
            return Equals(_command, other._command) && _currentState == other._currentState;
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
                return (_command.GetHashCode() * 397) ^ (int)_currentState;
            }
        }
    }
}