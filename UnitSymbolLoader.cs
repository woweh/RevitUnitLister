using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RevitUnitLister
{
    /// <summary>
    /// Loads unit symbols from Autodesk JSON schema files
    /// </summary>
    public class UnitSymbolLoader
    {
        private readonly Dictionary<string, string> _symbolCache = new Dictionary<string, string>();

        /// <summary>
        /// Gets a unit symbol from the cache
        /// </summary>
        /// <param name="unitTypeId">The unit type ID</param>
        /// <returns>The symbol text, or empty string if not found</returns>
        public string GetSymbol(string unitTypeId)
        {
            return _symbolCache.TryGetValue(unitTypeId, out string symbol)
                ? symbol
                : string.Empty;
        }

        /// <summary>
        /// Gets the number of symbols loaded in the cache
        /// </summary>
        public int SymbolCount => _symbolCache.Count;

        /// <summary>
        /// Loads unit symbols from Autodesk Revit schema files
        /// </summary>
        public void LoadSymbols()
        {
            string commonFiles = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            string unitBasePath = Path.Combine(commonFiles, "Autodesk Shared", "Revit Schemas 2024", "Schemas", "unit");

            if (!Directory.Exists(unitBasePath))
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Autodesk Revit Schemas 2024 directory not found at: {unitBasePath}");
                System.Diagnostics.Debug.WriteLine("No symbols will be available");
                return;
            }

            try
            {
                string symbolPath = Path.Combine(unitBasePath, "symbol");
                if (Directory.Exists(symbolPath))
                {
                    LoadSymbolFiles(symbolPath);
                }

                System.Diagnostics.Debug.WriteLine(
                    $"Loaded {_symbolCache.Count} unit symbols from Autodesk Revit Schemas 2024");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load unit symbols from schemas: {ex.Message}");
            }
        }

        /// <summary>
        /// Load symbol files from JSON directory
        /// </summary>
        private void LoadSymbolFiles(string symbolPath)
        {
            try
            {
                string[] jsonFiles = Directory.GetFiles(symbolPath, "*.json", SearchOption.TopDirectoryOnly);

                foreach (string jsonFile in jsonFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(jsonFile);
                        var jObject = JObject.Parse(json);

                        // Schema format: 
                        // {
                        //   "typeid": "autodesk.unit.symbol:atm-1.0.1",
                        //   "constants": [
                        // { "id": "unit", "typedValue": { "typeid": "autodesk.unit.unit:atmospheres-1.0.1" } },
                        //     { "id": "text", "value": "atm" }
                        //   ]
                        // }

                        if (!(jObject["constants"] is JArray constants))
                        {
                            continue;
                        }

                        string unitTypeId = null;
                        string symbolText = null;

                        foreach (var constant in constants)
                        {
                            string id = constant["id"]?.ToString();

                            switch (id)
                            {
                                case "unit":
                                    unitTypeId = constant["typedValue"]?["typeid"]?.ToString();
                                    break;
                                case "text":
                                    symbolText = constant["value"]?.ToString();
                                    break;
                            }
                        }

                        // Map unit TypeId to symbol text
                        if (!string.IsNullOrEmpty(unitTypeId) && !string.IsNullOrEmpty(symbolText))
                        {
                            _symbolCache[unitTypeId] = symbolText;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Failed to parse symbol file {Path.GetFileName(jsonFile)}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load symbol files: {ex.Message}");
            }
        }
    }
}