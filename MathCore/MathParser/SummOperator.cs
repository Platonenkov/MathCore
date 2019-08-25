﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using MathCore.MathParser.ExpressionTrees.Nodes;
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassCanBeSealed.Global

namespace MathCore.MathParser
{
    /// <summary>Оператор суммы</summary>
    public class SummOperator : Functional
    {
        /// <summary>Инициализация нового оператора суммы</summary>
        public SummOperator() : this("Σ") { }
        /// <summary>Инициализация нового обператора суммы</summary>
        /// <param name="Name"></param>
        public SummOperator(string Name) : base(Name) { }

        /// <summary>Инициализация оператора</summary>
        /// <param name="Parameters">Блок параметров</param>
        /// <param name="Function">Блок ядра функции</param>
        /// <param name="Parser">Парсер мат.выражения</param>
        /// <param name="Expression">Внешнее мат.выражение</param>
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
                        .Where(n => n is VariableValueNode && n.Parent is EqualityOperatorNode)
                        .Select(n => ((VariableValueNode)n).Variable)
                        .FirstOrDefault();

            Function.Variable.ClearCollection();
            if(iterator_var != null) Function.Variable.Add(iterator_var);
            Function.Tree
                .Where(n => n is VariableValueNode)
                .Cast<VariableValueNode>()
                .Where(n => !ReferenceEquals(n.Variable, iterator_var))
                .Foreach(n => Function.Variable.Add(n.Variable = Expression.Variable[n.Variable.Name]));

            Parameters.Variable.ClearCollection();
            Parameters.Tree
                .Where(n => n is VariableValueNode)
                .Cast<VariableValueNode>()
                .Where(n => !ReferenceEquals(n.Variable, iterator_var))
                .Foreach(n => Parameters.Variable.Add(n.Variable = Expression.Variable[n.Variable.Name]));
        }

        /// <summary>Метод определения значения</summary>
        /// <returns>Численное значение элемента выражения</returns>
        public override double GetValue(MathExpression ParametersExpression, MathExpression Function)
        {
            var parameters_root = ParametersExpression.Tree.Root;
            var iterator = ((VariableValueNode)parameters_root.Left)?.Variable;
            Debug.Assert(iterator != null, "iterator != null");
            var interval = (IntervalNode)parameters_root.Right;
            Debug.Assert(interval != null, "interval != null");
            var min = ((ComputedNode)(interval.Min)).Compute();
            var max = ((ComputedNode)(interval.Max)).Compute();
            var summ = 0.0;
            if(min < max)
                for(int i = (int)min, Max = (int)max; i < Max; i++)
                {
                    iterator.Value = i;
                    summ += Function.Compute();
                }
            else
                for(int i = (int)min, Min = (int)max - 1; i >= Min; i--)
                {
                    iterator.Value = i;
                    summ += Function.Compute();
                }
            return summ;
        }

        /// <summary>Метд суммирования</summary>
        /// <param name="d">Суммируемая функция - ядро</param>
        /// <param name="Min">Начало интервала</param>
        /// <param name="Max">Конец интервала</param>
        /// <param name="Parameters">Массив параметров функции</param>
        /// <returns>Значение суммы функции</returns>
        private delegate double SummDelegate(Delegate d, double Min, double Max, double[] Parameters);

        /// <summary>Получение значения суммы</summary>
        /// <param name="d">Суммируемая функция - ядро</param>
        /// <param name="Min">Начало интервала суммирования</param>
        /// <param name="Max">Конец интервала суммирования</param>
        /// <param name="Parameters">Массив параметров</param>
        /// <returns>Сумма функции</returns>
        private static double GetSumm(Delegate d, double Min, double Max, double[] Parameters)
        {
            var pp_len = Parameters.Length;

            var S = 0.0;
            var xx = new object[pp_len + 1];
            Array.Copy(Parameters, 0, xx, 1, pp_len);
            if(Min < Max)
                for(xx[0] = Min; (double)xx[0] < Max; xx[0] = ((double)xx[0]) + 1)
                    S += (double)d.DynamicInvoke(xx);
            else
                for(xx[0] = Min; (double)xx[0] > Max; xx[0] = ((double)xx[0]) - 1)
                    S += (double)d.DynamicInvoke(xx);
            return S;
        }

        /// <summary>Скомпилировать в выражение</summary>
        /// <param name="ParametersExpression">Выражение блока параметров</param>
        /// <param name="Function">Выражение блока ядра оператора - функции</param>
        /// <returns>Скомпилированное выражение System.Linq.Expressions</returns>
        public override Expression Compile(MathExpression ParametersExpression, MathExpression Function)
        {
            var parameters_root = ParametersExpression.Tree.Root;
            var iterator = ((VariableValueNode)parameters_root.Left)?.Variable;
            Debug.Assert(iterator != null, "iterator != null");
            var interval_node = parameters_root.Right;
            var min = ((ComputedNode)interval_node?.Left)?.Compile();
            Debug.Assert(min != null, "min != null");
            var max = ((ComputedNode)interval_node?.Right)?.Compile();
            Debug.Assert(max != null, "max != null");

            var iterator_parameter = Expression.Parameter(typeof(double), iterator.Name);
            var parameters = new[] { iterator_parameter };
            var body = ((ComputedNode)Function.Tree.Root).Compile(parameters);
            var expr = Expression.Lambda(body, parameters).Compile();

            var expr_p = new[]
            {
                Expression.Constant(expr), min, max,
                Expression.NewArrayInit(typeof(double))
            };

            return Expression.Call(new SummDelegate(GetSumm).Method, expr_p);
        }

        /// <summary>Скомпилировать в выражение</summary>
        /// <param name="Function">Ядро функции</param>
        /// <param name="Parameters">Массив параметров</param>
        /// <param name="ParametersExpression">Выражение параметров</param>
        /// <returns>Скомпилированное выражение System.Linq.Expressions</returns>
        public override Expression Compile(MathExpression ParametersExpression, MathExpression Function, ParameterExpression[] Parameters)
        {
            var parameters_root = ParametersExpression.Tree.Root;
            var iterator = ((VariableValueNode)parameters_root.Left)?.Variable;
            Debug.Assert(iterator != null, "iterator != null");
            var interval_node = parameters_root.Right;
            var min = ((ComputedNode)interval_node?.Left)?.Compile(Parameters);
            Debug.Assert(min != null, "min != null");
            var max = ((ComputedNode)interval_node?.Right)?.Compile(Parameters);
            Debug.Assert(max != null, "max != null");

            var iterator_parameter = Expression.Parameter(typeof(double), iterator.Name);
            Debug.Assert(iterator_parameter != null, "iterator_parameter != null");
            Debug.Assert(Parameters != null, "Parameters != null");
            var parameters = Parameters.AppendFirst(iterator_parameter).ToArray();
            var body = ((ComputedNode)Function.Tree.Root).Compile(parameters);
            var expr = Expression.Lambda(body, parameters).Compile();

            var expr_p = new[]
            {
                Expression.Constant(expr), min, max,
                Expression.NewArrayInit(typeof(double), Parameters)
            };

            return Expression.Call(new SummDelegate(GetSumm).Method, expr_p);
        }
    }
}