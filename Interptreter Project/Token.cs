
namespace InterpreterProject
{
    public enum TokenType
    {
        Note,
        Identifier,
        B,
        N,
        Break,
        Continue,
        If,
        While,
        Random,
        Return,
        True,
        False,
        Comment,
        Plus,
        PlusEquals,
        Minus,
        MinusEquals,
        Times,
        TimesEquals,
        Divide,
        DivideEquals,
        Modulo,
        ModuloEquals,
        Not,
        NotEquals,
        Invert,
        And,
        AndEquals,
        Or,
        OrEquals,
        Xor,
        XorEquals,
        LeftShift,
        RightShift,
        LessThan,
        LessOrEqualTo,
        GreaterThan,
        GreaterOrEqualTo,
        Equals,
        Assign,
        Tab,
        LeftParenthesy,
        RightParenthesy,
        LeftBracket,
        RightBracket,
        Coma,
        EOL,
        EOF
    };

    public class Token
    {
        public TokenType Type { get; set; }

        public string Text { get; set; }

        public int StartPos { get; set; }

        public Token(TokenType type, string text, int startPos)
        {
            Type = type;
            Text = text;
            StartPos = startPos;
        }

        public override string ToString()
        {
            return $"{Type.ToString()}: {Text} {StartPos}";
        }
    }
}
