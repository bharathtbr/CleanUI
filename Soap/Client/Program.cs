using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client
{
	public class Program
	{
		public static async Task Main()
		{
            var callbackUrl = "https://localhost:7049/api/messages";
            var message = "Message 3 for ed46d755-9eb4-4099-a856-a468dfddbbcc";
            //var jsonMessage = $"{{\"message\":\"{message}\"}}";
            string jsonData = JsonConvert.SerializeObject(message);

            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    // Log the request content
                    Console.WriteLine("Request Content:");
                    Console.WriteLine(jsonData);

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



            var binding = new BasicHttpBinding();
				var endpoint = new EndpointAddress(new Uri(string.Format("http://{0}:5050/Service.svc", Environment.MachineName)));
				var channelFactory = new ChannelFactory<ISampleService>(binding, endpoint);
				var serviceClient = channelFactory.CreateChannel();
				var result = serviceClient.Ping("hey");
				Console.WriteLine("Ping method result: {0}", result);

				var complexModel = new ComplexModelInput
				{
					StringProperty = Guid.NewGuid().ToString(),
					IntProperty = int.MaxValue / 2,
					ListProperty = new List<string> { "test", "list", "of", "strings" },
					DateTimeOffsetProperty = new DateTimeOffset(2018, 12, 31, 13, 59, 59, TimeSpan.FromHours(1))
				};

				var complexResult = serviceClient.PingComplexModel(complexModel);
				Console.WriteLine("PingComplexModel result. FloatProperty: {0}, StringProperty: {1}, ListProperty: {2}, DateTimeOffsetProperty: {3}, EnumProperty: {4}",
					complexResult.FloatProperty, complexResult.StringProperty, string.Join(", ", complexResult.ListProperty), complexResult.DateTimeOffsetProperty, complexResult.TestEnum);

				serviceClient.VoidMethod(out var stringValue);
				Console.WriteLine("Void method result: {0}", stringValue);

				var asyncMethodResult = serviceClient.AsyncMethod().Result;
				Console.WriteLine("Async method result: {0}", asyncMethodResult);

				var xmlelement = System.Xml.Linq.XElement.Parse("<test>string</test>");
				serviceClient.XmlMethod(xmlelement);

				Console.ReadKey();
		}
	}
}
