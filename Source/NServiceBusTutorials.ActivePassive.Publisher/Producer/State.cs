namespace NServiceBusTutorials.ActivePassive.Publisher.Producer
{
    internal enum State
    {
        Initializing,
        Paused,
        Running,
        Stopped
    }
}
