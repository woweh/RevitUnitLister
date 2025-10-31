using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace RevitUnitLister
{
    /// <summary>
    /// Service class responsible for exporting RevitUnitsData to various file formats
    /// </summary>
    public class RevitUnitsExporter
    {
        /// <summary>
        /// Exports the data to a JSON file
        /// </summary>
        /// <param name="data">The data to export</param>
        /// <param name="filePath">The full path where the JSON file should be saved</param>
        public void ExportToJson(RevitUnitsData data, string filePath)
        {
            using (var streamWriter = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    var serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, data);
                }
            }
        }

        /// <summary>
        /// Exports the data to a CSV file
        /// </summary>
        /// <param name="data">The data to export</param>
        /// <param name="filePath">The full path where the CSV file should be saved</param>
        public void ExportToCsv(RevitUnitsData data, string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write header
                writer.WriteLine("Quantity,Discipline,Quantity TypeId,Type Catalog String,Unit Name,Unit TypeId,Unit Symbol,Conversion Factor From Internal,Conversion Factor To Internal,Is Valid");

                // Write data rows
                foreach (var quantity in data.Quantities)
                {
                    foreach (var unit in quantity.Units)
                    {
                        writer.WriteLine(
            $"\"{quantity.DisplayName}\",\"{quantity.DisciplineName}\",\"{quantity.TypeId}\",\"{quantity.TypeCatalogString}\",\"{unit.DisplayName}\",\"{unit.TypeId}\",\"{unit.UnitSymbol}\",{unit.ConversionFromInternal},{unit.ConversionToInternal},{unit.IsValidUnit}");
                    }
                }
            }
        }
    }
}
