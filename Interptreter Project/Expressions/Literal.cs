
namespace InterpreterProject.Expressions
{
    public class Literal : IExprTree
    {
        public Token Token;

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Literal(Token token)
        {
            Token = token;
        }
    }
}
