using System.Text.Json;

namespace RaceDay.Web.Services
{//start of namespace

    //what came back from the API, with a friendly message for the user if something went wrong
    public class ApiResponse
    {//start of class

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public int StatusCode { get; set; }

        public string Body { get; set; } = string.Empty;

        //true when the API could not be reached at all
        public bool ServiceUnavailable { get; set; }

        public bool IsSuccess
        {
            get { return StatusCode >= 200 && StatusCode < 300; }
        }

        public bool IsUnauthorized
        {
            get { return StatusCode == 401; }
        }

        //turns the JSON body into a C# object
        public T? Data<T>()
        {//start of method

            try
            {
                return JsonSerializer.Deserialize<T>(Body, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }

        }//end of method

        //a message that is safe to show to the user
        public string Message
        {
            get
            {//start of method

                if (ServiceUnavailable)
                {
                    return "RaceDay is not available right now. Please try again in a moment.";
                }

                try
                {
                    using (JsonDocument document = JsonDocument.Parse(Body))
                    {
                        JsonElement root = document.RootElement;

                        //the API's own messages, for example "That email is already registered."
                        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out JsonElement message)
                            && message.ValueKind == JsonValueKind.String)
                        {
                            return message.GetString() ?? string.Empty;
                        }

                        //validation errors, for example a missing field
                        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("errors", out JsonElement errors)
                            && errors.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty field in errors.EnumerateObject())
                            {
                                foreach (JsonElement error in field.Value.EnumerateArray())
                                {
                                    return error.GetString() ?? "Please check the details you entered.";
                                }
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    //the body was not JSON, so use the default messages below
                }

                if (StatusCode == 401) return "Please log in to continue.";
                if (StatusCode == 403) return "You do not have permission to do that.";
                if (StatusCode == 404) return "We could not find what you were looking for.";
                if (StatusCode == 400) return "Please check the details you entered and try again.";

                return "Something went wrong. Please try again.";

            }//end of method
        }

    }//end of class
}//end of namespace