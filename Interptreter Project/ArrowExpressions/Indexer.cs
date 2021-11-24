
namespace InterpreterProject.ArrowExpressions
{
    public record Indexer(IExprTree Indexable, IExprTree Expr, int Line) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
