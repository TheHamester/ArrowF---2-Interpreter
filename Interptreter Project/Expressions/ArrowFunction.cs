﻿using System.Collections.Generic;

namespace InterpreterProject.Expressions
{
    public class ArrowFunction : ArrowType
    {
        public List<ArrowType> Types { get; set; }

        public ArrowFunction(string id, List<ArrowType> types, IExprTree value, bool isConst)
        {
            Id = id;
            Types = types;
            Value = value;
            IsConst = isConst;
        }

    }
}
