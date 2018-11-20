using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MirrorMapper
{
    /// <summary>
    /// This is an object relational mapper that will create a map from DataTable columns to object properties using Reflection. It is fully optimized and will not use Reflection while filling the object list.
    /// </summary>
    public static class Mirror
    {
        private static Dictionary<Guid, IMirrorObject> _mirrorObjects;

        static Mirror()
        {
            _mirrorObjects = new Dictionary<Guid, IMirrorObject>();
        }

        /// <summary>
        /// Use Reflection to fill a List of <typeref name="T" /> with identical properties.
        /// </summary>
        public static List<T> DynamicMap<T>(DataTable dataTable) where T : new()
        {
            List<T> result = new List<T>();
            IDataReader dataReader = dataTable.CreateDataReader();

            List<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
            Type destType = typeof(T);

            while (dataReader.Read())
            {
                object[] values = new object[columnNames.Count];
                dataReader.GetValues(values);

                T dto = new T();
                for (int i = 0; i < values.Length; i++)
                {
                    PropertyInfo prop = destType.GetProperty(columnNames[i]);
                    if (prop != null && !Convert.IsDBNull(values[i]))
                    {
                        prop.SetValue(dto, values[i], null);
                    }
                }
                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        /// Use a previously generated map to fill a List of <typeref name="T" /> with identical properties.
        /// </summary>
        public static List<T> Map<T>(DataTable dataTable) where T : new()
        {
            Guid typeID = typeof (T).GUID;
            MirrorObject<T> mirrorObject = (MirrorObject<T>) _mirrorObjects[typeID];
            IDataReader dataReader = dataTable.CreateDataReader();
            string[] columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
            T[] targets = new T[dataTable.Rows.Count];
            object[][] values = new object[targets.Length][];

            int row = 0;
            while (dataReader.Read())
            {
                targets[row] = new T();
                values[row] = new object[columnNames.Length];
                dataReader.GetValues(values[row]);
                row++;
            }

            mirrorObject.SetProperties(targets, columnNames, values);

            return targets.ToList();
        }

        /// <summary>
        /// Generate a map that can be used to fill a List of <typeref name="T" /> with identical properties.
        /// </summary>
        public static void CreateMap<T>() where T : new()
        {
            _mirrorObjects.Add(typeof(T).GUID, new MirrorObject<T>());
        }
    }
}
