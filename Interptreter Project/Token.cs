
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
        LeftShiftEquals,
        RightShift,
        RightShiftEquals,
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

    public record Token(TokenType Type, string Text, int Line) 
    {
        public override string ToString()
        {
            return $"{Type}: {Text} on line {Line}";
        }
    }
}
