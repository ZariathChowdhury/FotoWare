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
        string token;
        List<string> searchUrls = new List<string>();


        // PageProperties.json file contains Page Element Selectors
        IConfigurationRoot PageProperties = new ConfigurationBuilder()
                .AddJsonFile("PageProperties.json")
                .Build();

        // TestConfig.json contains Test Configuration Data
        IConfigurationRoot TestConfig = new ConfigurationBuilder()
                .AddJsonFile("TestConfig.json")
                .Build();


        [Test]
        public void GetAuthToken()
        {
            var client = new RestClient(TestConfig["auth_url"]);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type","application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/json");
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", TestConfig["API_id"]);
            request.AddParameter("client_secret", TestConfig["API_secret"]);

            IRestResponse response = client.Execute(request);
            TestContext.Progress.WriteLine("Starting TEST 2... ");
            TestContext.Progress.WriteLine("Get Auth Response :" + response.Content);

            JObject joResponse = JObject.Parse(response.Content);
            token = joResponse.GetValue("access_token").ToString();
            TestContext.Progress.WriteLine("Token is: " + token);

        }

        public class Data
        {
            public string searchURL { get; set; }
            public int assetCount { get; set; }
        }


        [Test]
        public void Search()
        {
            var client = new RestClient(
                TestConfig["search_url"]+TestConfig["keyword"]
                );
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type",
                "application/vnd.fotoware.collectionlist+json");
           
            request.AddHeader("Cookie", "FotoWebVersion=bdf8e2bb-c60e-4c12-b441-2156f0171049");
            request.AddHeader("Cookie", "mp_508f8100a814a68a076eaa38916099f4_mixpanel=%7B%7D");
            request.AddHeader("Cookie", "SessionToken="+token);
            request.AddHeader("Content-Type", "application/vnd.fotoware.collectionlist+json");
           
            IRestResponse searchResponse = client.Execute(request);
            TestContext.Progress.WriteLine("Search query response : " + searchResponse.Content);

            JArray dataArray = JArray.Parse(searchResponse.Content);

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
            TestContext.Progress.WriteLine("Number of Archive with assetCount 1: " + number);

            
        }

        [Test]
        public void GlobalSearch()
        {
            var request = new RestRequest(Method.GET);

            for (var i = 0; i < searchUrls.Count; i++)
            {
                var client = new RestClient(searchUrls[i]);
                request.AddHeader("Accept", "pplication/vnd.fotoware.assetlist+json");
                IRestResponse response = client.Execute(request);
                JObject resp = JObject.Parse(response.Content);
                var data = JObject.Parse(resp.ToString());
                var result = data[0];
                TestContext.Progress.WriteLine("First object in rendered array: "+ data[0]);
            }

        }

    }
}
