namespace WeFeelClient
{
    /// <summary>
    /// Rest Client to allow for sending and receiving signed RestFul APIs
    /// </summary>
    abstract class RestClient : IDisposable
    {
        HttpClient client;
        public RestClient(DelegatingHandler? client = null)
        {
            this.client = client == null ? new HttpClient() : new HttpClient(client);
        }

        /// <summary>
        /// Replace the Base HTTPClient Send method with retry logic
        /// </summary>
        protected HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption completionOption = default, CancellationToken cancellationToken = default)
        {
            if (completionOption != default && cancellationToken != default)
                return RetryOnSocketError(() => client.Send(request, completionOption, cancellationToken));
            else if (completionOption != default && cancellationToken == default)
                return RetryOnSocketError(() => client.Send(request, completionOption));
            else if (completionOption == default && cancellationToken != default)
                return RetryOnSocketError(() => client.Send(request, completionOption));
            else
                return RetryOnSocketError(() => client.Send(request));
        }
        /// <summary>
        /// Replace the Base HTTPClient SendAsync method with retry logic
        /// </summary>
        protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption = default, CancellationToken cancellationToken = default)
        {
            if (completionOption != default && cancellationToken != default)
                return await RetryOnSocketErrorAsync(client.SendAsync(request, completionOption, cancellationToken));
            else if (completionOption != default && cancellationToken == default)
                return await RetryOnSocketErrorAsync(client.SendAsync(request, completionOption));
            else if (completionOption == default && cancellationToken != default)
                return await RetryOnSocketErrorAsync(client.SendAsync(request, completionOption));
            else
                return await RetryOnSocketErrorAsync(client.SendAsync(request));
        }

        // This is designed to retry the request on socket error. Sleeps a random amount of time between 30-60 seconds to ensure we don't overload the server.
        // By retrying on System.IO.IOException only we ensure we're not repeating the same request multiple times.
        static readonly Random random = new();
        static HttpResponseMessage RetryOnSocketError(Func<HttpResponseMessage> task, int loopCount = 0)
            => RetryOnSocketErrorAsync(Task.Run(task), loopCount).RunSync();
        static async Task<HttpResponseMessage> RetryOnSocketErrorAsync(Task<HttpResponseMessage> task, int loopCount = 0)
        {
            try
            {
                var response = await task;
                return response;
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(HttpRequestException)
                    && ex.InnerException.GetType() == typeof(IOException)
                    && loopCount < 2)
                {
                    Thread.Sleep(random.Next(30000, 60000));
                    await RetryOnSocketErrorAsync(task, loopCount + 1);
                }
                throw;
            }
        }

        /// <summary>
        /// IDisposable interface
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }
    }
}
