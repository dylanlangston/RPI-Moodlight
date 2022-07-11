using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static System.String;

namespace RPI_Moodlight
{
    /// <summary>
    /// Http handler that allows for logging requests and responses 
    /// </summary>
    class HttpClientWithLogging : DelegatingHandler
    {
        private const int MAX_RETRIES = 10;
        private Random random = new();

        Action<Exception> OnError = null;
        public bool EnableLogging = true;
        public HttpClientWithLogging(bool enableLogging = true, Action<Exception> onError = null)
        {
            InnerHandler = new HttpClientHandler();
            EnableLogging = enableLogging;
            OnError = onError;
        }

        async Task<HttpResponseMessage> ErrorHandler(Task<HttpResponseMessage> task)
        {
            var retries = 0;
            HttpResponseMessage response = default;
            try
            {
                do
                {
                    response = await task;

                    if (response.IsSuccessStatusCode)
                        return response;
                    else if (!response.Is500Error())
                        throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase} {await response.Content.ReadAsStringAsync()}", new HttpRequestException($"{ Join(Empty, response.RequestMessage.RequestUri?.Segments ?? Array.Empty<string>()) } { response.RequestMessage.Method.Method }"));

                    var waitTime = random.Next(1000, 10000);
                    Thread.Sleep(waitTime);
                } while (retries++ < MAX_RETRIES);

                var body = BeautifyJson(await response.Content.ReadAsStringAsync());

                throw new Exception($"{(int)response.StatusCode} {response.ReasonPhrase} {(IsNullOrEmpty(body) ? "" : "\nBody: " + body)}", new HttpRequestException($"{ Join(Empty, response.RequestMessage.RequestUri?.Segments ?? Array.Empty<string>()) } { response.RequestMessage.Method.Method }"));
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(ex);
                throw;
            }
            //return response;
        }
        HttpResponseMessage ErrorHandler(Func<HttpResponseMessage> task)
            => ErrorHandler(Task.Run(task)).RunSync();

        void LogRequest(HttpRequestMessage request)
        {
            string requestString = Empty;
            if (EnableLogging)
            {
                // Log Query Parameters if they exist
                if (!IsNullOrEmpty(request.RequestUri?.Query))
                    requestString += $"Begin /{Join(Empty, request.RequestUri.Segments.Skip(1))} {request.Method.Method} Parameters: {request.RequestUri.Query.TrimStart('?')}" + "\n";
                else
                    requestString += $"Begin /{Join(Empty, request.RequestUri?.Segments.Skip(1) ?? Array.Empty<string>())} {request.Method.Method}" + "\n";

                if (request.Headers.Any())
                    requestString += "Request Headers:\n  " + Join("\n  ", request.Headers.Select(h => $"{h.Key}: {Join(",", h.Value)}")) + "\n";

                if (request?.Content != null)
                {
                    var content = request.Content.ReadAsStringAsync().RunSync();
                    if (!IsNullOrEmpty(content))
                    {
                        var trimmed = content.Trim();
                        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                            requestString += "Request Body: " + BeautifyJson(content) + "\n";
                        else
                            requestString += "Request Body: " + content + "\n";
                    }
                }
                IO_Utilities.Logging(requestString);
            }
        }

        void LogResponse(HttpRequestMessage request, HttpResponseMessage response)
        {
            string responseString = Empty;
            if (EnableLogging)
            {
                responseString += $"Response {response.StatusCode}\n";
                if (response?.Content != null)
                {
                    var content = response.Content.ReadAsStringAsync().RunSync();
                    if (!IsNullOrEmpty(content))
                    {
                        var trimmed = content.Trim();
                        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                            responseString += $"Body: " + BeautifyJson(content) + "\n";
                        else
                            responseString += "Body: " + content + "\n";
                    }
                }
                responseString += $"End {Join(Empty, request.RequestUri?.Segments ?? Array.Empty<string>())} {request.Method.Method}\n";
                IO_Utilities.Logging(responseString);
            }
        }

        static readonly JsonWriterOptions jsonWriterOptions = new()
        {
            Indented = true,
            SkipValidation = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public static string BeautifyJson(string json)
        {
            Func<string> beatify = () =>
            {
                using JsonDocument document = JsonDocument.Parse(json);
                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, jsonWriterOptions);
                document.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            };
            return beatify.HandleException(json);
        }

        #region Overrides
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogRequest(request);
            var response = ErrorHandler(() => base.Send(request, cancellationToken));
            LogResponse(request, response);
            return response;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogRequest(request);
            var response = await ErrorHandler(base.SendAsync(request, cancellationToken));
            LogResponse(request, response);
            return response;
        }
        #endregion
    }
}
