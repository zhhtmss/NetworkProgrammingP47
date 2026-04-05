using NetworkProgrammingP47.Orm.Nbu;
using System;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;
namespace NetworkProgrammingP47
{
	internal class ApiDemo
	{
		private Exchange exchange;
        public void Run()
		{
			Console.WriteLine("Курси валют НБУ");
			DemoXmlOrm();
			Console.WriteLine($"Заватажено {exchange.Currencies.Count()} курсів");
			while (true) 
			{
                Console.WriteLine("Введіть фрагмент назви валюти: ");
                String? fragment = Console.ReadLine();
				if (String.IsNullOrEmpty(fragment)) break;
                var query = exchange.Currencies.Where(c => c.ShortName.Contains(fragment, StringComparison.OrdinalIgnoreCase) ||
					c.FullName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
				Console.WriteLine($"Знайдено {query.Count()} результатів:");
				foreach(var c in query)
				{
					Console.WriteLine(c);
                }
            }
			
        }

        private void DemoJsonOrm()
        {
            String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
            using HttpClient httpClient = new();
            String body = httpClient.GetStringAsync(url).Result;
            List<NbuRate> rates = JsonSerializer.Deserialize<List<NbuRate>>(body)!;
			foreach (NbuRate rate in rates)
			{
				Console.WriteLine(rate);
			}
		}
        private void DemoJson()
		{
			String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
            using HttpClient httpClient = new();
            String body = httpClient.GetStringAsync(url).Result;
			var jsonElement = JsonSerializer.Deserialize<JsonElement>(body);
			if (jsonElement.ValueKind == JsonValueKind.Array)
			{
				Console.WriteLine("Одержано {0} записів", jsonElement.GetArrayLength());
				foreach(var rate in jsonElement.EnumerateArray())
				{
					//Console.WriteLine(String.Join(", ",
					//	rate.EnumerateObject()
					//	.Select(p => $"{p.Name}: {p.Value}")
					//));
					String name = rate.GetProperty("txt").GetString()!;
					String abbr = rate.GetProperty("cc").GetString()!;
					double course = rate.GetProperty("rate").GetDouble();
					double reverseCourse = 1.0 / course;
                    Console.WriteLine($"{name}: 1 {abbr} = {course:F2} UAH, 1 UAH = {reverseCourse:F4} {abbr}");
                }
            }
			else
			{
				Console.WriteLine("Не очікувано! JSON має тип: {0}", jsonElement.ValueKind);

            }
			// Console.WriteLine(body);
        }
        private void DemoXmlOrm()
		{
            String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";
            using HttpClient httpClient = new();
            Stream bodyStream = httpClient.GetStreamAsync(url).Result;
			XmlSerializer serializer = new XmlSerializer(typeof(Exchange));
			exchange = (Exchange)serializer.Deserialize(bodyStream)!;
			//foreach (var currency in exchange.Currencies)
			//{
			//	Console.WriteLine(currency);
            //}

        }

        private void DemoXml()
		{
			String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";
			using HttpClient httpClient = new();
			String body = httpClient.GetStringAsync(url).Result;
			//Console.WriteLine(body);
			XDocument xmlDocument = XDocument.Parse(body);
            foreach (var currency in xmlDocument.Root!.Descendants("currency"))
            {
                String cc = currency.Element("cc")!.Value;
                String text = currency.Element("txt")!.Value;
                Double rate = Double.Parse(
                    currency.Element("rate")!.Value,
                    CultureInfo.InvariantCulture
                );

                Console.WriteLine($"{cc} {text} {rate:F2}");
            }
        }

		
    }
}
