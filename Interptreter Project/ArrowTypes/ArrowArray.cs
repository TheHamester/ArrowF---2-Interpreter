using System.Collections.Generic;

namespace InterpreterProject.ArrowTypes
{
    public class ArrowArray : ArrowType
    {
        public ArrowType Type { get; set; }

        public ArrowArray(string id, ArrowType type, List<object> value, bool isConst)
        {
            Type = type;
            Value = value;
            Id = id;
            IsConst = isConst;
        }
    }
}
