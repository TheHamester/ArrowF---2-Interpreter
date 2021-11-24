using System;

namespace InterpreterProject
{
    public class OperationTypeMismatchException : Exception
    {
        private string type1;
        private string type2;
        private string operation;
        private bool binary;

        public OperationTypeMismatchException(string type1, string type2, string operation, bool binary) 
            : base($"Cannot apply a {(binary ? "binary" : "unary")} operation {operation} to objects of {(binary ? $"types {type1} and {type2}" : $"type {type1}")}")
        {
            this.type1 = type1;
            this.type2 = type2;
            this.operation = operation;
            this.binary = binary;
        }
    }
}
