using System.Collections.Generic;

namespace MirrorMapper
{
    internal interface IMirrorObject
    {
        void SetProperties(object target, Dictionary<string, object> values);
    }
}