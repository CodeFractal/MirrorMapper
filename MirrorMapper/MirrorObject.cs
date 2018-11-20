using System.Collections.Generic;
using System.Reflection;

namespace MirrorMapper
{
    internal class MirrorObject<T> : IMirrorObject
    {
        private Dictionary<string, MirrorProperty<T>> _mirrorProperties;

        public MirrorObject()
        {
            _mirrorProperties = new Dictionary<string, MirrorProperty<T>>();

            PropertyInfo[] properties = typeof (T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                _mirrorProperties.Add(property.Name, new MirrorProperty<T>(property));
            }
        }

        public void SetProperties(object target, Dictionary<string, object> values)
        {
            foreach (KeyValuePair<string, object> datum in values)
            {
                _mirrorProperties[datum.Key].Set(target, datum.Value);
            }
        }

        public void SetProperties(T[] targets, string[] properties, object[][] values)
        {
            IMirrorProperty mirrorProperty;
            int c, r;
            for (c = 0; c < properties.Length; c++)
            {
                if(_mirrorProperties.ContainsKey(properties[c]))
                {
                    mirrorProperty = _mirrorProperties[properties[c]];
                    for (r = 0; r < targets.Length; r++)
                    {
                        mirrorProperty.Set(targets[r], values[r][c]);
                    }
                }
            }
        }
    }
}