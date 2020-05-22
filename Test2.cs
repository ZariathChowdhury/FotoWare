using NUnit.Framework;
using RestSharp;
using Microsoft.Extensions.Configuration;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FotoWare
{
    public class Test2
    {
        int number = 0;
        List<string> searchUrls = new List<string>();


        // PageProperties.json file contains Page Element Selectors
        IConfigurationRoot PageProperties = new ConfigurationBuilder()
                .AddJsonFile("PageProperties.json")
                .Build();

        // TestConfig.json contains Test Configuration Data
        IConfigurationRoot TestConfig = new ConfigurationBuilder()
                .AddJsonFile("TestConfig.json")
                .Build();

        // TestConfig.json contains Test Configuration Data
        IConfigurationRoot response = new ConfigurationBuilder()
                .AddJsonFile("response.json")
                .Build();

        [Test]
        public string GetAuthToken()
        {
            var client = new RestClient(TestConfig["auth_url"]);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type","application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", TestConfig["API_id"]);
            request.AddParameter("client_secret", TestConfig["API_secret"]);

            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            JObject joResponse = JObject.Parse(response.Content);
            string token = joResponse.GetValue("token").ToString();
            Console.WriteLine(token);
            return token;

        }

        public class Data
        {
            public string searchURL { get; set; }
            public int assetCount { get; set; }
        }


        [Test]
        public void Search()
        {
            var client = new RestClient(TestConfig["search_url"]);
            var request = new RestRequest(Method.GET);

            string token = GetAuthToken();

            request.AddHeader("token", token);
            request.AddParameter(
                "q="+TestConfig["keyword"],
                ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            JArray dataArray = JArray.Parse(response.Content);

            IList<Data> dataObjects = dataArray.Select(p => new Data
            {
                searchURL = (string)p["searchURL"],
                assetCount = (int)p["assetCount"]
            }).ToList();

            foreach (var item in dataObjects)
            {
                if (item.assetCount == 1)
                {
                    number = +1;
                    searchUrls.Add(item.searchURL);
                }
            }
            Console.WriteLine("Number of Archive with assetCount 1: " + number);

            
        }

        [Test]
        public void GlobalSearch()
        {
            var request = new RestRequest(Method.GET);

            for (var i = 0; i < searchUrls.Count; i++)
            {
                var client = new RestClient(searchUrls[i]);
                request.AddHeader("Accept", "application/vnd.twitter-v1+json");
                IRestResponse response = client.Execute(request);
                JObject resp = JObject.Parse(response.Content);
                var data = JObject.Parse(resp.ToString());
                var result = data[0];
                Console.WriteLine("First object in rendered array: "+ data[0]);
            }

        }

    }
}
