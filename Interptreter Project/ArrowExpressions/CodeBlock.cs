using System.Collections.Generic;

namespace InterpreterProject.ArrowExpressions
{
    public record CodeBlock(List<IExprTree> Statements) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
