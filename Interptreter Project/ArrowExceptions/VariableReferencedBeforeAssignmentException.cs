using System;

namespace InterpreterProject.ArrowExceptions
{
    public class VariableReferencedBeforeAssignmentException : ArrowException
    {
        public string Id { get; }
        public VariableReferencedBeforeAssignmentException(string id, int line) : base(line, $"Variable {id} was referenced before assignment") 
        {
            Id = id;
        }
    }
}
