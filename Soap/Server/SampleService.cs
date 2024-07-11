using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
	public class SampleService : ISampleService
	{
		public string Ping(string s)
		{
			Console.WriteLine("Exec ping method");
			return s;
		}

		public ComplexModelResponse PingComplexModel(ComplexModelInput inputModel)
		{
			Console.WriteLine("Input data. IntProperty: {0}, StringProperty: {1}", inputModel.IntProperty, inputModel.StringProperty);

            // Immediately return response
            var response = new ComplexModelResponse
            {
                FloatProperty = float.MaxValue / 2,
                StringProperty = inputModel.StringProperty,
                ListProperty = inputModel.ListProperty,
                DateTimeOffsetProperty = inputModel.DateTimeOffsetProperty
            };

            // Start background task to send messages asynchronously
            Task.Run(() => SendMessagesAsync(inputModel));

            return response;
        }
        private async Task SendMessagesAsync(ComplexModelInput inputModel)
        {
            // Simulate sending messages asynchronously
            for (int i = 1; i <= 5; i++)
            {
                string message = $"Message {i} for {inputModel.StringProperty}";

                // Replace with your actual callback URL
                string callbackUrl = "https://localhost:7049/api/messages"; // Example API endpoint URL

                await SendMessageToCallbackAsync(message, callbackUrl);
                await Task.Delay(2000); // Simulate delay between messages (2 seconds)
            }
        }

        private async Task SendMessageToCallbackAsync(string message, string callbackUrl)
        {
            string jsonData = JsonConvert.SerializeObject(message);

            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    // Optional: Add headers if required
                    // client.DefaultRequestHeaders.Add("Authorization", "Bearer <your-token>");

                    // Make HTTP POST request to the callback URL
                    var response = await client.PostAsync(callbackUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Message '{message}' sent successfully to {callbackUrl}");
                    }
                    else
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Failed to send message '{message}' to {callbackUrl}. Status code: {response.StatusCode}");
                        Console.WriteLine($"Response: {responseBody}");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }

        public void VoidMethod(out string s)
		{
			s = "Value from server";
		}

		public Task<int> AsyncMethod()
		{
			return Task.Run(() => 42);
		}

		public int? NullableMethod(bool? arg)
		{
			return null;
		}

		public void XmlMethod(XElement xml)
		{
			Console.WriteLine(xml.ToString());
		}
    }
}
