using System.Threading.Tasks;

namespace NServiceBusTutorials.ActivePassive.Consumer.Interfaces
{
    internal interface IManageDistributedLocks
    {
        /// <summary>
        /// Gets or updates the distributed lock.
        /// </summary>
        /// <returns>True on success.  False on failure.</returns>
        /// <remarks>Implementation should deal with any exceptions internally, and swallow them.</remarks>
        Task<bool> GetOrMaintainLock();

        /// <summary>
        /// Releases the distributed lock.
        /// </summary>
        /// <remarks>Implementation should deal with any exceptions internally, and swallow them.</remarks>
        Task ReleaseLock();
    }
}