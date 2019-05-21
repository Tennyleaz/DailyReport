using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DailyReport
{
    public class MyHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private const string ContentType = "application/json";
        private const string DateFormat = "s";  // I don't know how this property works?

        public MyHttpClient(string serverUrl = "http://10.10.15.65:5001/api")
        {
            _serverUrl = serverUrl;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));
        }

        #region http get
        public async Task<T> QueryByIdAsync<T>(string objectName, int recordId)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            Uri query = new Uri(_serverUrl + "/" + objectName + "/" + recordId);
            var results = await HttpGetAsync<T>(query).ConfigureAwait(false);

            return results;
        }

        public async Task<List<T>> QueryAllAsync<T>(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");

            Uri query = new Uri(_serverUrl + "/" + objectName);
            var results = await HttpGetAsync<List<T>>(query).ConfigureAwait(false);

            return results;
        }

        public async Task<List<T>> QueryAsync<T>(string objectName, params string[] args)
        {
            if (string.IsNullOrEmpty(objectName)) throw new ArgumentNullException("objectName");
            if (args == null) throw new ArgumentNullException("objectName");

            string address = _serverUrl + "/" + objectName;
            foreach (string str in args)
            {
                address += "/" + str;
            }

            Uri query = new Uri(address);
            var results = await HttpGetAsync<List<T>>(query).ConfigureAwait(false);

            return results;
        }

        public async Task<T> HttpGetAsync<T>(Uri uri)
        {
            try
            {
                string response = await HttpGetAsync(uri);
                var jToken = JToken.Parse(response);
                if (jToken.Type == JTokenType.Array)
                {
                    var jArray = JArray.Parse(response);
                    return JsonConvert.DeserializeObject<T>(jArray.ToString());
                }
                // else
                try
                {
                    var jObject = JObject.Parse(response);
                    return JsonConvert.DeserializeObject<T>(jObject.ToString());
                }
                catch
                {
                    return JsonConvert.DeserializeObject<T>(response);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return default(T);
        }

        private async Task<string> HttpGetAsync(Uri uri)
        {
            var responseMessage = await _httpClient.GetAsync(uri).ConfigureAwait(false);
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            string response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            return null;
        }
        #endregion

        #region http post
        public async Task<T> HttpPostAsync<T>(object inputObject, string objectName)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                   Formatting.None,
                   new JsonSerializerSettings
                   {
                       NullValueHandling = NullValueHandling.Ignore,
                       ContractResolver = new CreateableContractResolver(),
                       DateFormatString = DateFormat
                   });
            try
            {
                Uri uri = new Uri(_serverUrl + "/" + objectName);
                var response = await HttpPostAsync(json, uri);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return default(T);
        }

        private async Task<string> HttpPostAsync(string payload, Uri uri)
        {
            var content = new StringContent(payload, Encoding.UTF8, ContentType);

            var responseMessage = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new Exception(responseMessage.StatusCode.ToString());
        }
        #endregion

        #region http put
        public async Task<SuccessResponse> HttpPutAsync<T>(object inputObject, string objectName, int id)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                   Formatting.None,
                   new JsonSerializerSettings
                   {
                       NullValueHandling = NullValueHandling.Ignore,
                       ContractResolver = new CreateableContractResolver(),
                       DateFormatString = DateFormat
                   });
            try
            {
                Uri uri = new Uri(_serverUrl + "/" + objectName + "/" + id);
                var response = await HttpPutAsync(json, uri);
                return string.IsNullOrEmpty(response) ?
                    new SuccessResponse { Id = "", Errors = "", Success = true } :
                    JsonConvert.DeserializeObject<SuccessResponse>(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new SuccessResponse
                {
                    Success = false,
                    Errors = e.Message
                };
            }
        }

        private async Task<string> HttpPutAsync(string payload, Uri uri)
        {
            var content = new StringContent(payload, Encoding.UTF8, ContentType);

            var responseMessage = await _httpClient.PutAsync(uri, content).ConfigureAwait(false);
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new Exception(responseMessage.StatusCode.ToString());
        }
        #endregion

        #region http patch
        public async Task<SuccessResponse> HttpPatchAsync(object inputObject, Uri uri)
        {
            var json = JsonConvert.SerializeObject(inputObject,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new UpdateableContractResolver(),
                    DateFormatString = DateFormat
                });
            try
            {
                var response = await HttpPatchAsync(json, uri);
                return string.IsNullOrEmpty(response) ?
                    new SuccessResponse { Id = "", Errors = "", Success = true } :
                    JsonConvert.DeserializeObject<SuccessResponse>(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new SuccessResponse
                {
                    Success = false,
                    Errors = e.Message
                };
            }
        }

        private async Task<string> HttpPatchAsync(string payload, Uri uri)
        {
            var content = new StringContent(payload, Encoding.UTF8, ContentType);

            var request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = new HttpMethod("PATCH"),
                Content = content
            };

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new Exception(responseMessage.StatusCode.ToString());
        }
        #endregion

        #region http delete
        public async Task<bool> HttpDeleteAsync(string objectName, int id)
        {
            try
            {
                Uri uri = new Uri(_serverUrl + "/" + objectName + "/" + id);
                await HttpDeleteAsync(uri);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private async Task<string> HttpDeleteAsync(Uri uri)
        {
            var responseMessage = await _httpClient.DeleteAsync(uri).ConfigureAwait(false);
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return string.Empty;
            }

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (responseMessage.IsSuccessStatusCode)
            {
                return response;
            }

            throw new Exception(responseMessage.StatusCode.ToString());
        }
        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class SuccessResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id;

        [JsonProperty(PropertyName = "success")]
        public bool Success;

        [JsonProperty(PropertyName = "errors")]
        public object Errors;
    }
}
