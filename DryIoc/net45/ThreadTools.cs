using System.Threading;

namespace DryIoc
{
    public static partial class ThreadTools
    {
        static partial void GetCurrentManagedThreadID(ref int threadID)
        {
            threadID = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
