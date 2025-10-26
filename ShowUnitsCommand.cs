using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace RevitUnitLister
{
    /// <summary>
    /// External command that displays the Unit Lister WPF window
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowUnitsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Collect all unit data from Revit
                var collector = new UnitCollector();
                var exportData = collector.CollectAllUnits(commandData.Application.Application);

                // Show WPF window with the data
                var window = new UnitListWindow(exportData);
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message + "\n" + ex.StackTrace;
                TaskDialog.Show("Error", "Failed to load units:\n" + ex.Message);
                return Result.Failed;
            }
        }
    }
}