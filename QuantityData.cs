using System;
using System.Collections.Generic;

namespace RevitUnitLister
{
    /// <summary>
    /// Represents a quantity (discipline/spec) with its associated units
    /// </summary>
    public class QuantityData : IEquatable<QuantityData>
    {
        public string TypeId { get; set; }
        public string DisplayName { get; set; }
        public string DisciplineTypeId { get; set; }
        public string DisciplineName { get; set; }
        public string TypeCatalogString { get; set; }
        public HashSet<UnitData> Units { get; set; } = new HashSet<UnitData>();

        // Equality based on TypeId (unique identifier)
        public bool Equals(QuantityData other)
        {
            if (other == null)
            {
                return false;
            }

            bool isEqual = string.Equals(TypeId, other.TypeId, StringComparison.Ordinal);

            // Debug logging
            if (isEqual && DisplayName != other.DisplayName)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"EQUALITY MATCH with different DisplayName: '{DisplayName}' vs '{other.DisplayName}' | TypeId: {TypeId}");
            }

            return isEqual;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as QuantityData);
        }

        public override int GetHashCode()
        {
            return TypeId?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"{DisplayName} ({DisciplineName}) - {TypeId} - {Units.Count} units";
        }
    }
}