using System;

namespace InterpreterProject.ArrowExceptions
{
    public class UnableToModifyAConstantVariableException : ArrowException
    {
        public string Id { get; }
        public UnableToModifyAConstantVariableException (string id, int line) : base(line, $"Unable to modify a constant value of {id}") 
        {
            Id = id;
        }
    }
}
