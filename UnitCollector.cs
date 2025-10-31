using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitUnitLister
{
    /// <summary>
    /// Service class responsible for collecting unit data from Revit
    /// </summary>
    public class UnitCollector
    {
        private readonly UnitSymbolLoader _symbolLoader = new UnitSymbolLoader();

        /// <summary>
        /// Collects all units from Revit grouped by quantity
        /// </summary>
        public RevitUnitsData CollectAllUnits(Autodesk.Revit.ApplicationServices.Application app)
        {
            var exportData = new RevitUnitsData
            {
                RevitVersion = app.VersionNumber,
                ExportDate = DateTime.Now
            };

            // Load unit symbols from Autodesk schema files
            _symbolLoader.LoadSymbols();

            // Get all spec type IDs (quantities/disciplines)
            var specTypeIds = UnitUtils.GetAllMeasurableSpecs();

            foreach (var specTypeId in specTypeIds)
            {
                ProcessQuantity(specTypeId, exportData);
            }

            // Calculate totals
            exportData.TotalQuantities = exportData.Quantities.Count;
            exportData.TotalUnits = exportData.Quantities.Sum(q => q.Units.Count);

            return exportData;
        }

        /// <summary>
        /// Process a single quantity (spec type)
        /// </summary>
        private void ProcessQuantity(ForgeTypeId specTypeId, RevitUnitsData dataData)
        {
            try
            {
                // Get discipline information
                var disciplineId = UnitUtils.GetDiscipline(specTypeId);

                var quantityData = new QuantityData
                {
                    TypeId = specTypeId.TypeId,
                    DisplayName = LabelUtils.GetLabelForSpec(specTypeId),
                    DisciplineTypeId = disciplineId?.TypeId ?? "Unknown",
                    DisciplineName = disciplineId != null ? LabelUtils.GetLabelForDiscipline(disciplineId) : "Unknown",
                    TypeCatalogString = UnitUtils.GetTypeCatalogStringForSpec(specTypeId) ?? string.Empty
                };

                // Get all units for this spec/quantity
                var unitTypeIds = UnitUtils.GetValidUnits(specTypeId);

                foreach (var unitTypeId in unitTypeIds)
                {
                    ProcessUnit(unitTypeId, quantityData, dataData);
                }

                if (quantityData.Units.Count > 0)
                {
                    AddQuantityToExport(quantityData, dataData);
                }
                else
                {
                    dataData.Warnings.Add($"Quantity '{quantityData.DisplayName}' has no valid units");
                }
            }
            catch (Exception ex)
            {
                dataData.FailedQuantities++;
                dataData.Errors.Add($"Failed to process quantity {specTypeId?.TypeId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error processing spec {specTypeId?.TypeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Process a single unit
        /// </summary>
        private void ProcessUnit(ForgeTypeId unitTypeId, QuantityData quantityData, RevitUnitsData dataData)
        {
            try
            {
                var unitData = new UnitData
                {
                    TypeId = unitTypeId.TypeId,
                    DisplayName = LabelUtils.GetLabelForUnit(unitTypeId),
                    IsValidUnit = UnitUtils.IsUnit(unitTypeId),
                    // Get unit symbol from loader
                    UnitSymbol = _symbolLoader.GetSymbol(unitTypeId.TypeId)
                };

                unitData.ConversionFromInternal =
                    GetConversionFromInternal(unitTypeId, unitData.DisplayName, dataData);

                unitData.ConversionToInternal =
                    GetConversionToInternal(unitTypeId, unitData.DisplayName, dataData);

                // HashSet automatically prevents duplicates
                if (!quantityData.Units.Add(unitData))
                {
                    dataData.Warnings.Add($"Duplicate unit skipped: {unitData.DisplayName} ({unitData.TypeId})");
                }
            }
            catch (Exception ex)
            {
                dataData.FailedUnits++;
                dataData.Errors.Add($"Failed to process unit {unitTypeId?.TypeId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error processing unit {unitTypeId?.TypeId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Add quantity to export data with duplicate checking
        /// </summary>
        private static void AddQuantityToExport(QuantityData quantityData, RevitUnitsData dataData)
        {
            if (!dataData.Quantities.Add(quantityData))
            {
                dataData.DuplicateQuantitiesSkipped++;
                dataData.Warnings.Add(
                    $"Duplicate quantity skipped: {quantityData.DisplayName} ({quantityData.TypeId})");
                System.Diagnostics.Debug.WriteLine(
                    $"DUPLICATE FOUND: {quantityData.DisplayName} | TypeId: {quantityData.TypeId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Added quantity: {quantityData.DisplayName} | TypeId: {quantityData.TypeId}");
            }
        }

        /// <summary>
        /// Get conversion factor for a unit
        /// </summary>
        private static double GetConversionFromInternal(ForgeTypeId unitTypeId, string displayName,
            RevitUnitsData dataData)
        {
            try
            {
                return UnitUtils.ConvertFromInternalUnits(1.0, unitTypeId);
            }
            catch (Exception ex)
            {
                dataData.Warnings.Add($"Failed to get conversion factor for unit '{displayName}': {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Get conversion factor to internal units for a unit
        /// </summary>
        private static double GetConversionToInternal(ForgeTypeId unitTypeId, string displayName,
            RevitUnitsData dataData)
        {
            try
            {
                return UnitUtils.ConvertToInternalUnits(1.0, unitTypeId);
            }
            catch (Exception ex)
            {
                dataData.Warnings.Add($"Failed to get conversion factor to internal units for unit '{displayName}': {ex.Message}");
                return 0;
            }
        }
    }
}