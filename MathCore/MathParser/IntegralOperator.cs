﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using MathCore.Annotations;
using MathCore.MathParser.ExpressionTrees.Nodes;
// ReSharper disable UnusedMember.Global

namespace MathCore.MathParser
{
    /// <summary>Комплексный оператор интегрирования</summary>
    public class IntegralOperator : Functional
    {
        //Использовать адаптивный метод интегрирования
        private bool _IsAdaptive;

        /// <summary>Создание нового комплексного интегратора интегрирования</summary>
        public IntegralOperator() : this("∫") { }

        /// <summary>Создание нового комплексного интегратора интегрирования</summary>
        /// <param name="Name">Имя оператора</param>
        public IntegralOperator([NotNull] string Name) : base(Name) { }

        /// <summary>Инициализация оператора</summary>
        /// <param name="Parameters">Блок параметров</param>
        /// <param name="Function">Блок ядра функции</param>
        /// <param name="Parser">Парсер</param>
        /// <param name="Expression">Внешнее выражение</param>
        public override void Initialize
        (
            MathExpression Parameters,
            MathExpression Function,
            ExpressionParser Parser,
            MathExpression Expression
        )
        {
            base.Initialize(Parameters, Function, Parser, Expression);

            var iterator_var = Parameters.Tree
                        .OfType<VariableValueNode>()
                        .Where(n => n.Parent is EqualityOperatorNode)
                        .Select(n => n.Variable)
                        .FirstOrDefault();
            if(iterator_var is null)
                throw new FormatException();
            var iterator_var_name = iterator_var.Name;
            var iterator_node = Parameters.Tree
                        .OfType<VariableValueNode>()
                        .FirstOrDefault(n => n.Parent is EqualityOperatorNode && n.Variable.Name == $"d{iterator_var_name}");
            var iterator_diff_var = iterator_node?.Variable;
            _IsAdaptive = iterator_diff_var is null;

            Function.Variable.ClearCollection();
            Function.Variable.Add(iterator_var);
            Function.Tree
                .OfType<VariableValueNode>()
                .Where(n => !ReferenceEquals(n.Variable, iterator_var))
                .Foreach(Function, Expression, (n, func, expr) => func.Variable.Add(n.Variable = expr.Variable[n.Variable.Name]));

            Parameters.Variable.ClearCollection();
            Parameters.Variable.Add(iterator_var);
            if (!_IsAdaptive)
            {
                Debug.Assert(iterator_diff_var != null, "iterator_diff_var != null");
                Parameters.Variable.Add(iterator_diff_var);
                if (iterator_node.Parent.Parent.IsLeftSubtree)
                    iterator_node.Parent.Parent.Parent.Left = null;
                else
                    iterator_node.Parent.Parent.Parent.Right = null;
                iterator_node.Parent.Parent = null;
                Parameters.Tree.Root = new FunctionArgumentNode("Domain", Parameters.Tree.Root)
                {
                    Right = new FunctionArgumentNode("step", iterator_node.Parent)
                };
            }
            Parameters.Tree
                .OfType<VariableValueNode>()
                .Where(n => !ReferenceEquals(n.Variable, iterator_var) && !ReferenceEquals(n.Variable, iterator_diff_var))
                .Foreach(Parameters, Expression, (n, pp, expr) => pp.Variable.Add(n.Variable = expr.Variable[n.Variable.Name]));

        }

        /// <summary>Метод определения значения</summary>
        /// <returns>Численное значение элемента выражения</returns>
        public override double GetValue(MathExpression ParametersExpression, MathExpression Function)
        {
            var x = ParametersExpression.Variable[0];
            var x_node = ParametersExpression.Tree
                        .OfType<VariableValueNode>()
                        .First(n => ReferenceEquals(x, n.Variable));
            var interval = (IntervalNode)x_node.Parent.Right;
            Debug.Assert(interval?.Left != null, "interval?.Left != null");
            var min = ((ComputedNode)interval.Left).Compute();
            Debug.Assert(interval.Right != null, "interval?.Right != null");
            var max = ((ComputedNode)interval.Right).Compute();

            Func<double, double> f = xx =>
            {
                x.Value = xx;
                return Function.Compute();
            };

            if(_IsAdaptive)
                return f.GetIntegralValue_Adaptive(min, max);

            var dx = ParametersExpression.Variable[$"d{x_node.Variable.Name}"];
            var dx_node = ParametersExpression.Tree
                        .OfType<VariableValueNode>()
                        .First(n => ReferenceEquals(dx, n.Variable));
            Debug.Assert(dx_node.Parent.Right != null, "dx_node.Parent.Right != null");
            dx.Value = ((ConstValueNode)dx_node.Parent.Right).Value;
            return f.GetIntegralValue(min, max, dx.GetValue());
        }

        /// <summary>Делегат адаптивного интегрирования</summary>
        /// <param name="d">Делегат интегрируемой функции</param>
        /// <param name="Min">Начальное значение интервала интегрирования</param>
        /// <param name="Max">Конечное значение интервала интегрирования</param>
        /// <param name="Parameters">Массив параметров интегрируемой функции</param>
        /// <returns>Значение интеграла</returns>
        private delegate double AdaptiveIntegralDelegate([NotNull] Delegate d, double Min, double Max, [NotNull] double[] Parameters);

        /// <summary>Получить численное значение интеграла адаптивным методом</summary>
        /// <param name="d">Делегат интегрируемой функции</param>
        /// <param name="Min">Начальное значение интервала интегрирования</param>
        /// <param name="Max">Конечное значение интервала интегрирования</param>
        /// <param name="Parameters">Массив параметров интегрируемой функции</param>
        /// <returns>Значение интеграла</returns>
        private static double GetAdaptiveIntegral([NotNull] Delegate d, double Min, double Max, [NotNull] double[] Parameters)
        {
            var pp_len = Parameters.Length;

            var xx = new object[pp_len + 1];
            Array.Copy(Parameters, 0, xx, 1, pp_len);
            Func<double, double> f = x =>
            {
                xx[0] = x;
                return (double)d.DynamicInvoke(xx);
            };
            return f.GetIntegralValue_Adaptive(Min, Max);
        }

        /// <summary>Получить численное значение интеграла методом трапеций</summary>
        /// <param name="d">Делегат интегрируемой функции</param>
        /// <param name="Min">Начальное значение интервала интегрирования</param>
        /// <param name="Max">Конечное значение интервала интегрирования</param>
        /// <param name="Parameters">Массив параметров интегрируемой функции</param>
        /// <param name="dx">Шаг интерполяции</param>
        /// <returns>Значение интеграла</returns>
        private delegate double IntegralDelegate([NotNull] Delegate d, double Min, double Max, [NotNull] double[] Parameters, double dx);

        /// <summary>Получить численное значение интеграла методом трапеций</summary>
        /// <param name="d">Делегат интегрируемой функции</param>
        /// <param name="Min">Начальное значение интервала интегрирования</param>
        /// <param name="Max">Конечное значение интервала интегрирования</param>
        /// <param name="Parameters">Массив параметров интегрируемой функции</param>
        /// <param name="dx">Шаг интерполяции</param>
        /// <returns>Значение интеграла</returns>
        private static double GetIntegral([NotNull] Delegate d, double Min, double Max, [NotNull] double[] Parameters, double dx)
        {
            var pp_len = Parameters.Length;

            var xx = new object[pp_len + 1];
            Array.Copy(Parameters, 0, xx, 1, pp_len);
            Func<double, double> f = x =>
            {
                xx[0] = x;
                return (double)d.DynamicInvoke(xx);
            };
            return f.GetIntegralValue(Min, Max, dx);
        }

        /// <summary>Скомпилировать в выражение</summary>
        /// <returns>Скомпилированное выражение <see cref="System.Linq.Expressions"/></returns>
        public override Expression Compile(MathExpression ParametersExpression, MathExpression Function)
        {
            var iterator = ParametersExpression.Variable[0];
            var x_node = ParametersExpression.Tree
                        .Where(n => n is VariableValueNode)
                        .Cast<VariableValueNode>()
                        .First(n => ReferenceEquals(iterator, n.Variable));

            var interval = (IntervalNode)x_node.Parent.Right ?? throw new InvalidOperationException("Отсутствует правое поддерево у родительского узла");
            var interval_left = (ComputedNode)interval.Left ?? throw new InvalidOperationException("Отсутствует левое поддерево");
            var interval_right = (ComputedNode)interval.Right ?? throw new InvalidOperationException("Отсутствует правое поддерево");
            var min = interval_left.Compile();
            var max = interval_right.Compile();

            var iterator_parameter = Expression.Parameter(typeof(double), iterator.Name);
            var parameters = new[] { iterator_parameter };
            var body = ((ComputedNode)Function.Tree.Root).Compile(parameters);
            var expr = Expression.Lambda(body, parameters).Compile();

            if(_IsAdaptive)
                return Expression.Call(new AdaptiveIntegralDelegate(GetAdaptiveIntegral).Method, new[]
                {
                    Expression.Constant(expr), min, max,
                    Expression.NewArrayInit(typeof(double))
                });

            var dx_var = ParametersExpression.Variable[1];
            var dx_node = ParametersExpression.Tree
                        .Where(n => n is VariableValueNode)
                        .Cast<VariableValueNode>()
                        .First(n => ReferenceEquals(dx_var, n.Variable));
            var dx = dx_node.Compile();

            return Expression.Call(new IntegralDelegate(GetIntegral).Method, new[]
            {
                Expression.Constant(expr), min, max,
                Expression.NewArrayInit(typeof(double)),
                dx
            });
        }

        /// <summary>Скомпилировать в выражение</summary>
        /// <param name="Function">Ядро функции</param>
        /// <param name="Parameters">Массив параметров</param>
        /// <param name="ParametersExpression">Выражение параметров</param>
        /// <returns>Скомпилированное выражение <see cref="System.Linq.Expressions"/></returns>
        public override Expression Compile
        (
            MathExpression ParametersExpression,
            MathExpression Function,
            ParameterExpression[] Parameters
        )
        {
            var iterator = ParametersExpression.Variable[0];
            var x_node = ParametersExpression.Tree
                        .Where(n => n is VariableValueNode)
                        .Cast<VariableValueNode>()
                        .First(n => ReferenceEquals(iterator, n.Variable));

            var interval = (IntervalNode)x_node.Parent.Right ?? throw new InvalidOperationException("Отсутствует правое поддерево у родительского узла");
            var interval_left = (ComputedNode)interval.Left ?? throw new InvalidOperationException("Отсутствует левое поддерево");
            var interval_right = (ComputedNode)interval.Right ?? throw new InvalidOperationException("Отсутствует правое поддерево");
            var min = interval_left.Compile();
            var max = interval_right.Compile();

            var iterator_parameter = Expression.Parameter(typeof(double), iterator.Name);
            var parameters = new[] { iterator_parameter };
            var body = ((ComputedNode)Function.Tree.Root).Compile(parameters);
            var expr = Expression.Lambda(body, parameters).Compile();

            if(_IsAdaptive)
                return Expression.Call(new AdaptiveIntegralDelegate(GetAdaptiveIntegral).Method, new[]
                {
                    Expression.Constant(expr), min, max,
                    Expression.NewArrayInit(typeof(double))
                });

            var dx_var = ParametersExpression.Variable[1];
            var dx_node = ParametersExpression.Tree
                        .Where(n => n is VariableValueNode)
                        .Cast<VariableValueNode>()
                        .First(n => ReferenceEquals(dx_var, n.Variable));
            var dx = dx_node.Compile();

            return Expression.Call(new IntegralDelegate(GetIntegral).Method, new[]
            {
                Expression.Constant(expr), min, max,
                Expression.NewArrayInit(typeof(double), Parameters.Cast<Expression>().ToArray()),
                dx
            });
        }
    }
}