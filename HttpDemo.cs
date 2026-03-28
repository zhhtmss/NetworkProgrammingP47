using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkProgrammingP47
{
    
    internal class HttpDemo
    {
        private Stopwatch stopwatch = new();
        public async Task RunAsync()
        {
            Console.WriteLine("Http Demo");
            String url;
            try { url = GetAndValidateUrl(); }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            stopwatch.Start();
            HttpClient httpClient = new();
            String data = await httpClient.GetStringAsync(url);
            long ms = stopwatch.ElapsedMilliseconds;
            Console.WriteLine(data);
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"Elapsed {ms} Milliseconds for loading, {stopwatch.ElapsedMilliseconds} total");
        }
        private String GetAndValidateUrl()
        {
            Console.Write("Введіть URL-адресу: ");
            String url = Console.ReadLine()!;
            int index = url.IndexOf("://");
            if (index == -1)
            {
                throw new FormatException("Введений URL повинен містити схему запита");
            }
            String scheme = url[..index];
            if (scheme != "http" && scheme != "https")
            {
                throw new FormatException($"Схему запиту '{scheme}' не пітдримується: тільки http або https.");
            }
            Console.WriteLine($"Схема запиту: '{scheme}'");
            return "https://itstep.org/";
        }
    }
}
