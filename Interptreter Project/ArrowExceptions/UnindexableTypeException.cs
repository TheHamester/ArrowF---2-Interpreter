using System;

namespace InterpreterProject.ArrowExceptions
{
    internal class UnindexableTypeException : ArrowException
    {
        public string Type { get; }

        public UnindexableTypeException(string type, int line) : base(line, $"Unindexable type {type}") 
        {
            Type = type;
        }
    }
}
