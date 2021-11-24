
namespace InterpreterProject.Expressions
{
    public class Reassignment : IExprTree
    {
        public string Id { get; set; }

        public IExprTree Right { get; set; }

        public TokenType Operation { get; set; }

        public object Accept(IExprVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public Reassignment(string id, IExprTree right, TokenType operation)
        {
            Id = id;
            Right = right;
            Operation = operation;
        }
    }
}
