using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPI_Moodlight
{
    // Heavily inspire by
    // https://anduin.aiursoft.com/post/2021/9/25/retry-with-exponetial-backoff-on-c
    internal class RetryWithExponentialBackOff
    {
        static Random random = new Random();

        public static void Retry(Action action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => RetryAsync(Task.Run(action), attempts, when, cancellationToken).RunSync();
        public static T Retry<T>(Func<T> action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => RetryAsync(Task.Run(action), attempts, when, cancellationToken).RunSync();
        public static void Retry(Task action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => RetryAsync(action, attempts, when, cancellationToken).RunSync();
        public static T Retry<T>(Task<T> action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => RetryAsync(action, attempts, when, cancellationToken).RunSync();
        public static async Task RetryAsync(Action action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => await RetryAsync(action, attempts, when, cancellationToken);
        public static async Task<T> RetryAsync<T>(Func<T> action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => await RetryAsync(Task.Run(action), attempts, when, cancellationToken);
        public static async Task RetryAsync(Task action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
            => await RetryAsync<object>(Task.Run(async () =>
            {
                await action;
                return default(object);
            }), 
            attempts, when, cancellationToken);
        public static async Task<T> RetryAsync<T>(Task<T> action, int attempts = 10, Predicate<Exception> when = null, CancellationToken? cancellationToken = null)
        {
            for (var i = 1; i < attempts; i++)
            {
                try
                {
                    action.Wait(cancellationToken);
                    var r = await action.WaitAsync(cancellationToken ?? new CancellationToken());
                    return r;
                }
                catch (Exception e)
                {
                    if (when != null)
                    {
                        var shouldRetry = when.Invoke(e);
                        if (!shouldRetry) throw;
                    }
                    if (i >= attempts) throw;

                    await Task.Delay(ExponentialBackoffTimeSlot(i) * 1000);
                }
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Please see <see href="https://en.wikipedia.org/wiki/Exponential_backoff">Exponetial backoff </see> time slot. 
        /// </summary>
        /// <param name="time">the time of trial</param>
        /// <returns>Time slot to wait.</returns>
        private static int ExponentialBackoffTimeSlot(int time)
        {
            var max = (int)Math.Pow(2, time);
            return random.Next(0, max);
        }
    }
}
