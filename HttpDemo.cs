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
        //private String GetAndValidateUrl()
        //{
        //    Console.Write("Введіть URL-адресу: ");
        //    String url = Console.ReadLine()!;
        //    int index = url.IndexOf("://");
        //    if (index == -1)
        //    {
        //        throw new FormatException("Введений URL повинен містити схему запита");
        //    }
        //    String scheme = url[..index];
        //    if (scheme != "http" && scheme != "https")
        //    {
        //        throw new FormatException($"Схему запиту '{scheme}' не пітдримується: тільки http або https.");
        //    }
        //    Console.WriteLine($"Схема запиту: '{scheme}'");
        //    return "https://itstep.org/";
        //}
        private String GetAndValidateUrl()
        {
            Console.Write("Введіть URL-адресу: ");
            String url = Console.ReadLine()!;

            var urlComponents = ValidateAndParseUrl(url);

            DisplayUrlComponents(urlComponents);

            return urlComponents["full_url"];
        }
        private Dictionary<string, string> ValidateAndParseUrl(string url)
        {
            var components = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new FormatException("URL не може бути порожнім");
            }

            int schemeIndex = url.IndexOf("://");
            if (schemeIndex == -1)
            {
                throw new FormatException("URL повинен містити схему запиту (http:// або https://)");
            }

            string scheme = url[..schemeIndex].ToLower();
            if (scheme != "http" && scheme != "https")
            {
                throw new FormatException($"Схему запиту '{scheme}' не підтримується: тільки http або https.");
            }

            components["scheme"] = scheme;

            string remaining = url[(schemeIndex + 3)..];

            int hostEndIndex = remaining.IndexOfAny(new char[] { '/', '?', '#' });
            string hostPort;

            if (hostEndIndex == -1)
            {
                hostPort = remaining;
                remaining = "";
            }
            else
            {
                hostPort = remaining[..hostEndIndex];
                remaining = remaining[hostEndIndex..];
            }

            string host;
            string port = "";

            int portIndex = hostPort.IndexOf(':');
            if (portIndex != -1)
            {
                host = hostPort[..portIndex];
                port = hostPort[(portIndex + 1)..];

                if (!int.TryParse(port, out int portNumber) || portNumber < 1 || portNumber > 65535)
                {
                    throw new FormatException($"Невірний номер порту '{port}'. Порт має бути числом від 1 до 65535.");
                }
            }
            else
            {
                host = hostPort;
                port = scheme == "https" ? "443" : "80";
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                throw new FormatException("Хост не може бути порожнім");
            }

            components["host"] = host;
            components["port"] = port;

            string path = "";
            string query = "";
            string fragment = "";

            if (!string.IsNullOrEmpty(remaining))
            {
                int fragmentIndex = remaining.IndexOf('#');
                if (fragmentIndex != -1)
                {
                    fragment = remaining[(fragmentIndex + 1)..];
                    remaining = remaining[..fragmentIndex];
                }

                int queryIndex = remaining.IndexOf('?');
                if (queryIndex != -1)
                {
                    query = remaining[(queryIndex + 1)..];
                    path = remaining[..queryIndex];
                }
                else
                {
                    path = remaining;
                }
            }
            
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            components["path"] = path;
            components["query"] = query;
            components["fragment"] = fragment;

            string fullUrl = $"{scheme}://{host}";
            if ((scheme == "http" && port != "80") || (scheme == "https" && port != "443"))
            {
                fullUrl += $":{port}";
            }
            fullUrl += path;
            if (!string.IsNullOrEmpty(query))
            {
                fullUrl += $"?{query}";
            }
            if (!string.IsNullOrEmpty(fragment))
            {
                fullUrl += $"#{fragment}";
            }

            components["full_url"] = fullUrl;

            return components;
        }
        private void DisplayUrlComponents(Dictionary<string, string> components)
        {
            Console.WriteLine("РОЗБІР URL-АДРЕСИ:");

            Console.WriteLine($"{"Схема:",-15} {components["scheme"]}");
            Console.WriteLine($"{"Хост:",-15} {components["host"]}");
            Console.WriteLine($"{"Порт:",-15} {components["port"]}");
            Console.WriteLine($"{"Шлях:",-15} {components["path"]}");

            if (!string.IsNullOrEmpty(components["query"]))
            {
                Console.WriteLine($"{"Параметри:",-15} {components["query"]}");
                ParseQueryString(components["query"]);
            }
            else
            {
                Console.WriteLine($"{"Параметри:",-15} (відсутні)");
            }

            if (!string.IsNullOrEmpty(components["fragment"]))
            {
                Console.WriteLine($"{"Фрагмент:",-15} {components["fragment"]}");
            }
            else
            {
                Console.WriteLine($"{"Фрагмент:",-15} (відсутній)");
            }

            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"{"Повний URL:",-15} {components["full_url"]}");
            Console.WriteLine(new string('=', 50) + "\n");
        }
        private void ParseQueryString(string query)
        {
            var parameters = query.Split('&');
            if (parameters.Length > 0 && !(parameters.Length == 1 && string.IsNullOrEmpty(parameters[0])))
            {
                Console.WriteLine($"{"",-15} Параметри запиту:");
                foreach (var param in parameters)
                {
                    var keyValue = param.Split('=');
                    if (keyValue.Length == 2)
                    {
                        Console.WriteLine($"{"",-17} • {keyValue[0]} = {keyValue[1]}");
                    }
                    else if (keyValue.Length == 1)
                    {
                        Console.WriteLine($"{"",-17} • {keyValue[0]} = (без значення)");
                    }
                }
            }
        }
    }
}
