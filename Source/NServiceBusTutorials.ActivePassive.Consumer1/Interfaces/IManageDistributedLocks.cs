namespace NServiceBusTutorials.ActivePassive.Consumer.Interfaces
{
    internal interface IManageDistributedLocks
    {
        bool GetOrMaintainLock();

        void ReleaseLock();
    }
}
