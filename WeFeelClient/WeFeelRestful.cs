namespace WeFeelClient
{
    internal class WeFeelRestful : RestClient
    {
        public WeFeelRestful(DelegatingHandler? client = null) : base(client) { }

        /// <summary>
        /// Leaky Bucket to rate limit requests
        /// </summary>
        static LeakyBucket leakyBucket = new(new()
        {
            LeakRate = 5,
            LeakRateTimeSpan = TimeSpan.FromMilliseconds(100),
            MaxFill = 10
        });
        static T Queue<T>(Func<T> action) => QueueAsync(Task.Run(action)).RunSync();
        static async Task<T> QueueAsync<T>(Task<T> action)
        {
            await leakyBucket.GainAccess();
            return await action;
        }

        /// <summary>
        /// Replace the Base HTTPClient Send method
        /// </summary>
        public HttpResponseMessage Send(string uri, HttpCompletionOption completionOption = default, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return Queue(() => base.Send(request, completionOption, cancellationToken));
        }
        /// <summary>
        /// Replace the Base HTTPClient SendAsync method
        /// </summary>
        public async Task<HttpResponseMessage> SendAsync(string uri, HttpCompletionOption completionOption = default, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await QueueAsync(base.SendAsync(request, completionOption, cancellationToken));
        }
    }
}
