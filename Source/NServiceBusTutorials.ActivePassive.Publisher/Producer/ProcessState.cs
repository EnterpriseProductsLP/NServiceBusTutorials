namespace NServiceBusTutorials.ActivePassive.Publisher.Producer
{
    internal enum ProcessState
    {
        Initializing,
        Paused,
        Running,
        Stopped
    }
}
