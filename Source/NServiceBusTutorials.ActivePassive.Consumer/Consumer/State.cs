namespace NServiceBusTutorials.ActivePassive.Consumer.Consumer
{
    internal enum State
    {
        Initializing,
        Paused,
        Running,
        Stopped,
        Waiting
    }
}
