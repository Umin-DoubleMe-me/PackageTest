using System;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleMe.Framework.Helper
{
    public class AsyncLock
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<IDisposable> Lock()
        {
            await _semaphore.WaitAsync();
            return new SemaphoreHelper(_semaphore);
        }

        public sealed class SemaphoreHelper : IDisposable
        {
            private readonly SemaphoreSlim _semaphoreSlim;
            public SemaphoreHelper(SemaphoreSlim semaphoreSlim) => _semaphoreSlim = semaphoreSlim;

            public void Dispose()
            {
                if (_semaphoreSlim != null)
                {
                    _semaphoreSlim.Release();
                }
            }
        }
    }
}

