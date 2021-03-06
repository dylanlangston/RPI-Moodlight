using RPI_Moodlight;
using System.Collections.Concurrent;

bool shouldExit = false;

#if DEBUG
Console.CancelKeyPress += (s, e) =>
{
    shouldExit = true;
};
#endif

try
{
    IO_Utilities.Logging("RPI-Moodlight Init");

    using var httpClient = new HttpClientWithLogging(false, (e) => IO_Utilities.Logging(e.ToString()));
    var weFeelClient = new WeFeelClient.WeFeelClient(new("http://wefeel.csiro.au/main/"), httpClient);

    while (!shouldExit)
    {
        var emotion = await RetryWithExponentialBackOff.RetryAsync<string>(weFeelClient.GetEmotionAsync(), 10);
        IO_Utilities.Logging("Emotion: " + emotion);
        Thread.Sleep(TimeSpan.FromMinutes(10));
    }
    IO_Utilities.Logging("RPI-Moodlight Exit");
}
catch (Exception ex)
{
    IO_Utilities.Logging(ex.ToString());
}
finally
{
    IO_Utilities.FlushAllTextToFiles();
}