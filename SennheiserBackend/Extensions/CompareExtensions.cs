namespace SennheiserBackend.Extensions
{
    /// <summary>
    /// Extension methods for instance comparison.
    /// </summary>
    public static class CompareExtensions
    {
        /// <summary>
        /// Compare two instances of T by value. Supports cascaded structures.
        /// </summary>
        /// <typeparam name="T">Type of instances to compare.</typeparam>
        /// <param name="a">Instance a.</param>
        /// <param name="b">Instance b.</param>
        /// <returns>A list of found changes.</returns>
        public static List<ValueChange> DetailedCompare<T>(this T a, T b)
        {
            var changes = new List<ValueChange>();

            var propertyInfos = a?.GetType().GetProperties();

            foreach (var propertyInfo in propertyInfos!)
            {
                var valA = propertyInfo.GetValue(a);
                var valB = propertyInfo.GetValue(b);

                if (valA == null && valB == null)
                {
                    continue;
                }

                if (!IsSimple(propertyInfo.PropertyType))
                {
                    if (valA == valB)
                    {
                        continue;
                    }

                    var subchanges = valA.DetailedCompare(valB);
                    subchanges.ForEach(s => s.Name = propertyInfo.Name + "." + s.Name);

                    changes.AddRange(subchanges);
                    continue;
                }

                if ((valA != null && !valA.Equals(valB)) || valA == null)
                {
                    var change = new ValueChange
                    {
                        Name = propertyInfo.Name,
                        ValueA = valA,
                        ValueB = valB
                    };

                    changes.Add(change);
                }
            }

            return changes;
        }

        private static bool IsSimple(Type type)
        {
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }
}
