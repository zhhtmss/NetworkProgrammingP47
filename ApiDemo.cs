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
        private List<(string Name, string Abbr, double Rate, double ReverseRate)> savedRates;
        //public void RunHW()
		//{
        //    Console.WriteLine("Курси валют НБУ");
        //    DemoJson();
        //}
        public void Run()
		{
			Console.WriteLine("Курси валют НБУ");
			DemoXmlOrm();
			Console.WriteLine($"Заватажено {exchange.Currencies.Count()} курсів");
            while (true) 
            {
                Console.WriteLine("\nОберіть опцію:");
                Console.WriteLine("1 - Пошук валюти за фрагментом назви");
                Console.WriteLine("2 - Завантажити курси на конкретну дату");
                Console.WriteLine("3 - Вихід");
                Console.Write("Ваш вибір: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        SearchCurrency();
                        break;
                    case "2":
                        LoadHistoricalRates();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
			//while (true) 
			//{
            //    Console.WriteLine("Введіть фрагмент назви валюти: ");
            //    String? fragment = Console.ReadLine();
			//	if (String.IsNullOrEmpty(fragment)) break;
            //    var query = exchange.Currencies.Where(c => c.ShortName.Contains(fragment, StringComparison.OrdinalIgnoreCase) ||
			//		c.FullName.Contains(fragment, StringComparison.OrdinalIgnoreCase));
			//	Console.WriteLine($"Знайдено {query.Count()} результатів:");
			//	foreach(var c in query)
			//	{
			//		Console.WriteLine(c);
            //    }
            //}
			
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
                savedRates = new List<(string, string, double, double)>();
                foreach (var rate in jsonElement.EnumerateArray())
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
                    
					savedRates.Add((name, abbr, course, reverseCourse));
                }
                ShowMenu();
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

        /*для дз*/
        private void SearchCurrency()
        {
            Console.Write("Введіть фрагмент назви валюти: ");
            string? fragment = Console.ReadLine();
            if (string.IsNullOrEmpty(fragment))
            {
                Console.WriteLine("Фрагмент не може бути порожнім.");
                return;
            }
            var query = exchange.Currencies.Where(c =>
                c.ShortName.Contains(fragment, StringComparison.OrdinalIgnoreCase) ||
                c.FullName.Contains(fragment, StringComparison.OrdinalIgnoreCase)).ToList();
            if (query.Count == 0)
            {
                Console.WriteLine("Немає результатів для заданого фрагмента.");
                return;
            }
            Console.WriteLine($"Знайдено {query.Count} результатів:");
            foreach (var c in query)
            {
                Console.WriteLine(c);
            }
        }
        private void LoadHistoricalRates()
        {
            Console.Write("Введіть дату (у форматі YYYY-MM-DD): ");
            string? dateInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dateInput))
            {
                Console.WriteLine("Дата не може бути порожньою.");
                return;
            }

            if (!DateTime.TryParseExact(dateInput.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                Console.WriteLine("Невірний формат дати. Введіть у форматі YYYY-MM-DD. Спробуйте ще раз.");
                return;
            }

            DateOnly latestAvailable = GetLatestAvailableDate();
            DateOnly requested = DateOnly.FromDateTime(date.Date);
            if (requested > latestAvailable)
            {
                Console.WriteLine($"Курси для {requested:yyyy-MM-dd} ще не опубліковані. Остання доступна дата: {latestAvailable:yyyy-MM-dd}.");
                return;
            }

            string url = $"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date={date:yyyyMMdd}";
            try
            {
                using HttpClient httpClient = new();
                Stream bodyStream = httpClient.GetStreamAsync(url).Result;
                XmlSerializer serializer = new XmlSerializer(typeof(Exchange));
                Exchange historicalExchange = (Exchange)serializer.Deserialize(bodyStream)!;
                if (historicalExchange == null || historicalExchange.Currencies == null || historicalExchange.Currencies.Count == 0)
                {
                    Console.WriteLine($"Немає даних для запитаної дати {requested:yyyy-MM-dd}.");
                    return;
                }

                exchange = historicalExchange;

                Console.WriteLine($"Курси валют на {requested:yyyy-MM-dd}:");
                foreach (var currency in historicalExchange.Currencies)
                {
                    Console.WriteLine(currency);
                }

                SearchCurrency();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при завантаженні даних: {ex.Message}");
            }
        }

        // Повертає останню дату, для якої НБУ вже опублікував курси.
        // Якщо сьогодні вихідний — це останній робочий день (п'ятниця).
        // Якщо будній день, але час менше 16:00 — останній робочий день перед сьогоднішнім.
        private DateOnly GetLatestAvailableDate()
        {
            DateTime now = DateTime.Now;
            DateTime candidate;
            if (now.DayOfWeek == DayOfWeek.Saturday)
            {
                candidate = now.AddDays(-1);
            }
            else if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                candidate = now.AddDays(-2); 
            }
            else
            {
                if (now.TimeOfDay < TimeSpan.FromHours(16))
                {
                    candidate = now.AddDays(-1);
                }
                else
                {
                    candidate = now;
                }

                if (candidate.DayOfWeek == DayOfWeek.Saturday)
                    candidate = candidate.AddDays(-1);
                else if (candidate.DayOfWeek == DayOfWeek.Sunday)
                    candidate = candidate.AddDays(-2);
            }

            return DateOnly.FromDateTime(candidate.Date);
        }
        private void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== МЕНЮ ===");
                Console.WriteLine("1: Вивести за збільшенням курсу");
                Console.WriteLine("2: Вивести за зменшенням курсу");
                Console.WriteLine("0: Вихід");
                Console.Write("Ваш вибір: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayByRateAscending();
                        break;
                    case "2":
                        DisplayByRateDescending();
                        break;
                    case "0":
                        Console.WriteLine("До побачення!");
                        return;
                    default:
                        Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }

        private void DisplayByRateAscending()
        {
            if (savedRates == null || savedRates.Count == 0)
            {
                Console.WriteLine("Немає збережених курсів валют");
                return;
            }

            var sortedRates = savedRates.OrderBy(r => r.Rate).ToList();

            Console.WriteLine("\n=== КУРСИ ВАЛЮТ (за збільшенням курсу) ===\n");
            Console.WriteLine($"{"Валюта"} {"Код"} {"Курс (UAH)"} {"Обернений курс"}");
            Console.WriteLine("-------------------------");

            foreach (var rate in sortedRates)
            {
                Console.WriteLine($"{rate.Name}: 1 {rate.Abbr} = {rate.Rate:F2} UAH, 1 UAH = {rate.ReverseRate:F4} {rate.Abbr}");
            }

            Console.WriteLine($"\nВсього: {sortedRates.Count} валют");
        }

        private void DisplayByRateDescending()
        {
            if (savedRates == null || savedRates.Count == 0)
            {
                Console.WriteLine("Немає збережених курсів валют");
                return;
            }

            var sortedRates = savedRates.OrderByDescending(r => r.Rate).ToList();

            Console.WriteLine("\n=== КУРСИ ВАЛЮТ (за зменшенням курсу) ===\n");
            Console.WriteLine($"{"Валюта"} {"Код"} {"Курс (UAH)"} {"Обернений курс"}");
            Console.WriteLine("-------------------------");

            foreach (var rate in sortedRates)
            {
                Console.WriteLine($"{rate.Name}: 1 {rate.Abbr} = {rate.Rate:F2} UAH, 1 UAH = {rate.ReverseRate:F4} {rate.Abbr}");
            }

            Console.WriteLine($"\nВсього: {sortedRates.Count} валют");
        }
    }
}