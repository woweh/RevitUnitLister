using System;
using System.Collections.Generic;

namespace RevitUnitLister
{
    /// <summary>
    /// Data structure to hold Revit units information
    /// </summary>
    public class RevitUnitsData
    {
        public string RevitVersion { get; set; }
        public DateTime ExportDate { get; set; }
        public int TotalQuantities { get; set; }
        public int TotalUnits { get; set; }
        public HashSet<QuantityData> Quantities { get; set; } = new HashSet<QuantityData>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public int DuplicateQuantitiesSkipped { get; set; }
        public int FailedUnits { get; set; }
        public int FailedQuantities { get; set; }
    }
}