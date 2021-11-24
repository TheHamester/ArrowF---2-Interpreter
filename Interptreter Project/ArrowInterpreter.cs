﻿using System;
using System.Collections.Generic;
using System.Linq;
using InterpreterProject.ArrowExpressions;
using InterpreterProject.ArrowExceptions;
using InterpreterProject.ArrowTypes;

namespace InterpreterProject
{
    public class ArrowInterpreter : IExprVisitor
    {
        private readonly List<ArrowType> variables = new() {
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

        public object Visit(ArrowApplication arrowApplication) => null;

        public object Evaluate(IExprTree expr) => expr.Accept(this);

        public object Visit(Grouping grouping) => grouping.Expr.Accept(this);

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
                        throw new VariableReferencedBeforeAssignmentException(literal.Token.Text, literal.Token.Line);
            }
            return null;
        }

        public object Visit(ArrayExpression arrayExpression)
        {
            List<object> elements = new();
            foreach (IExprTree expr in arrayExpression.Elements)
                elements.Add(Evaluate(expr));
            return elements;
        }

        public object Visit(Unary unary)
        {
            object right = Evaluate(unary.Right);
            ArrowType type = new();
            switch (unary.Token.Type)
            { 
                case TokenType.Invert:
                    if (right is ushort invert)
                        return (ushort)~invert;
                    if (right is List<object> invertArray)
                    {
                        ApplyUnaryOperationToArray<ushort>(invertArray, (o) => (ushort)(~o), unary.Token);
                        return right;
                    }
                    break;
                case TokenType.Not:
                    if (right is bool not)
                        return !not;
                    if (right is List<object> notArray)
                    {
                        ApplyUnaryOperationToArray<bool>(notArray, (o) => (!o), unary.Token);
                        return right;
                    }
                    break;
                default:
                    throw new Exception("wtf");
            }
            throw new OperationTypeMismatchException(right.GetType().ToString(), "", unary.Token.Text, false, unary.Token.Line);
        }

        public object Visit(Binary binary)
        {
            object left = Evaluate(binary.Left);
            object right = Evaluate(binary.Right);
            object result;

            ArrowType type = new();
            int topLayer = 0;
            switch (binary.Token.Type)
            { 
                case TokenType.Plus:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l + r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, List<object>>(left, right, (l, r) => l.Concat(r).ToList(), binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Minus:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l - r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, List<object>>(left, right, (l, r)
                           => { List<object> lCopy = new(l); List<object> rCopy = new(r); for (int i = rCopy.Count-1; i >= 0; i--) { if(lCopy.Remove(rCopy[i])) rCopy.Remove(rCopy[i]); } return lCopy.Count >= rCopy.Count ? lCopy.Concat(rCopy).ToList() : rCopy.Concat(lCopy).ToList(); }, binary.Token.Line);
                    if (result != null) return (List<object>)result;

                    break;
                case TokenType.Times:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l * r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, ushort>(left, right, (l, r) 
                        => { List<object> list = new(l); List<object> res = new(); for (int i = 0; i < r; i++) res = res.Concat(list).ToList(); return res; }, binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Divide:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l / r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<List<object>, ushort>(left, right, (l, r)
                        => { int n = l.Count / r; List<object> res = new();  for (int i = 0; i < n; i++) res.Add(l[i]); return res; }, binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Modulo:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l % r), binary.Token.Line);
                    if (result != null) return (ushort)result;
                    break;
                case TokenType.And:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l & r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l && r, binary.Token.Line);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l & r), (l, r) => l && r, binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Xor:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l ^ r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l ^ r, binary.Token.Line);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l ^ r), (l, r) => l ^ r, binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.Or:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l | r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    result = BinaryReturn<bool, bool>(left, right, (l, r) => l || r, binary.Token.Line);
                    if (result != null) return (bool)result;

                    result = TryApplyArrayToArrayOperation(left, right, (l, r) => (ushort)(l | r), (l, r) => l || r, binary.Token.Line);
                    if (result != null) return (List<object>)result;
                    break;
                case TokenType.LeftShift:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l << r), binary.Token.Line);
                    if (result != null)  return (ushort)result;

                    if (left is List<object> leftArrayLS && right is ushort rightNoteLS)
                    {
                        topLayer = GetDepthAndTypeList(leftArrayLS, ref type);
                        MatchesTypeAndDepth(leftArrayLS, type, topLayer, topLayer, binary.Token.Line);

                        leftArrayLS.RemoveRange(0, rightNoteLS);

                        return left;
                    }
                    break;
                case TokenType.RightShift:
                    result = BinaryReturn<ushort, ushort>(left, right, (l, r) => (ushort)(l >> r), binary.Token.Line);
                    if (result != null) return (ushort)result;

                    if (left is List<object> leftArrayRS && right is ushort rightNoteRS)
                    {
                        topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                        MatchesTypeAndDepth(leftArrayRS, type, topLayer, topLayer, binary.Token.Line);

                        leftArrayRS.RemoveRange(leftArrayRS.Count - rightNoteRS, rightNoteRS);

                        return left;
                    }
                    break;
            }

            throw new OperationTypeMismatchException(left.GetType().ToString(), right.GetType().ToString(), binary.Token.Text, true, binary.Token.Line);
        }

        private object BinaryReturn<T,U>(object left, object right, Func<T,U,T> f, int line)
        {
            int topLayer;
            ArrowType type = new();
            if (left is T plusL1 && right is U plusR1)
            {
                if (left is List<object> leftArray)
                {
                    topLayer = GetDepthAndTypeList(leftArray, ref type);
                    MatchesTypeAndDepth(leftArray, type, topLayer, topLayer, line);
                    if(right is List<object> rightArray)
                        MatchesTypeAndDepth(rightArray, type, topLayer, topLayer, line);
                }

                return f(plusL1, plusR1);
            }
            if (left is U plusL2 && right is T plusR2)
            {
                if (right is List<object> rightArray)
                {
                    topLayer = GetDepthAndTypeList(rightArray, ref type);
                    MatchesTypeAndDepth(rightArray, type, topLayer, topLayer, line);
                    if (left is List<object> leftArray)
                        MatchesTypeAndDepth(leftArray, type, topLayer, topLayer, line);
                }
                return f(plusR2, plusL2);
            }
            return null;
        }

        private void ApplyUnaryOperationToArray<T>(List<object> list, Func<T,T> func, Token token)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i] is List<object> sublist)
                    ApplyUnaryOperationToArray(sublist, func, token);
                else
                {
                    if (list[i] is T element)
                        list[i] = func(element);
                    else
                        throw new OperationTypeMismatchException(list[i].GetType().ToString(), "", token.Text, false, token.Line);
                }
            }
        }

        private List<object> TryApplyArrayToArrayOperation(object left, object right, Func<ushort, ushort, ushort> f1, Func<bool, bool, bool> f2, int line)
        {
            int topLayer;
            ArrowType type = new();
            if (left is List<object> leftArray && right is List<object> rightArray)
            {
                topLayer = GetDepthAndTypeList((List<object>)left, ref type);
                MatchesTypeAndDepth(leftArray, type, topLayer, topLayer, line);
                MatchesTypeAndDepth(rightArray, type, topLayer, topLayer, line);
                if (type is ArrowNote)
                    ApplyArrayToArrayOperation(leftArray, rightArray, f1, line);
                else if (type is ArrowBoolean)
                    ApplyArrayToArrayOperation(leftArray, rightArray, f2, line);
                return leftArray;
            }
            return null;
        }

        private void ApplyArrayToArrayOperation<T>(List<object> l1, List<object> l2, Func<T, T, T> func, int line)
        {
            int i = 0;
            while (i < l1.Count && i < l2.Count)
            {
                if ((l1[i] is List<object> l1i) && (l2[i] is List<object> l2i))
                    ApplyArrayToArrayOperation(l1i, l2i, func, line);
                else
                {
                    if (l1[i] is T l1element && l2[i] is T l2element)
                        l1[i] = func(l1element, l2element);
                    else
                        throw new OperationTypeMismatchException(l1.GetType().ToString(), l2.GetType().ToString(), "", true, line);
                }
                i++;
            }

            if (i < l1.Count)
                SetElementsToZero(l1, i);

            if (i < l2.Count)
                AddZeros(l1, l2, i);        
        }

        private void SetElementsToZero(List<object> list, int starts)
        {
            for (int i = starts; i < list.Count; i++)
            {
                if (list[i] is List<object> sublist)
                    SetElementsToZero(sublist, 0);
                else if (list[i] is ushort)
                    list[i] = (ushort)0;
                else if (list[i] is bool)
                    list[i] = false;
            }
        }

        private void AddZeros(List<object> l1, List<object> l2, int starts)
        {
            List<object> l;
            for (int i = starts; i < l2.Count; i++)
            {
                if (l2[i] is List<object> sublist)
                {
                    l = new List<object>();
                    AddZeros(l, sublist, 0);
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

            if (type is ArrowNote && result is ushort resultNote)
                variables[variables.Count() - 1].Value = resultNote;
            else if (type is ArrowBoolean && result is bool resultBool)
                variables[variables.Count() - 1].Value = resultBool;
            else if (type is ArrowArray && result is List<object> resultArray)
            {
                variables[variables.Count() - 1].Value = resultArray;
                ArrowType t = new();
                int topLayer = GetDepthAndType((ArrowArray)variables[variables.Count() - 1], ref t);
                MatchesTypeAndDepth((List<object>)variables[variables.Count() - 1].Value, t, topLayer, topLayer, assignment.Line);
            }
            else
                throw new AssignmentTypeMismatchException(type.Id, result.GetType().ToString(), variables[variables.Count() - 1].Value.GetType().ToString(), assignment.Line);
            return null;
        }

        public object Visit(Reassignment reassignment)
        {
            object result = Evaluate(reassignment.Right);
            
            int varIndex = variables.IndexOf(variables.First(v => v.Id == reassignment.Id));
            if (varIndex == -1)
                throw new VariableReferencedBeforeAssignmentException(reassignment.Id, reassignment.Line);
            if(variables[varIndex].IsConst)
                throw new UnableToModifyAConstantVariableException(reassignment.Id, reassignment.Line);
            switch (reassignment.Operation)
            {
                case TokenType.Assign:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteAssign)
                        variables[varIndex].Value = resultNoteAssign;
                    else if (variables[varIndex] is ArrowBoolean && result is bool resultBoolAssign)
                        variables[varIndex].Value = resultBoolAssign;
                    else if (variables[varIndex] is ArrowArray && result is List<object> resultArrayAssign)
                    { 
                        ArrowType t = new();
                        int topLayer = GetDepthAndType((ArrowArray)variables[varIndex], ref t);

                        MatchesTypeAndDepth((List<object>)result, t, topLayer, topLayer, reassignment.Line);
                        variables[varIndex].Value = resultArrayAssign;
                    }
                    break;
                case TokenType.OrEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteOr)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value | resultNoteOr);
                    break;
                case TokenType.AndEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteAnd)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value & resultNoteAnd);
                    break;
                case TokenType.XorEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteXor)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value ^ resultNoteXor);
                    break;
                case TokenType.PlusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNotePlus)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value + resultNotePlus);
                    break;
                case TokenType.MinusEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteMinus)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value - resultNoteMinus);
                    break;
                case TokenType.TimesEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteTimes)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value * resultNoteTimes);
                    break;
                case TokenType.DivideEquals:
                    if (variables[varIndex] is ArrowNote && result is ushort resultNoteDivide)
                        variables[varIndex].Value = (ushort)((ushort)variables[varIndex].Value / resultNoteDivide);
                    break;

            }
            throw new AssignmentTypeMismatchException(reassignment.Id, result.GetType().ToString(), variables[varIndex].GetType().ToString(), reassignment.Line);
        }

        public object Visit(Indexer indexer)
        {
            object index = Evaluate(indexer.Expr);

            if (index is ushort ind)
            {

                object indexable = Evaluate(indexer.Indexable);

                if (indexable is List<object> indexableArray)
                    return indexableArray[ind];
                else if (indexable is ArrowArray indexableArrowArray)
                    return ((List<object>)indexableArrowArray.Value)[ind];
                else
                    throw new UnindexableTypeException(indexable.GetType().ToString(), indexer.Line);
            }
            else
                throw new InvalidIndexException(index.GetType().ToString(), indexer.Line);
        }

        private int GetDepthAndType(ArrowArray arrowArray, ref ArrowType type)
        {
            if (arrowArray.Type is ArrowArray arrayType)
                return 1 + GetDepthAndType(arrayType, ref type);
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
                if (list[0] is List<object> list0)
                    return 1 + GetDepthAndTypeList(list0, ref type);
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

        private void MatchesTypeAndDepth(List<object> value, ArrowType type, int fullLayer, int layer, int line)
        {
            foreach (object o in value)
            {
                if (o is List<object> o1)
                    MatchesTypeAndDepth(o1, type, fullLayer, layer - 1, line);
                else if (o is bool)
                {
                    if (type is not ArrowBoolean)
                        throw new UnmatchedArrayTypeException("Boolean", type.ToString(), line);
                    if (layer != 1)
                        throw new UnmatchedArrayDepthException(fullLayer, line);

                }
                else if (o is ushort)
                {
                    if(type is not ArrowNote)
                        throw new UnmatchedArrayTypeException("Note", type.ToString(), line);
                    if (layer != 1)
                        throw new UnmatchedArrayDepthException(fullLayer, line);
                }
            }
        }
    }
}
