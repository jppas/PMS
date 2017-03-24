using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcWinAuth.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> TestResources()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError) throw new Exception(disco.Error);

            var client2 = new TokenClient(
                disco.TokenEndpoint,
                "client",
                "secret");

            var idSvrResponse=client2.RequestClientCredentialsAsync("api1");

            var baseAddress = "http://localhost:3721";

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(idSvrResponse.Result.AccessToken);
            var response = await client.GetStringAsync("identity");

            //"\n\nService claims:".ConsoleGreen();
            // JArray.Parse(response)
            return Content(response);

            //var handler = new HttpClientHandler()
            //{
            //    AllowAutoRedirect = true,
            //    UseDefaultCredentials = true
            //};

            //using (var client = new HttpClient(handler))
            //{
            //    client.BaseAddress = new Uri("http://localhost:3721");
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

              
            //    var rawResult = client.GetAsync("identity").Result;

               
            //    var jsonResult = await rawResult.Content.ReadAsStringAsync();
            //    var apiResult = JsonConvert.DeserializeObject<Dictionary<string,string>>(jsonResult);

            //    return Content(apiResult.ToString());
                
            //}

            //return Content("Hello");
        }
    }
}
