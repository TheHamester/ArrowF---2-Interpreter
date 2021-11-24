
namespace InterpreterProject.ArrowExpressions
{
    public record Literal(Token Token) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }

}
