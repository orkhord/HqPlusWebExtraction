using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HqPlusWebExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            var extractor = new BookingDataExtractor();

            var currentDirectory = Directory.GetCurrentDirectory();
            var inputDataDirectory = Path.Combine(Directory.GetParent(currentDirectory).Parent.Parent.FullName, "input-data");
            var filePath = Path.Combine(inputDataDirectory, "Kempinski Hotel Bristol Berlin, Germany - Booking.com.html");

            if (args.Any())
            {
                filePath = args[0];
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found :(");
                return;
            }

            var result = extractor.ExtractFromFile(filePath).Result;

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };
            var json = JsonSerializer.Serialize(result, options);

            Console.WriteLine(json);
        }
    }
}
