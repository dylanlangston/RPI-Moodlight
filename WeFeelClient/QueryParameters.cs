using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.String;

namespace WeFeelClient
{
    /// <summary>
    /// Query parameters for the request. Includes code to convert between common types.
    /// </summary>
    public class QueryParameters : IEnumerable<KeyValuePair<string, string>>
    {
        KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[0];
        public QueryParameters(IEnumerable<KeyValuePair<string, string>> parameters)
            => this.parameters = parameters.ToArray();
        public QueryParameters(SortedDictionary<string, string> parameters)
            => this.parameters = parameters.ToArray();
        public QueryParameters(string parameters = null)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                this.parameters = new KeyValuePair<string, string>[0];
                return;
            }

            var items = parameters.Split('&');
            if (items.IsNullOrEmpty()) throw new ArgumentNullException("parameters");
            var kvPairs = items.Select(i => i.Split('='));
            if (!kvPairs.All(kv => kv.Length == 2)) throw new FormatException("Parameters not in correct format: " + parameters?.ToString());
            this.parameters = kvPairs.Select(i => new KeyValuePair<string, string>(i.First(), i.Last())).ToArray();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => new QueryParametersEnum(parameters);
        private IEnumerator GetEnumerator1() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator1();

        /// <summary>
        /// Class Impliments IEnumerator interface for QueryParameters Object
        /// </summary>
        public class QueryParametersEnum : IEnumerator<KeyValuePair<string, string>>
        {
            int i = -1;
            KeyValuePair<string, string>[] parameters;

            public QueryParametersEnum(KeyValuePair<string, string>[] Parameters)
            {
                parameters = Parameters;
            }
            public KeyValuePair<string, string> Current { get => parameters[i]; }
            private object Current1 { get => this.Current; }

            object IEnumerator.Current { get => Current1; }

            public bool MoveNext()
            {
                i = i + 1;
                return i < parameters.Length;
            }

            public void Reset() => throw new NotImplementedException();

            private bool disposedValue = false;
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    if (disposing)
                    {
                        // Dispose of managed resources.
                    }
                    parameters = null;
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Override QueryParameters ToString
        /// </summary>
        public override string ToString()
        {
            StringBuilder canonicalQueryParameters = new();
            foreach (var parameter in parameters.OrderBy(p => p.Key, StringComparer.Ordinal).ThenBy(p => p.Value, StringComparer.Ordinal))
                canonicalQueryParameters.AppendFormat($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value ?? String.Empty)}&");
            if (canonicalQueryParameters.Length > 0) canonicalQueryParameters.Remove(canonicalQueryParameters.Length - 1, 1);
            return canonicalQueryParameters.ToString();
        }

        /// <summary>
        /// Implicitly convert SortedDictionary to QueryParameters
        /// </summary>
        public static implicit operator QueryParameters(SortedDictionary<string, string> d) => new QueryParameters(d);
        /// <summary>
        /// Implicitly convert string to QueryParameters
        /// </summary>
        public static implicit operator QueryParameters(string s) => new QueryParameters(s);
        /// <summary>
        /// Explicitly convert QueryParameters to string
        /// </summary>
        public static explicit operator string(QueryParameters p) => p?.ToString() ?? string.Empty;
    }
}
