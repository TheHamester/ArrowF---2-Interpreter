
namespace InterpreterProject.ArrowExpressions
{
    public record Grouping(IExprTree Expr) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);

    }
}
