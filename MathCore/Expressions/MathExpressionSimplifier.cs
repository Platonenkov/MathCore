using MathCore;
using MathCore.Vectors;

// ReSharper disable once CheckNamespace
namespace System.Linq.Expressions
{
    public class MathExpressionSimplifier : ExpressionVisitorEx
    {
        private static bool IsNumerical(object value) => 
            value is double
            || value is float
            || value is int
            || value is short
            || value is uint
            || value is ushort
            || value is byte
            || value is sbyte
            || value is long
            || value is ulong
            || value is Complex
            || value is Vector2D
            || value is Vector3D
            ;

        protected override Expression VisitBinary(BinaryExpression b)
        {
            var @base = base.VisitBinary(b);

            b = @base as BinaryExpression;
            if(b is null) return @base;

            if (!(b.Left is ConstantExpression l) || !(b.Right is ConstantExpression r)) return @base;
            if (!IsNumerical(l.Value) || !IsNumerical(r.Value)) return @base;
            var left_value = (double)l.Value;
            var right_value = (double)r.Value;

            return b.NodeType switch
            {
                ExpressionType.Add => Expression.Constant(left_value + right_value),
                ExpressionType.AddChecked => Expression.Constant(left_value + right_value),
                ExpressionType.Subtract => Expression.Constant(left_value - right_value),
                ExpressionType.SubtractChecked => Expression.Constant(left_value - right_value),
                ExpressionType.Multiply => Expression.Constant(left_value * right_value),
                ExpressionType.MultiplyChecked => Expression.Constant(left_value * right_value),
                ExpressionType.Divide => Expression.Constant(left_value / right_value),
                ExpressionType.GreaterThan => Expression.Constant(left_value > right_value),
                ExpressionType.GreaterThanOrEqual => Expression.Constant(left_value >= right_value),
                ExpressionType.LessThan => Expression.Constant(left_value < right_value),
                ExpressionType.LessThanOrEqual => Expression.Constant(left_value <= right_value),
                ExpressionType.Equal => Expression.Constant(Math.Abs(left_value - right_value) < double.Epsilon),
                ExpressionType.NotEqual => Expression.Constant(Math.Abs(left_value - right_value) > double.Epsilon),
                _ => @base
            };
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            var @base = base.VisitUnary(u);
            return @base is UnaryExpression
                ? !(u.Operand is ConstantExpression operand)
                    ? @base
                    : IsNumerical(operand.Value)
                        ? @base.NodeType switch
                        {
                            ExpressionType.Negate => Expression.Constant(-(double) operand.Value),
                            ExpressionType.NegateChecked => Expression.Constant(-(double) operand.Value),
                            _ => @base
                        }
                        : @base
                : @base;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            var @base = base.VisitMethodCall(m);

            if(!(@base is MethodCallExpression call) || call.Object != null && !(call.Object is ConstantExpression)) return @base;
            var method = call.Method;
            var obj = (ConstantExpression)call.Object;

            var pp = call.Arguments.ToArray();
            if(!pp.All(p => p is ConstantExpression expression && IsNumerical(expression.Value)))
                return @base;

            var vv = pp.Cast<ConstantExpression>().Select(p => p.Value).ToArray();
            var result = method.Invoke(obj?.Value, vv);
            return Expression.Constant(result);
        }
    }
}