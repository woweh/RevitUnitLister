using System;

namespace RevitUnitLister
{
    /// <summary>
    /// Represents detailed information about a unit in Revit
    /// </summary>
    public class UnitData : IEquatable<UnitData>
    {
        public string TypeId { get; set; }
        public string DisplayName { get; set; }
        public double ConversionFromInternal { get; set; }
        public double ConversionToInternal { get; set; }
        public string UnitSymbol { get; set; }
        public bool IsValidUnit { get; set; }

        // Equality based on TypeId (unique identifier)
        public bool Equals(UnitData other)
        {
            return other != null && string.Equals(TypeId, other.TypeId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnitData);
        }

        public override int GetHashCode()
        {
            return TypeId?.GetHashCode() ?? 0;
        }
    }
}