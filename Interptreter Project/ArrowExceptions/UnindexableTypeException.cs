using System;

namespace InterpreterProject.ArrowExceptions
{
    internal class UnindexableTypeException : ArrowException
    {
        public readonly string Type;

        public UnindexableTypeException(string type, int line) : base(line, $"Unindexable type {type}") 
        {
            Type = type;
        }
    }
}
