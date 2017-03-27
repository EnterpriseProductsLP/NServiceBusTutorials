namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
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
