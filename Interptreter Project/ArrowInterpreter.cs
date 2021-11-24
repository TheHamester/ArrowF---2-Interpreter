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
            new ArrowNote("UpRight",   (ushort)(1<<7),  true),
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

        public object Evaluate(IExprTree expr) => expr.Accept(this);

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
                    if (right is List<object>)
                    {
                        ApplyUnaryOperationToArray<ushort>((List<object>)right, (o) => (ushort)(~o), unary.Token);
                        return right;
                    }
                    break;
                case TokenType.Not:
                    if (right is bool not)
                        return !not;
                    if (right is List<object>)
                    {
                        ApplyUnaryOperationToArray<bool>((List<object>)right, (o) => (!o), unary.Token);
                        return right;
                    }
                    break;
                default:
                    throw new Exception("wtf");
            }
            throw new OperationTypeMismatchException(right.GetType().ToString(), "", unary.Token.Text, false);
        }

        public object Visit(Binary binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);

            object result;

            ArrowType type = new ArrowType();
            int topLayer = 0;
            switch (binary.Token.Type)
            { 
                case TokenType.Plus:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l + r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, List<object>>(left, right, (l, r) => l.Concat(r).ToList());
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Minus:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l - r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, List<object>>(left, right, (l, r)
                           => { List<object> lCopy = new List<object>(l); List<object> rCopy = new List<object>(r); for (int i = rCopy.Count-1; i >= 0; i--) { if(lCopy.Remove(rCopy[i])) rCopy.Remove(rCopy[i]); } return lCopy.Count >= rCopy.Count ? lCopy.Concat(rCopy).ToList() : rCopy.Concat(lCopy).ToList(); });
                    if (result != null) return (List<object>)result;

                    break;
                case TokenType.Times:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l * r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, ushort>(left, right, (l, r) 
                        => { List<object> list = new List<object>(l); List<object> res = new List<object>(); for (int i = 0; i < r; i++) res = res.Concat(list).ToList(); return res; });
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Divide:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l / r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, ushort>(left, right, (l, r)
                        => { int n = l.Count / r; List<object> res = new List<object>();  for (int i = 0; i < n; i++) res.Add(l[i]); return res; });
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Modulo:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l % r));
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.And:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l & r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l && r);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l & r), (l, r) => l && r);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Xor:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l ^ r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l ^ r);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l ^ r), (l, r) => l ^ r);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Or:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l | r));
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l || r);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l | r), (l, r) => l || r);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.LeftShift:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l << r));
                    if (result != null)  return (ushort)result;

                    if (left is List<object> && right is ushort)
                    {
                        topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                        MatchesTypeAndDepth((List<object>)left, type, topLayer);

                        ((List<object>)left).RemoveRange(0, (ushort)right);

                        return left;
                    }

                    break;
                case TokenType.RightShift:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l >> r));
                    if (result != null) return (ushort)result;

                    if (left is List<object> && right is ushort)
                    {
                        topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                        MatchesTypeAndDepth((List<object>)left, type, topLayer);

                        ((List<object>)left).RemoveRange(((List<object>)left).Count - (ushort)right, (ushort)right);

                        return left;
                    }

                    break;
            }

            throw new OperationTypeMismatchException(left.GetType().ToString(), right.GetType().ToString(), binary.Token.Text, true);
        }

        private object BinaryReturn<T,U>(object left, object right, Func<T,U,T> f)
        {
            int topLayer = 0;
            ArrowType type = new ArrowType();
            if (left is T plusL1 && right is U plusR1)
            {
                if (left is List<object>)
                {
                    topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                    MatchesTypeAndDepth((List<object>)left, type, topLayer);
                    if(right is List<object>)
                        MatchesTypeAndDepth((List<object>)right, type, topLayer);
                }

                return f(plusL1, plusR1);
            }
            if (left is U plusL2 && right is T plusR2)
            {
                if (right is List<object>)
                {
                    topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                    MatchesTypeAndDepth((List<object>)left, type, topLayer);
                    if (left is List<object>)
                        MatchesTypeAndDepth((List<object>)left, type, topLayer);
                }
                return f(plusR2, plusL2);
            }
            return null;
        }

        private void ApplyUnaryOperationToArray<T>(List<object> list, Func<T,T> func, Token token)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] is List<object>)
                    ApplyUnaryOperationToArray((List<object>)list[i], func, token);
                else
                {
                    if (list[i] is T)
                        list[i] = func((T)list[i]);
                    else
                        throw new OperationTypeMismatchException(list[i].GetType().ToString(), "", token.Text, false);
                }
            }
        }

        private List<object> TryApplyArrayToArrayOperation(object left, object right, Func<ushort, ushort, ushort> f1, Func<bool, bool, bool> f2)
        {
            int topLayer = 0;
            ArrowType type = new ArrowType();
            if (left is List<object> && right is List<object>)
            {
                topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                MatchesTypeAndDepth((List<object>)left, type, topLayer);
                MatchesTypeAndDepth((List<object>)right, type, topLayer);
                if (type is ArrowNote)
                    ApplyArrayToArrayOperation((List<object>)left, (List<object>)right, f1);
                else if (type is ArrowBoolean)
                    ApplyArrayToArrayOperation((List<object>)left, (List<object>)right, f2);
                return (List<object>)left;
            }
            return null;
        }

        private void ApplyArrayToArrayOperation<T>(List<object> l1, List<object> l2, Func<T, T, T> func)
        {
            int i = 0;
            while (i < l1.Count && i < l2.Count)
            {
                if ((l1[i] is List<object> l1i) && (l2[i] is List<object> l2i))
                    ApplyArrayToArrayOperation(l1i, l2i, func);
                else
                {
                    if (l1[i] is T && l2[i] is T)
                        l1[i] = func((T)l1[i], (T)l2[i]);
                    else
                        throw new Exception("unmatched types");
                }
                i++;
            }

            if (i < l1.Count)
                SetElementsToZero(l1, i);

            if (i < l2.Count)
                AddZeros(l1,l2, i);
                
        }

        private void SetElementsToZero(List<object> list, int starts)
        {
            for (int i = starts; i < list.Count; i++)
            {
                if (list[i] is List<object>)
                    SetElementsToZero((List<object>)list[i], 0);
                else if (list[i] is ushort)
                    list[i] = (ushort)0;
                else if (list[i] is bool)
                    list[i] = false;
            }
        }

        private void AddZeros(List<object> l1, List<object> l2, int starts)
        {
            List<object> l = new List<object>();
            for (int i = starts; i < l2.Count; i++)
            {
                if (l2[i] is List<object>)
                {
                    l = new List<object>();
                    AddZeros(l, (List<object>)l2[i], 0);
                    l1.Add(l);
                }
                else if (l2[i] is ushort)
                    l1.Add((ushort)0);
                else if (l2[i] is bool)
                    l1.Add(false);
            }
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
                    break;
                case TokenType.OrEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value | (ushort)result);
                    break;
                case TokenType.AndEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value & (ushort)result);
                    break;
                case TokenType.XorEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value ^ (ushort)result);
                    break;
                case TokenType.PlusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value + (ushort)result);
                    break;
                case TokenType.MinusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value - (ushort)result);
                    break;
                case TokenType.TimesEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value * (ushort)result);
                    break;
                case TokenType.DivideEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value / (ushort)result);
                    break;

            }
            throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
        }

        private void ReassignVariable<T,U>(Func<T, T, T> f, int varIndex, T result)
        {
            if (variables[varIndex] is U && result is T)
                variables[varIndex].Value = f((T)variables[varIndex].Value, result);
            else
                throw new Exception($"Cannot assign value of type {result.GetType()} to variable of type {variables[varIndex].GetType()}");
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

        private int GetDepthAndTypeList(List<object> list, ref ArrowType type)
        {
            if (list.Count != 0)
            {
                if (list[0] is List<object>)
                    return 1 + GetDepthAndTypeList((List<object>)list[0], ref type);
                else
                    foreach (object o in list)
                    {
                        if (o is ushort)
                        {
                            type = new ArrowNote("", 0, false);
                            return 1;
                        }
                        else if (o is bool)
                        {
                            type = new ArrowBoolean("", false, false);
                            return 1;
                        }
                    }
            }
            type = null;
            return 1;
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

        public object Visit(Statement statement) => null;

        public object Visit(ArrowApplication arrowApplication) => null;
    }
}
