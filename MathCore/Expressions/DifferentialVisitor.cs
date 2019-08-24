using System.Collections.Generic;

namespace System.Linq.Expressions
{
    public class DifferentialVisitor : ExpressionVisitorEx
    {
        private static bool CheckNumType(Type type)
        {
            return type == typeof(double)
                   || type == typeof(int)
                   || type == typeof(short)
                   || type == typeof(long)
                   || type == typeof(float);
        }

        private static void CheckValueType(Type type)
        {
            if(!CheckNumType(type)) throw new NotSupportedException("���������������� ��� ������ " + type);
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            CheckValueType(c.Type);
            return Expression.Constant(0.0);
        }

        private static Expression sAdd(Expression a, Expression b) { return sAdd(Expression.Add(a, b)); }
        private static Expression sAdd(double a, Expression b) { return sAdd(Expression.Add(Expression.Constant(a), b)); }
        //private static Expression sAdd(Expression a, double b) { return sAdd(Expression.Add(a, Expression.Constant(b))); }
        //private static Expression sInc(Expression a) { return sAdd(a, 1); }
        //private static Expression sSubtract(Expression a, Expression b) { return sAdd(Expression.Subtract(a, b)); }
        private static Expression sSubtract(double a, Expression b) { return sAdd(Expression.Subtract(Expression.Constant(a), b)); }
        private static Expression sSubtract(Expression a, double b) { return sAdd(Expression.Subtract(a, Expression.Constant(b))); }
        //private static Expression sDec(Expression a) { return sSubtract(a, 1); }
        private static Expression sAdd(BinaryExpression b)
        {
            var l = b.Left as ConstantExpression;
            var r = b.Right as ConstantExpression;
            if(l == null && r == null) return b;
            if(l != null && r != null)
                return b.NodeType == ExpressionType.Add
                    ? Expression.Constant((double)l.Value + (double)r.Value)
                    : Expression.Constant((double)l.Value - (double)r.Value);
            if(l != null && l.Value.Equals(0.0))
                return b.NodeType == ExpressionType.Add
                    ? b.Right
                    : Expression.MakeUnary(ExpressionType.Negate, b.Right, b.Right.Type);
            if(r != null && r.Value.Equals(0.0))
                return b.Left;
            return b;
        }

        private static Expression sMultiply(Expression a, Expression b) { return sMultiply(Expression.Multiply(a, b)); }
        //private static Expression sMultiply(double a, Expression b) { return sMultiply(Expression.Multiply(Expression.Constant(a), b)); }
        //private static Expression sMultiply(Expression a, double b) { return sMultiply(Expression.Multiply(a, Expression.Constant(b))); }
        private static Expression sMultiply(BinaryExpression b)
        {
            var l = b.Left as ConstantExpression;
            var r = b.Right as ConstantExpression;
            if(l == null && r == null) return b;
            if(l != null && r != null)
                return Expression.Constant((double)l.Value * (double)r.Value);
            if(l != null)
            {
                if(l.Value.Equals(0.0)) return l;
                if(l.Value.Equals(1.0)) return b.Right;
            }
            if(r != null)
            {
                if(r.Value.Equals(0.0)) return r;
                if(r.Value.Equals(1.0)) return b.Left;
            }
            return b;
        }

        private static Expression sDivade(Expression a, Expression b) { return sDivade(Expression.Divide(a, b)); }
        private static Expression sDivade(double a, Expression b) { return sDivade(Expression.Divide(Expression.Constant(a), b)); }
        //private static Expression sDivade(Expression a, double b) { return sDivade(Expression.Divide(a, Expression.Constant(b))); }
        private static Expression sDivade(BinaryExpression b)
        {
            var l = b.Left as ConstantExpression;
            var r = b.Right as ConstantExpression;
            if(l == null && r == null) return b;
            if(l != null && r != null)
                return Expression.Constant((double)l.Value / (double)r.Value);
            if(l != null)
            {
                if(l.Value.Equals(0.0)) return l;
                if(l.Value.Equals(1.0)) return b;
            }
            if(r != null)
            {
                if(r.Value.Equals(0.0)) return Expression.Constant(double.PositiveInfinity);
                if(r.Value.Equals(1.0)) return b.Left;
            }
            return b;
        }

        private static Expression sPower(Expression a, Expression b) { return sPower(Expression.Power(a, b)); }
        private static Expression sPower(Expression a, double b) { return sPower(Expression.Power(a, Expression.Constant(b))); }
        private static Expression sPower(BinaryExpression b)
        {
            var l = b.Left as ConstantExpression;
            var r = b.Right as ConstantExpression;
            if(l == null && r == null) return b;
            if(l != null && r != null)
                return Expression.Constant(Math.Pow((double)l.Value, (double)r.Value));
            if(l != null && (l.Value.Equals(0.0) || l.Value.Equals(1.0))) return l;
            if(r != null)
            {
                if(r.Value.Equals(0.0)) return Expression.Constant(1.0);
                if(r.Value.Equals(1.0)) return b.Left;
            }
            return b;
        }

        private static Expression sCall(MethodCallExpression m)
        {
            var args = m.Arguments;
            if(args.All(a => a is ConstantExpression))
            {
                var v = args.Cast<ConstantExpression>().Select(a => a.Value).ToArray();
                var objExpr = m.Object as ConstantExpression;
                object o = null;
                if(objExpr != null) o = objExpr.Value;
                m.Method.Invoke(o, v);
            }
            return m;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            //return base.VisitBinary(b);
            var NodeType = b.NodeType;
            switch(NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    {
                        var expr = base.VisitBinary(b);
                        return expr is BinaryExpression
                            ? sAdd((BinaryExpression)expr)
                            : expr;
                    }

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    {
                        var left = b.Left;
                        var right = b.Right;
                        var left_d = Visit(left);
                        var right_d = Visit(right);

                        var l = sMultiply(left_d, right);
                        var r = sMultiply(left, right_d);
                        return sAdd(l, r);
                    }

                case ExpressionType.Divide:
                    {
                        var x = b.Left;
                        var y = b.Right;
                        var dx = Visit(x);
                        var dy = Visit(y);

                        var l = sMultiply(dx, y);
                        var r = sMultiply(x, dy);
                        var X = sAdd(l, r);
                        var Y = sPower(y, 2);
                        return sDivade(X, Y);
                    }
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return base.VisitBinary(b);
                case ExpressionType.UnaryPlus:
                    return base.VisitBinary(b);
                case ExpressionType.Power:
                    {
                        var x = b.Left;
                        var y = b.Right;
                        var dx = Visit(x);

                        var A = sPower(x, sSubtract(y, 1));
                        var B = sMultiply(dx, y);
                        var AB = sMultiply(B, A);

                        if(y is ConstantExpression) return AB;
                        var dy = Visit(y);
                        var C = sPower(b);
                        var d = sCall(Expression.Call(typeof(Math), "Log", null, x));
                        var D = sMultiply(C, dy);
                        var CD = sMultiply(d, D);
                        return sAdd(AB, CD);
                    }
                default:
                    throw new NotSupportedException("���������������� ��� ��������");
            }
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            CheckValueType(p.Type);
            return Expression.Constant(1.0);
        }

        private Expression MathMethod(string Name, params Expression[] p)
        {
            return Expression.Call(typeof(Math), Name, null, p);
        }

        protected Expression VisitMathMethodCall(MethodCallExpression m)
        {
            switch(m.Method.Name)
            {
                case "Pow": return Visit(sPower(Expression.Power(m.Arguments[0], m.Arguments[1])));
                case "Sin":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), MathMethod("Cos", x));
                    }
                case "Cos":
                    {
                        var x = m.Arguments[0];
                        return Expression.Negate(sMultiply(Visit(x), MathMethod("Sin", x)));
                    }
                case "Tan":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), sDivade(1, sPower(MathMethod("Cos", x), 2)));
                    }
                case "Asin":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), sDivade(1, MathMethod("Sqrt", sSubtract(1, sPower(x, 2)))));
                    }
                case "Acos":
                    {
                        var x = m.Arguments[0];
                        return Expression.Negate(sMultiply(Visit(x), sDivade(1, MathMethod("Sqrt", sSubtract(1, sPower(x, 2))))));
                    }
                case "Atan":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), sDivade(1, sAdd(1, sPower(x, 2))));
                    }
                case "Sinh":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), MathMethod("Cosh", x));
                        //return sMultiply(dx, Expression.Call(typeof(Math), "Cos", null, x));
                    }
                case "Cosh":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), MathMethod("Sinh", x));
                    }
                case "Tanh":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), sDivade(1, sPower(sDivade(1, MathMethod("Tanh", x)), 2)));
                    }
                case "Abs":
                    {
                        var x = m.Arguments[0];
                        var condition = Expression.Condition
                            (
                                Expression.Equal(x, Expression.Constant(0.0)),
                                Expression.Constant(0.0),
                                Expression.Convert(MathMethod("Sign", x), typeof(double))
                            );
                        return sMultiply(Visit(x), condition);
                    }
                case "Sign":
                    {
                        var x = m.Arguments[0];
                        var condition = Expression.Condition
                            (
                                Expression.Equal(x, Expression.Constant(0.0)),
                                Expression.Constant(double.PositiveInfinity),
                                Expression.Constant(0.0)
                            );
                        return sMultiply(Visit(x), condition);
                    }
                case "Sqrt":
                    return Visit(sPower(m.Arguments[0],
                        Expression.Divide(Expression.Constant(1.0), Expression.Constant(2.0))));
                case "Exp":
                    {
                        var x = m.Arguments[0];
                        return sMultiply(Visit(x), MathMethod("Exp", x));
                    }
                case "Log":
                    {
                        var x = m.Arguments[0];
                        if(m.Arguments.Count > 1)
                        {
                            var a = m.Arguments[1];
                            var expr = sDivade(MathMethod("Log", x), MathMethod("Log", a));
                            return Visit(expr);
                        }
                        var dx = Visit(x);
                        return sDivade(dx, x);
                    }
                case "Log10":
                    {
                        var x = m.Arguments[0];
                        var expr = MathMethod("Log", x, Expression.Constant(10.0));
                        return Visit(expr);
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        public class MethodDifferentialEventArgs : EventArgs
        {
            public MethodCallExpression Method { get; private set; }

            public Expression DifferentialExpression { get; set; }

            public MethodDifferentialEventArgs(MethodCallExpression Method) { this.Method = Method; }
        }

        public event EventHandler<MethodDifferentialEventArgs> MethodDifferential;

        protected virtual void OnMethodDifferential(MethodDifferentialEventArgs Args)
        {
            var handlers = MethodDifferential;
            if(handlers != null) handlers.Invoke(this, Args);
        }

        private Expression OnMethodDifferential(MethodCallExpression method)
        {
            var args = new MethodDifferentialEventArgs(method);
            OnMethodDifferential(args);
            return args.DifferentialExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var result = OnMethodDifferential(m);
            if(result != null) return result;

            var method = m.Method;
            if(method.DeclaringType == typeof(Math))
                return VisitMathMethodCall(m);
            throw new NotSupportedException();
            //return base.VisitMethodCall(m);
        }
    }
}