using System.Text.Json;
using WeFeelClient.DeserializationObjects;

namespace WeFeelClient
{
    public class WeFeelClient
    {
        internal Uri baseURL;
        internal WeFeelRestful client;

        // Endpoints
        public EmotionsClass Emotions;
        public ZonesClass Zones;
        public GendersClass Genders;
        public TweetsClass Tweets;

        public WeFeelClient(Uri baseURL, DelegatingHandler client = null)
        {
            this.baseURL = baseURL;
            this.client = new(client);

            this.Emotions = new(this);
            this.Zones = new(this);
            this.Genders = new(this);
            this.Tweets = new(this);
        }

        public abstract class WeFeelEndpoints
        {
            protected WeFeelClient weFeel;
            public WeFeelEndpoints(WeFeelClient client)
            {
                weFeel = client;
            }
        }

        public class EmotionsClass : WeFeelEndpoints
        {
            public EmotionsClass(WeFeelClient client) : base(client) { }

            public async Task<EmotionsTree> TreeAsync()
                => await JsonSerializer.DeserializeAsync<EmotionsTree>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/emotions/tree").ResponseBodyAsync());

            public async Task<List<EmotionsPrimary>> PrimaryAsync()
                => await JsonSerializer.DeserializeAsync<List<EmotionsPrimary>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/emotions/primary").ResponseBodyAsync());

            public async Task<List<EmotionsSecondary>> SecondaryAsync(string primaryEmotion)
                => await JsonSerializer.DeserializeAsync<List<EmotionsSecondary>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary").ResponseBodyAsync());

            public async Task<List<EmotionsSecondaryRaw>> SecondaryRawAsync(string primaryEmotion, string secondaryEmotion)
                => await JsonSerializer.DeserializeAsync<List<EmotionsSecondaryRaw>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary/{secondaryEmotion.ToLowerInvariant()}/raw").ResponseBodyAsync());

            public async Task<List<Timepoints>> PrimaryTimepointsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/emotions/primary/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<List<Timepoints>> SecondaryTimepointsAsync(string primaryEmotion, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<List<Timepoints>> SecondaryRawTimepointsAsync(string primaryEmotion, string secondaryEmotion, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary/{secondaryEmotion.ToLowerInvariant()}/raw/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> PrimaryTotalsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/emotions/primary/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> SecondaryTotalsAsync(string primaryEmotion, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> SecondaryRawTotalsAsync(string primaryEmotion, string secondaryEmotion, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/emotions/primary/{primaryEmotion.ToLowerInvariant()}/secondary/{secondaryEmotion.ToLowerInvariant()}/raw/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public EmotionsTree Tree() => TreeAsync().RunSync();

            public List<EmotionsPrimary> Primary() => PrimaryAsync().RunSync();

            public List<EmotionsSecondary> Secondary(string primaryEmotion) => SecondaryAsync(primaryEmotion).RunSync();

            public List<EmotionsSecondaryRaw> SecondaryRaw(string primaryEmotion, string secondaryEmotion) => SecondaryRawAsync(primaryEmotion, secondaryEmotion).RunSync();

            public List<Timepoints> PrimaryTimepoints(QueryParameters parameters = null) => PrimaryTimepointsAsync(parameters).RunSync();

            public List<Timepoints> SecondaryTimepoints(string primaryEmotion, QueryParameters parameters = null) => SecondaryTimepointsAsync(primaryEmotion, parameters).RunSync();

            public List<Timepoints> SecondaryRawTimepoints(string primaryEmotion, string secondaryEmotion, QueryParameters parameters = null) => SecondaryRawTimepointsAsync(primaryEmotion, secondaryEmotion, parameters).RunSync();

            public Dictionary<string, double> PrimaryTotals(QueryParameters parameters = null) => PrimaryTotalsAsync(parameters).RunSync();

            public Dictionary<string, double> SecondaryTotals(string primaryEmotion, QueryParameters parameters = null) => SecondaryTotalsAsync(primaryEmotion, parameters).RunSync();

            public Dictionary<string, double> SecondaryRawTotals(string primaryEmotion, string secondaryEmotion, QueryParameters parameters = null) => SecondaryRawTotalsAsync(primaryEmotion, secondaryEmotion, parameters).RunSync();
        }

        public class ZonesClass : WeFeelEndpoints
        {
            public ZonesClass(WeFeelClient client) : base(client) { }

            public async Task<ZonesTree> TreeAsync()
                => await JsonSerializer.DeserializeAsync<ZonesTree>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/zones/tree").ResponseBodyAsync());

            public async Task<List<Zones>> ContinentsAsync()
                => await JsonSerializer.DeserializeAsync<List<Zones>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/zones/continents").ResponseBodyAsync());

            public async Task<List<Zones>> ContinentsTimezonesAsync(string continent)
                => await JsonSerializer.DeserializeAsync<List<Zones>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/zones/continents/{continent}/timezones").ResponseBodyAsync());

            public async Task<List<Timepoints>> ContinentsTimepointsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/zones/continents/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<List<Timepoints>> ContinentsTimezonesTimepointsAsync(string continent, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/zones/continents/{continent}/timezones/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> ContinentsTotalsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/zones/continents/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> ContinentsTimezonesTotalsAsync(string continent, QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + $"api/zones/continents/{continent}/timezones/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public ZonesTree Tree() => TreeAsync().RunSync();

            public List<Zones> Continents() => ContinentsAsync().RunSync();

            public List<Zones> ContinentsTimezones(string continent) => ContinentsTimezonesAsync(continent).RunSync();

            public List<Timepoints> ContinentsTimepoints(QueryParameters parameters = null) => ContinentsTimepointsAsync(parameters).RunSync();

            public List<Timepoints> ContinentsTimezonesTimepoints(string continent, QueryParameters parameters = null) => ContinentsTimezonesTimepointsAsync(continent, parameters).RunSync();

            public Dictionary<string, double> ContinentsTotals(QueryParameters parameters = null) => ContinentsTotalsAsync(parameters).RunSync();

            public Dictionary<string, double> ContinentsTimezonesTotals(string continent, QueryParameters parameters = null) => ContinentsTimezonesTotalsAsync(continent, parameters).RunSync();
        }

        public class GendersClass : WeFeelEndpoints
        {
            public GendersClass(WeFeelClient client) : base(client) { }

            public async Task<List<Timepoints>> TimepointsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/genders/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> TotalsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/genders/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public List<Timepoints> Timepoints(QueryParameters parameters = null) => TimepointsAsync(parameters).RunSync();

            public Dictionary<string, double> Totals(QueryParameters parameters = null) => TotalsAsync(parameters).RunSync();
        }

        public class TweetsClass : WeFeelEndpoints
        {
            public TweetsClass(WeFeelClient client) : base(client) { }

            public async Task<List<Timepoints>> TimepointsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<List<Timepoints>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/tweets/timepoints?" + parameters?.ToString()).ResponseBodyAsync());

            public async Task<Dictionary<string, double>> TotalsAsync(QueryParameters parameters = null)
                => await JsonSerializer.DeserializeAsync<Dictionary<string, double>>(await weFeel.client.SendAsync(weFeel.baseURL.AbsoluteUri + "api/tweets/totals?" + parameters?.ToString()).ResponseBodyAsync());

            public List<Timepoints> Timepoints(QueryParameters parameters = null) => TimepointsAsync(parameters).RunSync();

            public Dictionary<string, double> Totals(QueryParameters parameters = null) => TotalsAsync(parameters).RunSync();
        }
    }
}