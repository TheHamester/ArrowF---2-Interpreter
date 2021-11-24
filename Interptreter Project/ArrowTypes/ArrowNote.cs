
namespace InterpreterProject.ArrowTypes
{
    public class ArrowNote : ArrowType
    {
        public ArrowNote(string id, ushort value, bool isConst)
        {
            Id = id;
            Value = value;
            IsConst = isConst;
        }
    }
}
