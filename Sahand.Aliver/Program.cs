using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;

namespace Sahand.Aliver
{
    class Program
    {
        static void Main(string[] args)
        {
            var aliveUrl = ConfigurationManager.AppSettings["AliveUrl"];
            var pollOnEverySec = Convert.ToInt32(ConfigurationManager.AppSettings["PollOnEverySec"]);

            while (true)
            {
                aliveUrl += ("?rnd=" + (new Random().Next(999999)));

                var callRes = CallUrl(aliveUrl);
                Console.Write($"{{ pollOnEverySec: {pollOnEverySec}, calledOn: \"{DateTime.Now}\", res: \"{callRes}\" }}\r\n");

                Thread.Sleep(pollOnEverySec * 1000);
            }
        }

        private static string CallUrl(string uri)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    var temp = reader.ReadToEnd();
                }

                return "Call is OK";
            }
            catch (Exception e)
            {
                RestartApp();

                return "ERROR: " + e;
            }
        }

        private static void RestartApp()
        {
            //Start process, friendly name is something like MyApp.exe (from current bin directory)
            System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);

            //Close the current process
            Environment.Exit(0);
        }
    }
}
