
namespace InterpreterProject.Expressions
{
    public interface IExprVisitor
    {
        object Visit(Binary binary);
        object Visit(Literal literal);
        object Visit(Grouping grouping);
        object Visit(Unary unary);
        object Visit(Assignment assignment);
        object Visit(Reassignment assignment);
        object Visit(Statement statement);
        object Visit(ArrowApplication arrowApplication);
        object Visit(ArrayExpression arrayExpression);
        object Visit(Indexer arrayExpression);
    }
}
