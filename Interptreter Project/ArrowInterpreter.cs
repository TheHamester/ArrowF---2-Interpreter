using System;
using System.Collections.Generic;
using System.Linq;
using InterpreterProject.Expressions;

namespace InterpreterProject
{
    public class ArrowInterpreter : IExprVisitor
    {
        private List<ArrowType> variables = new List<ArrowType>() {
            new ArrowNote("None",      (ushort)0,       true),
            new ArrowNote("Left",      (ushort)1,       true),
            new ArrowNote("Down",      (ushort)(1<<1),  true),
            new ArrowNote("Up",        (ushort)(1<<2),  true),
            new ArrowNote("Right",     (ushort)(1<<3),  true),
            new ArrowNote("LeftDown",  (ushort)(1<<4),  true),
            new ArrowNote("LeftUp",    (ushort)(1<<5),  true),
            new ArrowNote("DownRight", (ushort)(1<<6),  true),
            new ArrowNote("UpRight",   (ushort)(1<<7),   true),
            new ArrowNote("Mine",      (ushort)(1<<8),  true),
            new ArrowNote("Fake",      (ushort)(1<<9),  true),
            new ArrowNote("Hold",      (ushort)(1<<10), true),
            new ArrowNote("Roll",      (ushort)(1<<11), true),
            new ArrowNote("Red",       (ushort)(1<<12), true),
            new ArrowNote("Blue",      (ushort)(1<<13), true),
            new ArrowNote("Green",     (ushort)(1<<14), true),
            new ArrowNote("Yellow",    (ushort)(1<<15), true)
        };

        public object Interpret(IExprTree tree)
        {
            foreach (IExprTree expr in ((ArrowApplication)tree).Statements)
            {
                if (expr is Assignment assignment)
                    Evaluate(assignment);
                else if (expr is Reassignment reassignment)
                    Evaluate(reassignment);
                else
                    Console.WriteLine(Evaluate(expr));
            }
            return null;
        }

        public object Evaluate(IExprTree expr)
        {
            return expr.Accept(this);
        }

        public object Visit(Literal literal)
        {
            switch (literal.Token.Type)
            {
                case TokenType.Note:
                    return ushort.Parse(literal.Token.Text);
                case TokenType.True:
                    return true;
                case TokenType.False:
                    return false;
                case TokenType.Identifier:
                    object value;
                    
                    try
                    { value = variables.First(v => v.Id == literal.Token.Text).Value; }
                    catch (InvalidOperationException)
                    { value = null; }

                    if (value != null)
                        return value;
                    else
                        throw new Exception("Variable referenced before assignment");
            }
            return null;
        }

        public object Visit(ArrayExpression arrayExpression)
        {
            List<object> elements = new List<object>();
            foreach (IExprTree expr in arrayExpression.Elements)
                elements.Add(Evaluate(expr));
            return elements;
        }

        public object Visit(Grouping grouping) => grouping.Expr.Accept(this);

        public object Visit(Unary unary)
        {
            object right = Evaluate(unary.Right);
            ArrowType type = new ArrowType();
            switch (unary.Token.Type)
            { 
                case TokenType.Invert:
                    if (right is ushort invert)
                        return (ushort)~invert;
                    if (right is string)
                        if (variables.Where(t => t.Id == (string)right).First() is ArrowNote note)
                            return note.Value;

                    throw new OperationTypeMismatchException(right.GetType().ToString(), "", unary.Token.Text, false);
                case TokenType.Not:
                    if (right is bool not)
                        return !not;
                    if (right is string)
                        if (variables.Where(t => t.Id == (string)right).First() is ArrowBoolean boolean)
                            return boolean.Value;
                    throw new OperationTypeMismatchException(right.GetType().ToString(), "", unary.Token.Text, false);
                default:
                    throw new Exception("wtf");
            }
        }

        public object Visit(Binary binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            object result;
            switch (binary.Token.Type)
            { 
                case TokenType.Plus:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l + r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.Minus:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l - r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.Times:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l * r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.Divide:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l / r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.Modulo:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l % r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.And:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l & r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, ArrowBoolean>(left, right, (l, r) => l && r);
                    if (result != null) return (bool)result;
                    break;
                case TokenType.Xor:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l ^ r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, ArrowBoolean>(left, right, (l, r) => l ^ r);
                    if (result != null) return (bool)result;
                    break;
                case TokenType.Or:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l | r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, ArrowBoolean>(left, right, (l, r) => l || r);
                    if (result != null) return (bool)result;
                    break;
                case TokenType.LeftShift:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l << r));
                    if (result != null)  return (ushort)result;
                    break;
                case TokenType.RightShift:
                    result = BinaryReturn<ushort, ArrowNote>(left, right, (l, r) => (ushort)(l >> r));
                    if (result != null) return (ushort)result;
                    break;
            }
            throw new OperationTypeMismatchException(left.GetType().ToString(), right.GetType().ToString(), binary.Token.Text, true);
        }

        public object BinaryReturn<T, U>(object left, object right, Func<T,T,T> f) where U : ArrowType
        {
            if (left is T plusL && right is T plusR)
                return f(plusL, plusR);
            if (left is string && right is string)
            {
                ArrowType t1 = variables.First(t => t.Id == (string)left);
                ArrowType t2 = variables.First(t => t.Id == (string)right);
                if (t1 is U && t2 is U)
                    return f((T)((U)t1).Value, (T)((U)t2).Value);
            }
            if (left is string && right is T)
            {
                ArrowType t1 = variables.First(t => t.Id == (string)left);
                if (t1 is U)
                    return f((T)((U)t1).Value, (T)right);
            }
            if (left is T && right is string)
            {
                ArrowType t1 = variables.First(t => t.Id == (string)right);
                if (t1 is U)
                    return f((T)left, (T)((U)t1).Value);
            }
            return null;
        }

        public object Visit(Assignment assignment)
        {
            ArrowType type = assignment.Type;
            variables.Add(type);

            object result = null;
            if (assignment.Right != null)
                result = Evaluate(assignment.Right);
            else
            {
                if (type is ArrowNote)
                    result = (ushort)0;
                else if (type is ArrowBoolean)
                    result = false;
                else if (type is ArrowArray)
                    result = new List<ArrowType>();
            }

            if (type is ArrowNote && result is ushort)
                variables[variables.Count() - 1].Value = (ushort)result;
            else if (type is ArrowBoolean && result is bool)
                variables[variables.Count() - 1].Value = (bool)result;
            else if (type is ArrowArray && result is List<object>)
            {
                variables[variables.Count() - 1].Value = (List<object>)result;
                ArrowType t = new ArrowType();
                int topLayer = GetDepthAndType((ArrowArray)variables[variables.Count() - 1], ref t);

                MatchesTypeAndDepth((List<object>)variables[variables.Count() - 1].Value, t, topLayer);
            }
            else
                throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {type.GetType()}");

            return null;
        }

        public object Visit(Reassignment reassignment)
        {
            object result = Evaluate(reassignment.Right);
            
            int varIndex = variables.IndexOf(variables.First(v => v.Id == reassignment.Id));
            if (varIndex == -1)
                throw new Exception("Variable referenced before assignment");
            if(variables[varIndex].IsConst)
                throw new Exception("Cannot reassign a value for a constant");
            switch (reassignment.Operation)
            {
                case TokenType.Assign:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)result;
                    else if (variables[varIndex] is ArrowBoolean && result is bool)
                        variables[varIndex].Value = (bool)result;
                    else if (variables[varIndex] is ArrowArray && result is List<object>)
                    {
                        
                        ArrowType t = new ArrowType();
                        int topLayer = GetDepthAndType((ArrowArray)variables[varIndex], ref t);

                        MatchesTypeAndDepth((List<object>)result, t, topLayer);

                        

                        variables[varIndex].Value = (List<object>)result;
                    }
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.OrEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value | (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.AndEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value & (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.XorEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value ^ (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.PlusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value + (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.MinusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value - (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.TimesEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value * (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
                case TokenType.DivideEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value / (ushort)result);
                    else
                        throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
                    break;
            }

            return null;
        }

        private void ReassignVariable<T,U>(Func<T, T, T> f, int varIndex, T result)
        {
            if (variables[varIndex] is U && result is T)
                variables[varIndex].Value = f((T)variables[varIndex].Value, result);
            else
                throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
        }

        public object Visit(Statement statement)
        {
            return null;
        }

        public object Visit(ArrowApplication arrowApplication)
        {
            return null;
        }

        public object Visit(Indexer indexer)
        {
            object index = Evaluate(indexer.Expr);

            if (index is ushort)
            {

                object indexable = Evaluate(indexer.Indexable);

                if (indexable is List<object>)
                    return ((List<object>)indexable)[(ushort)index];
                else if (indexable is ArrowArray)
                    return ((List<object>)((ArrowArray)indexable).Value)[(ushort)index];
                else
                    throw new Exception($"Unindexable type {indexable.GetType()}");
            }
            else
                throw new Exception($"cannot use {index.GetType()} as index");
        }

        private int GetDepthAndType(ArrowArray arrowArray, ref ArrowType type)
        {
            if (arrowArray.Type is ArrowArray)
                return 1 + GetDepthAndType((ArrowArray)arrowArray.Type, ref type);
            else
            {
                type = arrowArray.Type;
                return 1;
            }
        }

        private void MatchesTypeAndDepth(List<object> value, ArrowType type, int layer)
        {
            foreach (object o in value)
            {
                if (o is List<object> o1)
                    MatchesTypeAndDepth(o1, type, layer - 1);
                else if (o is bool && (!(type is ArrowBoolean) || layer != 1))
                    throw new Exception("doesnt match type or depth");
                else if (o is ushort && (!(type is ArrowNote) || layer != 1))
                    throw new Exception("doesnt match type or depth");
            }

        }

    }
}
