using System;

namespace InterpreterProject.ArrowExceptions
{
    public class OperationTypeMismatchException : ArrowException
    {
        public string Type1 { get; }
        public string Type2 { get; }
        public string Operation { get; }
        public bool Binary { get; }

        public OperationTypeMismatchException(string type1, string type2, string operation, bool binary, int line) 
            : base(line, $"Cannot apply a {(binary ? "binary" : "unary")} operation {operation} to objects of {(binary ? $"types {type1} and {type2}" : $"type {type1}")}")
        {
            Type1 = type1;
            Type2 = type2;
            Operation = operation;
            Binary = binary;
        }
    }
}
