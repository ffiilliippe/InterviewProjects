using RestSharp;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

namespace TheMonarchs
{
    public class MonarchsModel
    {
        public int id { get; set; }
        public string nm { get; set; }
        public string cty { get; set; }
        public string hse { get; set; }
        public string yrs { get; set; }
    }

    public class MonarchsProcessedModel
    {
        public int id { get; set; }
        public string nm { get; set; }
        public string hse { get; set; }
        public int yrsQtd { get; set; }
        public string yrFrom { get; set; }
        public string yrTo { get; set; }
    }
    internal class Program
    {
        static void Main(string[] args)
        {

            var client = new RestClient();

            var request = new RestRequest("https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings");

            var response = client.Get(request).Content;

            var monarchsResponse = JsonSerializer.Deserialize<List<MonarchsModel>>(response.Trim());

            //I created a new list with 3 more fields to facilitate future processing.
            var monarchsProcessed = GetMonarchsProcessed(monarchsResponse); 

            ShowResults(monarchsProcessed);
            
           Console.Read();
        }

        private static void ShowResults(List<MonarchsProcessedModel>? monarchsProcessed)
        {
            
            Console.WriteLine($"1.How many monarchs are in the dataset? " +
                $"\nR: In the dataset there are {monarchsProcessed.Count()} monarchs");

            var longestMonarch = monarchsProcessed.MaxBy(c => c.yrsQtd);
            Console.WriteLine($"\n2.Which monarch ruled the longest and for how many years ? " +
                $"\nR: The Monarch who ruled the longest was {longestMonarch.nm} " +
                $"and She ruled for {longestMonarch.yrsQtd} years.");

            var longestHouse = GetTheLongestHouse(monarchsProcessed);
            Console.WriteLine($"\n3.Which house ruled the longest and for how many years ? " +
                $"\nR: The longest house ruled was {longestHouse.FirstOrDefault().hse} and it ruled for {longestHouse.Sum(s => s.yrsQtd)} years");

            var commonFirstName = GetCommonFirstName(monarchsProcessed);
            Console.WriteLine($"\n4.What is the most common first name in the dataset ? " +
                $"\nR: The most common first name is {commonFirstName}.");


            var yearsCurrentHouse = GetYearsCurrentHouse(monarchsProcessed);
            Console.WriteLine($"\n5.What is the house of the current monarch and for how many years did that house rule throughout history ? " +
                $"\nR: The house of the current monarch is {monarchsProcessed.Last().hse} and that house roled for {yearsCurrentHouse} years throughout history.");

        }
        private static List<MonarchsProcessedModel> GetTheLongestHouse(List<MonarchsProcessedModel> monarchs)
        {
            //I grouped and ordered by quantity
            return monarchs.GroupBy(it => it.hse)
                                        .OrderByDescending(grp => grp.Count())
                                        .First().ToList();
        }
        private static int GetYearsCurrentHouse(List<MonarchsProcessedModel> monarchsProcessed)
        {
            //I filtered by current house records and then added the number of years.
            return monarchsProcessed.Where(w => w.hse == monarchsProcessed.Last().hse).ToList().Sum(s => s.yrsQtd);
        }
        private static string GetCommonFirstName(List<MonarchsProcessedModel> monarchsProcessed)
        {
            //First I created a temporary list with just the first name and then grouped it by quantity.
            var monarchTemp = new List<MonarchsProcessedModel>();

            foreach(var monarch in monarchsProcessed)
            {
                var monarchs = new MonarchsProcessedModel();
                monarchs.nm = monarch.nm.Split(' ')[0];

                monarchTemp.Add(monarchs);
            }

            var ret = monarchTemp.GroupBy(it => it.nm)
                                       .OrderByDescending(grp => grp.Count())
                                       .First();

            return ret.Key;
        }
        private static List<MonarchsProcessedModel> GetMonarchsProcessed(List<MonarchsModel>? monarchsResponse)
        {
            //Filling in the new list with the new fields.
            var modelList = new List<MonarchsProcessedModel>();
            foreach (var monarch in monarchsResponse)
            {
                var modelTemp = new MonarchsProcessedModel();
                modelTemp.id = monarch.id;
                modelTemp.nm = monarch.nm;
                modelTemp.hse = monarch.hse;
                modelTemp.yrFrom = monarch.yrs.Split('-')[0];
                try
                {
                    modelTemp.yrTo = monarch.yrs.Split('-')[1];
                }
                catch
                {
                    modelTemp.yrTo = monarch.yrs.Split('-')[0];
                }

                modelTemp.yrsQtd = Convert.ToInt16(string.IsNullOrEmpty(modelTemp.yrTo) ?
                    DateTime.Now.Year.ToString() : modelTemp.yrTo) - Convert.ToInt16(modelTemp.yrFrom);

                modelTemp.yrsQtd = modelTemp.yrsQtd == 0 ? 1: modelTemp.yrsQtd;

                modelList.Add(modelTemp);
            }

            return modelList;
        }
    }
}
