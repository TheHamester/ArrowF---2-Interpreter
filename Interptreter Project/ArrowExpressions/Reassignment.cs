
namespace InterpreterProject.ArrowExpressions
{
    public record Reassignment(string Id, IExprTree Right, TokenType Operation, int Line) : IExprTree
    {
        public object Accept(IExprVisitor visitor) => visitor.Visit(this);
    }
}
