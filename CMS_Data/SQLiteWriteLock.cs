using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS_Data
{
    public static class SQLiteWriteLock
    {
        // Semaphore toàn cục: chỉ cho phép 1 thao tác ghi chạy 1 lần
        private static readonly SemaphoreSlim _writeLock = new(1, 1);

        public static async Task<T> RunAsync<T>(Func<Task<T>> action)
        {
            await _writeLock.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public static async Task RunAsync(Func<Task> action)
        {
            await _writeLock.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}
