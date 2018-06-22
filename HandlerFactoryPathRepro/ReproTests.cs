using Funq;
using NUnit.Framework;
using ServiceStack;

namespace HandlerFactoryPathRepro
{
    [TestFixture]
    public class ReproTests
    {
        [Test]
        public void Test1()
        {
            var apiUrl = "http://localhost:2000/";

            var appHost = new TestAppHost()
            .Init()
            .Start(apiUrl);

            JsonServiceClient serviceClient = new JsonServiceClient(apiUrl);

            Hello request = new Hello { Name = "ServiceStack" };
            HelloResponse response = serviceClient.Post(request);

            Assert.That(response.Reply.Contains(request.Name));

            appHost.Dispose();
        }

        [Test]
        public void Test2()
        {
            var apiUrl = "http://localhost:2000/api/";

            var appHost = new TestAppHostWithHandlerFactoryPath()
            .Init()
            .Start(apiUrl);

            JsonServiceClient serviceClient = new JsonServiceClient(apiUrl);

            Hello request = new Hello { Name = "ServiceStack" };
            HelloResponse response = serviceClient.Post(request);

            Assert.That(response.Reply.Contains(request.Name));

            appHost.Dispose();
        }
    }

    public class TestAppHost : AppSelfHostBase
    {
        public TestAppHost()
                 : base("Test Container",
                 typeof(HelloServices).Assembly)
        { }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig()
            {
                ApiVersion = "v1",
                WsdlServiceNamespace = "http://schemas.example.com/",
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), true)
            });
        }
    }

    public class TestAppHostWithHandlerFactoryPath : AppSelfHostBase
    {
        public TestAppHostWithHandlerFactoryPath()
                 : base("Test Container",
                 typeof(HelloServices).Assembly)
        { }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig()
            {
                ApiVersion = "v1",
                HandlerFactoryPath = "api", // comment this out and it works
                WsdlServiceNamespace = "http://schemas.example.com/",
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), true)
            });
        }
    }

    [Route("/hello/{Name}")]
    [Route("/hello")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse : IHasResponseStatus
    {
        public string Reply { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class HelloServices : Service
    {
        public object Any(Hello request) => new HelloResponse { Reply = $"Hello, {request.Name}" };
    }
}