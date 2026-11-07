using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{//start of namespace

    //the only way the website talks to the RaceDay API
    public class ApiClient
    {//start of class

        //the name of the cookie the API uses for its session
        private const string ApiSessionCookieName = ".AspNetCore.Session";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        public Task<ApiResponse> GetAsync(string path)
        {
            return SendAsync(HttpMethod.Get, path, null);
        }

        public Task<ApiResponse> PostAsync(string path, object? body = null)
        {
            return SendAsync(HttpMethod.Post, path, ToJson(body));
        }

        public Task<ApiResponse> PutAsync(string path, object body)
        {
            return SendAsync(HttpMethod.Put, path, ToJson(body));
        }

        public Task<ApiResponse> DeleteAsync(string path)
        {
            return SendAsync(HttpMethod.Delete, path, null);
        }

        //sends an image to the API, which stores it in Azure Blob Storage
        public Task<ApiResponse> UploadAsync(string path, IFormFile file)
        {//start of method

            MultipartFormDataContent form = new MultipartFormDataContent();
            StreamContent fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", file.FileName);

            return SendAsync(HttpMethod.Post, path, form);

        }//end of method

        private static HttpContent? ToJson(object? body)
        {
            if (body == null)
            {
                return null;
            }

            return new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        private async Task<ApiResponse> SendAsync(HttpMethod method, string path, HttpContent? content)
        {//start of method

            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, path))
                {
                    request.Content = content;

                    //send the visitor's API session cookie so the API knows who they are
                    ISession? session = _accessor.HttpContext?.Session;
                    string? cookie = session?.GetString(SessionKeys.ApiCookie);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        request.Headers.Add("Cookie", cookie);
                    }

                    using (HttpResponseMessage response = await _http.SendAsync(request))
                    {
                        //remember the session cookie the API gives us (for example after login)
                        if (session != null && response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? setCookies))
                        {
                            foreach (string setCookie in setCookies)
                            {
                                if (setCookie.StartsWith(ApiSessionCookieName, StringComparison.Ordinal))
                                {
                                    session.SetString(SessionKeys.ApiCookie, setCookie.Split(';')[0]);
                                }
                            }
                        }

                        string body = await response.Content.ReadAsStringAsync();
                        return new ApiResponse { StatusCode = (int)response.StatusCode, Body = body };
                    }
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                //the API is switched off or too slow
                return new ApiResponse { StatusCode = 0, ServiceUnavailable = true };
            }

        }//end of method

    }//end of class
}//end of namespace