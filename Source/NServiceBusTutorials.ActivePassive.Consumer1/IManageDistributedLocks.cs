namespace NServiceBusTutorials.ActivePassive.Consumer
{
    internal interface IManageDistributedLocks
    {
        bool GetOrMaintainLock();

        void ReleaseLock();
    }
}
