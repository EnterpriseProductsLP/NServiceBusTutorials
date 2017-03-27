namespace NServiceBusTutorials.ActivePassive.Consumer.StateMachine
{
    internal enum ProcessState
    {
        Initializing,
        Paused,
        Running,
        Stopped,
        Waiting
    }
}
