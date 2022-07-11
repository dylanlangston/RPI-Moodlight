using System.Net;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RPI_Moodlight
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

        public static bool Is500Error(this HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.InternalServerError
                || response.StatusCode == HttpStatusCode.NotImplemented
                || response.StatusCode == HttpStatusCode.BadGateway
                || response.StatusCode == HttpStatusCode.ServiceUnavailable
                || response.StatusCode == HttpStatusCode.GatewayTimeout
                || response.StatusCode == HttpStatusCode.HttpVersionNotSupported;
        }

        public static T HandleException<T>(this Func<T> action, T defaultResponse) => HandleException<T, Exception>(action, defaultResponse);
        public static T HandleException<T, E>(this Func<T> action, T defaultResponse) where E : Exception
        {
            try
            {
                return action();
            }
            catch (E)
            {
                return defaultResponse;
            }
        }

        public static void HandleException(this Action action) => HandleException<Exception>(action);
        public static void HandleException<E>(this Action action) where E : Exception
        {
            try
            {
                action();
            }
            catch (E) { }
        }


        static ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 2
        };


        static JsonSerializerOptions _jsonOptions;
        static JsonSerializerOptions jsonOptions {
            get {
                if (_jsonOptions == null)
                {
                    _jsonOptions = new JsonSerializerOptions();
                    _jsonOptions.Converters.Add(new TupleAsArrayFactory());
                }
                return _jsonOptions;
            }
        }
        public static string GetEmotion(this WeFeelClient.WeFeelClient weFeelClient, DateTime? time = null)
            => GetEmotionInternal(weFeelClient, time).RunSync();
        public static async Task<string> GetEmotionAsync(this WeFeelClient.WeFeelClient weFeelClient, DateTime? time = null)
             => await GetEmotionInternal(weFeelClient, time);
        static async Task<string> GetEmotionInternal(WeFeelClient.WeFeelClient weFeelClient, DateTime? time = null)
        {
            try
            {
                // Get continents
                var continents = await weFeelClient.Zones.ContinentsAsync();
                IO_Utilities.Logging(JsonSerializer.Serialize(continents));

                // Get timezones for each continent
                ConcurrentDictionary<string, List<WeFeelClient.DeserializationObjects.Zones>> continentsAndZones = new();
                await Parallel.ForEachAsync(continents, parallelOptions, async (continent, ct) =>
                {
                    continentsAndZones.TryAdd(continent.Path, await weFeelClient.Zones.ContinentsTimezonesAsync(continent.Path));
                });
                IO_Utilities.Logging(JsonSerializer.Serialize(continentsAndZones));

                // Get all emotions every five minutes for the last half a day on every continent in every timezone
                ConcurrentBag<WeFeelClient.DeserializationObjects.Timepoints> emotionTimepoints = new();
                var offset = new DateTimeOffset(time ?? DateTime.Now); //DateTimeOffset.Now;
                await Parallel.ForEachAsync(continentsAndZones.SelectMany(i => i.Value), parallelOptions, async (zone, ct) =>
                {
                    (await weFeelClient.Emotions.PrimaryTimepointsAsync(new SortedDictionary<string, string>
                    {
                    { "granularity", "minute" },
                    { "start", offset.AddHours(-1).ToUnixTimeMilliseconds().ToString() },
                    { "end", offset.ToUnixTimeMilliseconds().ToString() },
                    { "continent", zone.Path.Split('/').First() },
                    { "timezone", zone.Id }
                    })).ForEach(eT => emotionTimepoints.Add(eT));
                });

                // Calculate Averages across all emotion timepoints
                List<(DateTime time, List<(string emotion, double average)> averages)> averages = new();
                var allEmotions = emotionTimepoints.SelectMany(eT => eT.Counts).Select(c => c.Key).Distinct().ToList();
                var allTimes = emotionTimepoints.Select(eT => eT.Start).Distinct().ToList();
                allTimes.ForEach(time =>
                {
                    List<(string emotion, double average)> currentTimeAverage = new();
                    allEmotions.ForEach(emotion =>
                    {
                        currentTimeAverage.Add((emotion, emotionTimepoints.Where(eT => eT.Start == time).SelectMany(eT => eT.Counts).Where(c => c.Key == emotion).Average(c => c.Value)));
                    });
                    averages.Add((time, currentTimeAverage));
                });
                IO_Utilities.Logging(JsonSerializer.Serialize<List<(DateTime time, List<(string emotion, double average)> averages)>>(averages, jsonOptions));

                // Calculate differences between averages
                List<(DateTime time, List<(string emotion, double difference)> differences)> differences = new();
                for (int c = 0; c < averages.Count - 1; c++)
                {
                    var start = averages[c];
                    var end = averages[c + 1];
                    differences.Add((end.time, end.averages.Select(i => (i.emotion, (i.average - start.averages.FirstOrDefault(e => e.emotion == i.emotion).average))).ToList()));
                }
                IO_Utilities.Logging(JsonSerializer.Serialize<List<(DateTime time, List<(string emotion, double difference)> differences)>>(differences, jsonOptions));

                // Calculate the average differenes over the past half a day
                List<(string emotion, double difference)> averageDifferenes = new();
                allEmotions.ForEach(emotion =>
                {
                    averageDifferenes.Add((emotion, differences.SelectMany(d => d.differences).Where(d => d.emotion == emotion).Average(d => d.difference)));
                });
                IO_Utilities.Logging(JsonSerializer.Serialize<List<(string emotion, double difference)>>(averageDifferenes, jsonOptions));

                return averageDifferenes.SkipWhile(d => d.emotion == "*").OrderBy(d => d.difference).FirstOrDefault().emotion ?? "Unknown";
            } 
            catch
            {
                return "Unknown";
            }
        }
    }
}
