
namespace InterpreterProject.ArrowExpressions
{
    public interface IExprVisitor
    {
        object Visit(Binary binary);
        object Visit(Literal literal);
        object Visit(Grouping grouping);
        object Visit(Unary unary);
        object Visit(Assignment assignment);
        object Visit(Reassignment reassignment);
        object Visit(CodeBlock arrowApplication);
        object Visit(ArrayExpression arrayExpression);
        object Visit(Indexer indexer);
        object Visit(IfStatement ifStatement);
    }
}
