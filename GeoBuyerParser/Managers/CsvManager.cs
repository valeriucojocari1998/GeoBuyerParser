using CsvHelper;
using GeoBuyerPromotion.Models;
using System.Globalization;
using System.IO;
using System.Collections.Generic;

namespace GeoBuyerPromotion.Managers
{
    public class CsvManager
    {
        public string FolderName { get; }

        public CsvManager(string folderName)
        {
            FolderName = folderName;
            var path = Path.Combine(".", "CSV");
            Directory.CreateDirectory(Path.Combine(".", "CSV"));
            Directory.CreateDirectory(Path.Combine(".", "CSV", folderName));
        }

        public void AppendToCsv(string fileName, IEnumerable<object> records)
        {
            string folderPath = Path.Combine(".", "CSV", FolderName);
            string filePath = Path.Combine(folderPath, fileName);

            // Determine whether to write the header based on file existence.
            bool writeHeader = !File.Exists(filePath);

            // Use StreamWriter with append mode to add data to the CSV file.
            using (var writer = new StreamWriter(filePath, append: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (writeHeader)
                {
                    csv.WriteHeader(records.GetType().GetGenericArguments()[0]);
                    csv.NextRecord();
                }

                foreach (var record in records)
                {
                    csv.WriteRecord(record);
                    csv.NextRecord();
                }
            }
        }

        public void WriteListsToCsv(Spot spot, List<ExtendedCategory> categories, List<ExtendedProduct> products)
        {
            AppendToCsv("spot.csv", new List<Spot>() { spot });
            AppendToCsv("category.csv", categories);
            AppendToCsv("product.csv", products);
        }
    }
}
