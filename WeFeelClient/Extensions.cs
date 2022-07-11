namespace WeFeelClient
{
    /// <summary>
    /// Misc Extensions
    /// </summary>
    internal static class Extensions
    {
        private static TaskFactory _taskFactory => new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(this Task task)
            => _taskFactory.StartNew(() => task).Unwrap().GetAwaiter().GetResult();
        public static T RunSync<T>(this Task<T> task)
            => _taskFactory.StartNew(() => task).Unwrap().GetAwaiter().GetResult();

        public static Stream ResponseBody(this HttpResponseMessage response)
            => Task.Run(() => response).ResponseBodyAsync().RunSync();
        public static async Task<Stream> ResponseBodyAsync(this Task<HttpResponseMessage> response)
            => await (await response).Content.ReadAsStreamAsync();

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
            => collection == null || collection.Count == 0;
    }
}
