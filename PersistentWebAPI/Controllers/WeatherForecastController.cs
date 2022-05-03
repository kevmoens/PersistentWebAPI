using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Runtime.Loader;

namespace PersistentWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private SimpleUnloadable _unloadable;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, SimpleUnloadable unloadable)
        {
            _logger = logger;
            _unloadable = unloadable;
        }

        [HttpPost()]
        public IEnumerable<WeatherForecast> PostForecasts()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = GetNextTemp(),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        private int GetNextTemp()
        {
            var assembly = _unloadable.Context.GetAssembly("RoslynCompileSample");
            Type? twriter = assembly.GetType("RoslynCompileSample.LocalTemp");
            MethodInfo? method = twriter.GetMethod("NextTemp");
            var writer = Activator.CreateInstance(twriter);
            var output = method.Invoke(writer, new object[] { });
            writer = null;
            return (int)output;
        }
        [HttpPost("Clear")]
        public void Clear()
        {
            _unloadable.Context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _unloadable.Context = new SimpleUnloadableAssemblyLoadContext();
        }

    }
}