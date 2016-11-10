using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ChatbotCore
{
    public class NYTimes : INews
    {
        private string Url = "https://api.nytimes.com/svc/topstories/v2/technology.json";
        private string urlParameters = "?api_key=2d06abfd57704a438e619fd975066fb8";

        public string getNews()
        {
            //Code gotten from: http://stackoverflow.com/questions/9620278/how-do-i-make-calls-to-a-rest-api-using-c
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Url);
            string responseString = "empty";

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!

                responseString = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return responseString;
        }
    }
}
