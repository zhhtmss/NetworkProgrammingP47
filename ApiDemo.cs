using System;
using System.Text.Json;
using static System.Net.WebRequestMethods;
namespace NetworkProgrammingP47
{
	internal class ApiDemo
	{
		public void Run()
		{
			Console.WriteLine("Курси валют НБУ");
			DemoJson();
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
		private void DemoXml()
		{
			String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";
			using HttpClient httpClient = new();
			String body = httpClient.GetStringAsync(url).Result;
			Console.WriteLine(body);
        }

		
    }
}
