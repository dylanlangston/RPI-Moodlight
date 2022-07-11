using System.Collections.Concurrent;

namespace WeFeelClient
{
    /// <summary>
    /// Leaky Bucket to rate limit
    /// https://dotnetcoretutorials.com/2019/11/24/implementing-a-leaky-bucket-client-in-net-core/
    /// </summary>
    class LeakyBucket
    {
        public class BucketConfiguration
        {
            public int MaxFill { get; set; }
            public TimeSpan LeakRateTimeSpan { get; set; }
            public int LeakRate { get; set; }
        }

        private readonly BucketConfiguration _bucketConfiguration;
        private readonly ConcurrentQueue<DateTime> currentItems;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private Task leakTask;

        public LeakyBucket(BucketConfiguration bucketConfiguration)
        {
            _bucketConfiguration = bucketConfiguration;
            currentItems = new ConcurrentQueue<DateTime>();
        }

        public async Task GainAccess(TimeSpan? maxWait = null)
        {
            //Only allow one thread at a time in. 
            await semaphore.WaitAsync(maxWait ?? TimeSpan.FromHours(1));
            try
            {
                //If this is the first time, kick off our thread to monitor the bucket. 
                if (leakTask == null)
                    leakTask = Task.Factory.StartNew(Leak);

                while (true)
                {
                    if (currentItems.Count >= _bucketConfiguration.MaxFill)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    currentItems.Enqueue(DateTime.UtcNow);
                    return;
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        //Infinite loop to keep leaking. 
        private void Leak()
        {
            //Wait for our first queue item. 
            while (currentItems.Count == 0)
                Thread.Sleep(250);

            while (true)
            {
                Thread.Sleep(_bucketConfiguration.LeakRateTimeSpan);
                for (int i = 0; i < currentItems.Count && i < _bucketConfiguration.LeakRate; i++)
                {
                    DateTime dequeueItem;
                    currentItems.TryDequeue(out dequeueItem);
                }
            }
        }
    }
}
