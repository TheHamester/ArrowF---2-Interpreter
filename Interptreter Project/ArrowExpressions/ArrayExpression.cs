using System.Collections.Generic;

namespace InterpreterProject.ArrowExpressions
{
    public record ArrayExpression(List<IExprTree> Elements) : IExprTree 
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);

    }
}
