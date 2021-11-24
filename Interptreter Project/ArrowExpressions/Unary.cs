
namespace InterpreterProject.ArrowExpressions
{
    public record Unary(Token Token, IExprTree Right) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
