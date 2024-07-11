using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using Models;

public class SoapServiceClient
{
    private readonly ISampleService _serviceClient;

    public SoapServiceClient()
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(new Uri($"http://{Environment.MachineName}:5050/Service.svc"));
        var channelFactory = new ChannelFactory<ISampleService>(binding, endpoint);
        _serviceClient = channelFactory.CreateChannel();
        var result = _serviceClient.Ping("hey");
        Console.WriteLine("Ping method result: {0}", result);
    }

    public string Ping(string input) => _serviceClient.Ping(input);

    public ComplexModelResponse PingComplexModel(ComplexModelInput input) => _serviceClient.PingComplexModel(input);

    public void VoidMethod(out string stringValue) => _serviceClient.VoidMethod(out stringValue);

    public Task<int> AsyncMethod() => _serviceClient.AsyncMethod();

    public void XmlMethod(XElement xmlElement) => _serviceClient.XmlMethod(xmlElement);
}
