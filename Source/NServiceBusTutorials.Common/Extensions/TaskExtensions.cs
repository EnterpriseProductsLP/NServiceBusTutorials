using System.Threading.Tasks;

namespace NServiceBusTutorials.Common.Extensions
{
    public static class TaskExtensions
    {
        public static void Ignore(this Task task)
        {
        }

        public static void Inline(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static TResult Inline<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
