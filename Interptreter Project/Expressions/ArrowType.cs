using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterProject.Expressions
{
    public class ArrowType
    {
        public string Id { get; set; }

        public object Value { get; set; }

        public bool IsConst { get; set; }
    }
}
