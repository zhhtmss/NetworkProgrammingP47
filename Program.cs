using System.Text;

namespace NetworkProgrammingP47
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            // new HttpDemo().RunAsync().Wait();
            new ApiDemo().Run();
            
            Console.WriteLine("Program finished");
        }
    }
}