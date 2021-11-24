
namespace InterpreterProject.ArrowTypes
{
    public class ArrowBoolean : ArrowType
    {
        public ArrowBoolean(string id, bool value, bool isConst)
        {
            Id = id;
            Value = value;
            IsConst = isConst;
        }
    }
}
