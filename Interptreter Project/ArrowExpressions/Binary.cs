
namespace InterpreterProject.ArrowExpressions
{
    public record Binary(IExprTree Left, Token Token, IExprTree Right) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
