using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace ProblemsDownloader
{
    class Problem
    {
        public string id;
        public string size;
        public string operators;
        public bool solved;
    }

    internal class Program
    {
        private static string url =
            "http://icfpc2013.cloudapp.net/myproblems?auth=0071PimxQKpGJdtDE76gsjAoaOagBVX3tdGOfCQHvpsH1H";

        private static void Main(string[] args)
        {
            var json = new JavaScriptSerializer();
            byte[] data = new WebClient().DownloadData(url);
            var problems = json.Deserialize<List<Problem>>(Encoding.ASCII.GetString(data));

            Console.ReadKey();
        }

    }
}