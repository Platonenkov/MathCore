﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MathCore;
using MathCore.Annotations;
using MathCore.DifferencialEquations.Numerical;
using MathCore.Evulations;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace System
{
    //Вещественная функция одного вещественного оаргумента
    using Function = Func<double, double>;

    /// <summary>Класс методов-расширений для функции</summary>
    public static class FuncExtentions
    {
        [NotNull]
        public static Task<TResult> InvokeAsync<TResult>([NotNull] this Func<TResult> func) => Task.Factory.FromAsync(func.BeginInvoke, func.EndInvoke, null);
        [NotNull]
        public static Task<TResult> InvokeAsync<TValue, TResult>([NotNull] this Func<TValue, TResult> func, TValue value) => Task.Factory.FromAsync(func.BeginInvoke, func.EndInvoke, value, null);

        /// <summary>Преобразование функции в вычисление</summary>
        /// <typeparam name="T">Тип возвращаемого функцией результата</typeparam>
        /// <param name="function">Преобразуемая функция</param>
        /// <param name="Name">Имя вычисления</param>
        /// <returns>Вычисление функции</returns>
        [NotNull]
        public static FunctionEvulation<T> ToEvulation<T>([NotNull] this Func<T> function, [CanBeNull] string Name = null) => Name is null ? new FunctionEvulation<T>(function) : new NamedFunctionEvulation<T>(function, Name);

        /// <summary>Поиск нуля функции методом Ньютона</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="df">Дифференциал исследуемой функции</param>
        /// <param name="x0">Начальное приближение</param>
        /// <param name="max_iterations">Максимальное количество итераций. <exception cref="IndexOutOfRangeException"/> при превышении</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="IndexOutOfRangeException">Если корень не найден за указанное число шагов</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_NewtonsMethod
        (
            [NotNull] this Function f,
            [NotNull] Function df,
            double x0,
            int max_iterations,
            double eps = 1e-5
        )
        {
            while (max_iterations-- > 0)
            {
                var x = x0 - f(x0) / df(x0);
                if (Math.Abs(x - x0) < eps) return x;
                x0 = x;
            }
            throw new IndexOutOfRangeException("Метод превысил допустимое число шагов поиска");
        }

        /// <summary>Поиск нуля функции методом Ньютона</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="df">Дифференциал исследуемой функции</param>
        /// <param name="x0">Начальное приближение</param>
        /// <param name="max_iterations">Максимальное количество итераций. <exception cref="IndexOutOfRangeException"/> при превышении</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="IndexOutOfRangeException">Если корень не найден за указанное число шагов</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_NewtonsMethodAsync
        (
            [NotNull] this Function f,
            [NotNull] Function df,
            double x0,
            int max_iterations,
            double eps = 1e-5
        ) =>
            Task.Factory.StartNew(() => f.GetRoot_NewtonsMethod(df, x0, max_iterations, eps));

        /// <summary> Поиск нуля функции методом бисекции</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="max_iterations">Максимальное количество итераций. <exception cref="IndexOutOfRangeException"/> при превышении</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="ArithmeticException"><paramref name="f" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
        /// <exception cref="IndexOutOfRangeException">Если корень не найден за указанное число шагов</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_BisectionMethod
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            int max_iterations,
            double eps = 1e-5
        )
        {
            while (max_iterations-- > 0)
            {
                var x = (x1 + x2) / 2;

                var fx = f(x);

                if (fx.Equals(0d) || (x2 - x1) / 2 < eps) return x;
                if (Math.Sign(fx) == Math.Sign(f(x1)))
                    x1 = x;
                else
                    x2 = x;
            }
            throw new IndexOutOfRangeException("Метод превысил допустимое число шагов поиска");
        }

        /// <summary> Поиск нуля функции методом бисекции</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="max_iterations">Максимальное количество итераций. <exception cref="IndexOutOfRangeException"/> при превышении</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="ArithmeticException"><paramref name="f" /> is equal to <see cref="F:System.Double.NaN" />. </exception>
        /// <exception cref="IndexOutOfRangeException">Если корень не найден за указанное число шагов</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_BisectionMethodAsync
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            int max_iterations,
            double eps = 1e-5
        ) =>
            Task.Factory.StartNew(() => f.GetRoot_BisectionMethod(x1, x2, max_iterations, eps));

        /// <summary> Поиск нуля функции методом Золотого сечения</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_GoldenSection([NotNull] this Function f, double x1, double x2, double eps = 1e-5)
        {
            const double k = Consts.GoldenRatio;

            var d = x2 - x1;
            var X1 = x1 + k * d;
            var X2 = x2 - k * d;

            while (Math.Abs(X2 - X1) > eps)
            {
                var _X1 = f(X1);
                var _X2 = f(X2);

                if (_X2 < _X1)
                {
                    x2 = X1;
                    X1 = X2;  //fd=fc;fc=f(c)
                    X2 = x2 - k * (x2 - x1);
                }
                else
                {
                    x1 = X2;
                    X2 = X1;  //fc=fd;fd=f(d)
                    X1 = x1 + k * (x2 - x1);
                }
            }
            return (x2 + x1) / 2;
        }

        /// <summary> Поиск нуля функции методом Золотого сечения</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_GoldenSectionAsync([NotNull] this Function f, double x1, double x2, double eps = 1e-5) => f.GetRoot_GoldenSectionAsync(x1, x2, eps);

        /// <summary> Поиск нуля функции методом Троичного деления</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_TernarySearch([NotNull] this Function f, double x1, double x2, double eps = 0.0001)
        {
            while (Math.Abs(x2 - x1) > eps)
            {
                var d = (x2 - x1) / 3;
                var X1 = x1 + d;
                var X2 = x2 - d;
                if (f(X1) < f(X2)) x1 = X1; else x2 = X2;
            }
            return (x2 - x1) / 2;
        }

        /// <summary> Поиск нуля функции методом Троичного деления</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_TernarySearchAsync([NotNull] this Function f, double x1, double x2, double eps = 0.0001) => Task.Factory.StartNew(() => f.GetRoot_TernarySearch(x1, x2, eps));

        /// <summary>Поиск нуля функции методом False position</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="max_iterations">Максимальное допустимое число итераций метода</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_FalsiPositionMethod
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            int max_iterations,
            double eps = 0.0001
        )
        {/* starting values at endpoints of interval */
            var _x1 = f(x1);

            var _x2 = f(x2);

            var x = 0d;

            for (int n = 0, side = 0; n < max_iterations; n++)
            {
                x = (_x1 * x2 - _x2 * x1) / (_x1 - _x2);
                if (Math.Abs(x2 - x1) < eps * Math.Abs(x2 + x1)) break;
                var _x = f(x);

                if (_x * _x2 > 0)
                {
                    /* _x and ft have same sign, copy r to x2 */
                    x2 = x;
                    _x2 = _x;
                    if (side == -1) _x1 /= 2;
                    side = -1;
                }
                else if (_x1 * _x > 0)
                {
                    /* _x and fs have same sign, copy r to x1 */
                    x1 = x;
                    _x1 = _x;
                    if (side == +1) _x2 /= 2;
                    side = +1;
                }
                else //_x * f_ very small (looks like zero)
                    break;
            }
            return x;
        }

        /// <summary>Поиск нуля функции методом False position</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="max_iterations">Максимальное допустимое число итераций метода</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_FalsiPositionMethodAsync
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            int max_iterations,
            double eps = 0.0001
        ) =>
            Task.Factory.StartNew(() => f.GetRoot_FalsiPositionMethod(x1, x2, max_iterations, eps));

        /// <summary> Поиск нуля функции методом хорд</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static double GetRoot_MethodOfChords([NotNull] this Function f, double x1, double x2, double eps = 0.0001)
        {
            while (Math.Abs(x2 - x1) > eps)
            {
                var _x1 = f(x1);

                var _x2 = f(x2);
                x1 = x2 - (x2 - x1) * _x2 / (_x2 - _x1);
                x2 = x1 - (x1 - x2) * _x1 / (_x1 - _x2);
            }
            return x2;
        }

        /// <summary> Поиск нуля функции методом хорд</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала поиска</param>
        /// <param name="x2">Конец интервала поиска</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Значение аргумента нуля функции</returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [NotNull]
        public static Task<double> GetRoot_MethodOfChordsAsync([NotNull] this Function f, double x1, double x2, double eps = 0.0001) => Task.Factory.StartNew(() => f.GetRoot_MethodOfChords(x1, x2, eps));

        /// <summary>Карирование функции двух параметров</summary>
        /// <typeparam name="TArg1">Тип значение первого параметра функции</typeparam>
        /// <typeparam name="TArg2">Тип значение второго параметра функции</typeparam>
        /// <typeparam name="TResult">Тип результата функции</typeparam>
        /// <param name="f">Карируемая функция</param>
        [NotNull]
        public static Func<TArg2, Func<TArg1, TResult>> Carring<TArg1, TArg2, TResult>([NotNull] this Func<TArg1, TArg2, TResult> f) => y => x => f(x, y);

        /// <summary>Сложение функции с числом g(x) = f(x) + a</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="a">Прибавляемое число</param>
        /// <returns>Новая функция, значения которой равны значениям исходной функции плюс указанному числу</returns>
        [NotNull]
        public static Function Add([NotNull] this Function f, double a) => a.Equals(0) ? f : x => f(x) + a;

        /// <summary>Сложение двух функций g(x) = f1(x) + f2(x)</summary>
        /// <param name="f1">Функция - первое слогаемое</param>
        /// <param name="f2">Функция - второе слогаемое</param>
        /// <returns>Функция, значения которой равны сумме значений двух исходных функций</returns>
        [NotNull]
        public static Function Add([NotNull] this Function f1, [NotNull] Function f2) => x => f1(x) + f2(x);

        /// <summary>Изменение знака аргумента функции g(x) = f(-x)</summary>
        /// <param name="f">Исходная функция</param>
        /// <returns>Новая функция, значения аргумента которой отрицательны по отношению к аргументу исходной функци g(x) = f(-x)</returns>
        [NotNull]
        public static Function ArgumentReverse([NotNull] this Function f) => x => f(-x);

        /// <summary>Смещение аргумента функции на указанное значение g(x) = f(x-a)</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="x0">Значение смещения аргумента</param>
        /// <returns>Новая функция со смещённым аргументом</returns>
        [NotNull]
        public static Function ArgumentShift([NotNull] this Function f, double x0) => x0.Equals(0) ? f : x => f(x - x0);

        /// <summary>Сжатие аргумента функци g(x) = f(k * x + b)</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="k">Коэффициент сжатия аргумента</param>
        /// <param name="b">Коэффициент смещения аргумента</param>
        /// <returns>Новая функция со сжатым и смещённым аргументом</returns>
        [NotNull]
        public static Function ArgumentCompression([NotNull] this Function f, double k, double b = 0d) =>
            k.Equals(1) && b.Equals(0)
                ? f
                : (k.Equals(1)
                    ? f.ArgumentShift(b)
                    : (b.Equals(0)
                        ? (Function)(x => f(k * x))
                        : x => f(k * x + b)));

        /// <summary>Деление функции на число g(x) = f(x) / a</summary>
        /// <param name="f">Делимая функция</param>
        /// <param name="a">Вещественный делитель</param>
        /// <returns>
        /// Функция, значния которой равны значениям исходной функции, делимые на вещественный делитель.
        /// Если вещественный делитель равен 0, то возвращается функция, значения которой равны +бесконечности если
        /// аргумент больше нуля, -бесконечности, если аргумент меньше нуля и NaN, если аргумент равен нулю.
        /// </returns>
        [NotNull]
        public static Function Divade([NotNull] this Function f, double a) => a.Equals(1) ? f : (a.Equals(0) ? (Function)(x => x > 0 ? double.PositiveInfinity : (x < 0 ? double.NegativeInfinity : double.NaN)) : x => f(x) / a);

        /// <summary>Деление функции на функцию g(x) = f1(x) / f2(x)</summary>
        /// <param name="f1">Функция - делимое</param>
        /// <param name="f2">Функция - делитель</param>
        /// <returns>Функция, значения которой равны отношению значений исходных функций</returns>
        [NotNull]
        public static Function Divade([NotNull] this Function f1, [NotNull] Function f2) => x => f1(x) / f2(x);

        /// <summary>Функция от функци q(f(x))</summary>
        /// <param name="f">Внутренняя функция</param>
        /// <param name="q">Внешняя функция</param>
        /// <returns>ФУнкция q от функции f</returns>
        [NotNull]
        public static Function Func([NotNull] this Function f, [NotNull] Function q) => x => q(f(x));

        /// <summary>Функция от функции - функционал (преобразователь функции)</summary>
        /// <typeparam name="TIn">Тип аргумента функции</typeparam>
        /// <typeparam name="TOut">ТИп значения функции</typeparam>
        /// <param name="f">Исходная (преобразуемая) функция</param>
        /// <param name="q">Метод преобразования исходной функции, тип значения которого соответствует типу исходной функции</param>
        /// <returns>Новая функция, преобразованная указанным методом на основе исходной функции</returns>
        [NotNull]
        public static Func<TIn, TOut> FuncFrom<TIn, TOut>
        (
            [NotNull] this Func<TIn, TOut> f,
            [NotNull] Func<Func<TIn, TOut>, Func<TIn, TOut>> q
        ) =>
            q(f);

        /// <summary>Функция от функции f(q(x))</summary>
        /// <param name="f">Внешняя функция</param>
        /// <param name="q">Внутренняя функция</param>
        /// <returns>Функция f от функции q</returns>
        [NotNull]
        public static Function FuncFor([NotNull] this Function f, [NotNull] Function q) => x => f(q(x));

        /// <summary>олучить автокорреляционную функцию от указанной функции</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="deltaX">Диапазон коореляции</param>
        /// <param name="x0">Смещение</param>
        /// <returns>Автокорреляционная функция</returns>
        [NotNull]
        public static Function GetAkf([NotNull] this Function f, double deltaX, double x0 = 0.0) => f.GetConvolution(f.ArgumentReverse(), deltaX, x0);

        /// <summary>Функция корреляции между двумя функциями на указанному интервале корреляции с указанным смещеинем</summary>
        /// <param name="f">Первая функция</param>
        /// <param name="g">Вторая функция</param>
        /// <param name="deltaX">Интервал корреляции</param>
        /// <param name="x0">Смещение</param>
        /// <returns>Функция корреляции двух исходных функций</returns>
        [NotNull]
        public static Function GetConvolution([NotNull] this Function f, [NotNull] Function g, double deltaX, double x0 = 0.0)
        {
            var min_x = x0 - deltaX / 2;
            var max_x = x0 + deltaX / 2;
            return x => ((Function)(t => f(t) * g(x - t))).GetIntegralValue_Adaptive(min_x, max_x);
        }

        #region Численное дифференцирование

        /// <summary>Функция численного дифференцирования исходной функции</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="dx">Дифференциальный участок</param>
        /// <param name="n">Номер метода численного дифференцирования в пределах [0,4]</param>
        /// <returns>Функция численного дифференциала от исходной функции</returns>
        [NotNull]
        public static Function GetDifferencial([NotNull] this Function f, double dx = 0.0001, int n = 0) => x => f.GetDifferencialValue(x, dx, n);

        /// <summary>Определение значения численного дифференциала в указанной точке, с указанным шагом дифференцирования и номером метода</summary>
        /// <param name="f">Дифференцируемая функция</param>
        /// <param name="x">Точка дифференцирования</param>
        /// <param name="dx">Шаг дифференцирования</param>
        /// <param name="n">Номер метода</param>
        /// <returns>Численное значение дифференциала функции в указанной точке</returns>
        public static double GetDifferencialValue([NotNull] this Function f, double x, double dx = 0.0001, int n = 0)
        {
            if (n == 0)
            {
                var Dx = dx / 2d;
                var min_x = x - Dx;
                var max_x = x + Dx;

                var Dy = f(max_x) - f(min_x);
                return !dx.Equals(0d)
                        ? Dy / dx
                        : (Dy > 0d
                            ? double.PositiveInfinity
                            : (Dy < 0d
                                ? double.NegativeInfinity
                                : double.NaN));
            }
            n--;
            var result = 0d;

            for (var i = 0; i < 6; i++) result += Solover.Differential.diff_a[n, i] * f(x + (i * dx));
            return result / Solover.Differential.diff_b[n] / dx;
        }

        #endregion

        #region Численное интегрирование

        /// <summary>Определённый численный интеграл функции (метод трапеций)</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начальное значение интервала интегрирования</param>
        /// <param name="x2">Конечное значение интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение результата численного интегрирования функции методом трапеций</returns>
        public static double GetIntegralValue([NotNull] this Function f, double x1, double x2, double f0 = 0, double dx = 0.0001)
        {
            if (x1.Equals(x2)) return f0;

            f0 += f(x1) / 2;
            x2 -= dx;
            while ((x1 += dx) < x2) f0 += f(x1);
            var v = f(x1 += dx);
            return ((v + (v = f(x2))) * (x2 - x1) + (2 * f0 + v) * dx) * .5;
        }

        /// <summary>Определённый численный интеграл функции (метод трапеций)</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начальное значение интервала интегрирования</param>
        /// <param name="x2">Конечное значение интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение результата численного интегрирования функции методом трапеций</returns>
        [NotNull]
        public static Task<double> GetIntegralValueAsync
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double f0 = 0,
            double dx = 0.0001
        ) =>
            x1.Equals(x2) ? Task.FromResult(f0) : Task.Factory.StartNew(() => f.GetIntegralValue(x1, x2, f0, dx));

        /// <summary>Двойной определённый численный интеграл функции (метод трапеций)</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начальное значение интервала интегрирования</param>
        /// <param name="x2">Конечное значение интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="f1">Начальное значение первой производной</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение результата двойного численного интегрирования функции методом трапеций</returns>
        public static double GetIntegral2Value
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double f1 = 0,
            double f0 = 0,
            double dx = 0.0001
        )
        {
            if (x1.Equals(x2)) return f0;

            var dx05 = dx * .5;
            var v = f(x1);
            x2 -= dx;
            while (x1 < x2) f0 += f1 + (f1 += (v + (v = f(x1 += dx))) * dx05);
            return f0 * dx05 + (f1 + (v + f(x2 += dx)) * (dx05 = .5 * (x2 - x1))) * dx05;
        }

        /// <summary>Определённый численный интеграл функции (метод трапеций)</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начальное значение интервала интегрирования</param>
        /// <param name="x2">КОнечное значение интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="f1">Начальное значение первой производной</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение результата численного интегрирования функции методом трапеций</returns>
        [NotNull]
        public static Task<double> GetIntegral2ValueAsync
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double f1 = 0,
            double f0 = 0,
            double dx = 0.0001
        ) =>
            x1.Equals(x2) ? Task.FromResult(f0) : Task.Factory.StartNew(() => f.GetIntegral2Value(x1, x2, f1, f0, dx));

        /// <summary>Интегрирование функции с модификацией ядра интеграла</summary>
        /// <param name="f">Подинтегральная функция f(x)</param>
        /// <param name="Core">Ядро интегрирования: Core(f(x),x)</param>
        /// <param name="x1">Начало интервала интегрирования</param>
        /// <param name="x2">Конец интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение определённого интеграла от ядра интегрирования методом тропеций</returns>
        public static double GetIntegralValue
        (
            [NotNull] this Function f,
            [NotNull] Func<double, double, double> Core,
            double x1,
            double x2,
            double f0 = 0,
            double dx = 0.0001
        )
        {
            if (x1.Equals(x2)) return f0;

            f0 += f(x1) / 2;
            x2 -= dx;
            while ((x1 += dx) < x2) f0 += Core(f(x1), x1);
            var v = f(x1 += dx);
            return ((v + (v = Core(f(x2), x2))) * (x2 - x1) + (2 * f0 + v) * dx) * .5;
        }

        /// <summary>Расчитать интеграл функции асинхронно</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="Core">Ядро интегрирования</param>
        /// <param name="x1">Начало интервала интегрирования</param>
        /// <param name="x2">Конец интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции (задача Коши)</param>
        /// <param name="dx">ШАг интегрирования</param>
        /// <returns>Задача расчёта значения интеграла функции</returns>
        [NotNull]
        public static Task<double> GetIntegralValueAsync
        (
            [NotNull] this Function f,
            [NotNull] Func<double, double, double> Core,
            double x1,
            double x2,
            double f0 = 0,
            double dx = 0.0001
        ) =>
            x1.Equals(x2) ? Task.FromResult(f0) : Task.Factory.StartNew(() => f.GetIntegralValue(Core, x1, x2, dx));

        /// <summary>Интегрирование функции с модификацией ядра интеграла</summary>
        /// <param name="f">Подинтегральная функция f(x)</param>
        /// <param name="Core">Ядро интегрирования: Core(f(x),x)</param>
        /// <param name="x1">Начало интервала интегрирования</param>
        /// <param name="x2">Конец интервала интегрирования</param>
        /// <param name="f0">Начальное значение функции</param>
        /// <param name="f1">Начальное значение первой производной</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Значение определённого интеграла от ядра интегрирования методом тропеций</returns>
        public static double GetIntegral2Value
        (
            [NotNull] this Function f,
            [NotNull] Func<double, double, double> Core,
            double x1,
            double x2,
            double f1 = 0,
            double f0 = 0,
            double dx = 0.0001
        )
        {
            if (x1.Equals(x2)) return f0;

            var dx05 = dx * .5;
            var v = Core(f(x1), x1);
            x2 -= dx;
            while (x1 < x2) f0 += f1 + (f1 += (v + (v = f(x1 += dx))) * dx05);
            return f0 * dx05 + (f1 + (v + Core(f(x2 += dx), x2)) * (dx05 = .5 * (x2 - x1))) * dx05;
        }

        /// <summary>Асинхронный расчёт двойного интеграла функции</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="Core">Ядро интегрирования</param>
        /// <param name="x1">Начало интервала интегрирования</param>
        /// <param name="x2">Конец интервала интегрирования</param>
        /// <param name="f1">Начальное значение первой производной (задача Коши)</param>
        /// <param name="f0">Начальное значение функции (задача Коши)</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Задача численного расчёта второго интеграла функции</returns>
        [NotNull]
        public static Task<double> GetIntegral2ValueAsync
        (
            [NotNull] this Function f,
            [NotNull] Func<double, double, double> Core,
            double x1,
            double x2,
            double f1 = 0,
            double f0 = 0,
            double dx = 0.0001
        ) =>
            x1.Equals(x2) ? Task.FromResult(f0) : Task.Factory.StartNew(() => f.GetIntegral2Value(Core, x1, x2, dx));

        /// <summary>Численный расчёт определённого интеграла методом симпсона</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Нижний предел интегрирования</param>
        /// <param name="x2">Верхний предел интегрирования</param>
        /// <param name="N">Число интервалов интегрирования N > 2</param>
        /// <returns>Интеграл функции на отрезке метдом Симпсона</returns>
        public static double GetIntegralValue_Simpson([NotNull] this Function f, double x1, double x2, int N = 100)
        {
            if (x1.Equals(x2)) return 0;

            var dx = Math.Abs(x2 - x1) / N;
            var dx05 = dx / 2;
            var x = x1;
            var s = .0;
            var s_dx05 = f(x + dx05);
            for (var i = 1; i < N; i++)
            {
                s += f(x += dx);
                s_dx05 += f(x + dx05);
            }
            return (f(x1) + 2 * s + 4 * s_dx05 + f(x2)) * dx / 6;
        }

        /// <summary>Численный расчёт определённого интеграла методом симпсона</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Нижний предел интегрирования</param>
        /// <param name="x2">Верхний предел интегрирования</param>
        /// <param name="f0">Начально значение функции</param>
        /// <param name="dx">Шаг интегрирования</param>
        /// <returns>Интеграл функции на отрезке метдом Симпсона</returns>
        public static double GetIntegralValue_Simpson
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double f0,
            double dx
        )
        {
            if (x1.Equals(x2)) return 0;

            var dx05 = 0.5 * dx;
            var x = x1;
            var s = .0;
            var s_dx05 = f(x + dx05);
            while ((x += dx) < x2)
            {
                s += f(x);
                s_dx05 += f(x + dx05);
            }
            return (f(x1) + 2 * s + 4 * s_dx05 + f(x2)) * dx / 6;
        }

        /// <summary>Численный расчёт определённого интеграла методом симпсона</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Нижний предел интегрирования</param>
        /// <param name="x2">Верхний предел интегрирования</param>
        /// <param name="N">Число интервало интегрирования N > 2</param>
        /// <returns>Интеграл функции на отрезке метдом Симпсона</returns>
        [NotNull]
        public static Task<double> GetIntegralValue_SimpsonAsync([NotNull] this Function f, double x1, double x2, int N = 100) => x1.Equals(x2) ? Task.FromResult(0d) : Task.Factory.StartNew(() => f.GetIntegralValue_Simpson(x1, x2, N));

        /// <summary>Численный расчёт определённого интеграла методом адаптивного разбиения</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Нижний предел интегрирования</param>
        /// <param name="x2">Верхний предел интегрирования</param>
        /// <param name="N">Начальное разбиение отрезка (по умолчанию 2 точки)</param>
        /// <param name="eps">Точность вычисления интеграла</param>
        /// <returns>Адаптивный интеграл функции</returns>
        /// <remarks>
        /// Функция рекурентно на каждом этапе расчитывает два численных интеграла (методом Симпсона): для указанного числа точек и для удвоенного.
        /// Если разница между расчитанными интегралами меньше указанной точности, то возвращается значение интеграла для удвоенного числа точек
        /// Иначе рекурентно расчитывается сумма двух интегралов (адаптивным методом) для правой и левой половины интервала интегрирования с удвоенным
        /// числом точек для каждого из них. Для каждой половины рекурентно повторяется проделанная процедура 
        /// </remarks>
        public static double GetIntegralValue_Adaptive([NotNull] this Function f, double x1, double x2, int N = 2, double eps = 1e-6)
        {
            if (x1.Equals(x2)) return 0;

            var I1 = f.GetIntegralValue_Simpson(x1, x2, N);
            var I2 = f.GetIntegralValue_Simpson(x1, x2, N <<= 1);
            return Math.Abs(I1 - I2) < eps
                               ? I2
                               : f.GetIntegralValue_Adaptive(x1, .5 * (x1 + x2), N, eps)
                                 + f.GetIntegralValue_Adaptive(.5 * (x1 + x2), x2, N, eps);
        }

        /// <summary>Численный расчёт определённого интеграла методом адаптивного разбиения</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Нижний предел интегрирования</param>
        /// <param name="x2">Верхний предел интегрирования</param>
        /// <param name="N">Начальное разбиение отрезка</param>
        /// <param name="eps">Точность вычисления интеграла</param>
        /// <returns>Адаптивный интеграл функции</returns>
        [NotNull]
        public static async Task<double> GetIntegralValue_AdaptiveAsync
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            int N = 2,
            double eps = 0.000001
        )
        {
            if (x1.Equals(x2)) return 0;

            var I1 = await f.GetIntegralValue_SimpsonAsync(x1, x2, N).ConfigureAwait(false);
            var I2 = await f.GetIntegralValue_SimpsonAsync(x1, x2, N <<= 1).ConfigureAwait(false);

            if (Math.Abs(I1 - I2) < eps) return I2;
            var t1 = f.GetIntegralValue_AdaptiveAsync(x1, .5 * (x1 + x2), N, eps);
            var t2 = f.GetIntegralValue_AdaptiveAsync(.5 * (x1 + x2), x2, N, eps);
            return (await Task.WhenAll(t1, t2).ConfigureAwait(false)).Sum();
        }

        //public static double GetIntegralValue_InfiniteIntervals(this Function f, double x1, double x2, int N = 2, double eps = 0.000001)
        //{
        //    if(x2 < x1)
        //        return -f.GetIntegralValue_InfiniteIntervals(x2, x1, N, eps);

        //    var x1_inf = double.IsNegativeInfinity(x1);
        //    var x2_inf = double.IsPositiveInfinity(x2);

        //    if(x1_inf && x2_inf)
        //    {
        //        Func<double, double> F = t =>
        //            {
        //                var t2 = t * t;
        //                var T = 1 - t2;
        //                return f(t / T) * (1 + t2) / T / T;
        //            };
        //        return F.GetIntegralValue_Adaptive(0, 1, N, eps);
        //    }
        //    if(x1_inf)
        //    {
        //        Func<double, double> F = t =>
        //            {
        //                var t2 = t * t;
        //                return f(x2 - (1 - t) / t) / t2;
        //            };
        //        return F.GetIntegralValue_Adaptive(0, 1, N, eps);
        //    }
        //    if(x2_inf)
        //    {
        //        Func<double, double> F = t =>
        //            {
        //                var T = 1 - t;
        //                return f(x1 + t / T) / T / T;
        //            };
        //        return F.GetIntegralValue_Adaptive(0, 1, N, eps);
        //    }
        //    return f.GetIntegralValue_Adaptive(x1, x2, N, eps);
        //}

        /// <summary>Значение интеграла функции методом сплайнов</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начало отрезка интегрирования</param>
        /// <param name="x2">Конец отрезка интегрирования</param>
        /// <param name="N">Разбиение интервала интегрирования</param>
        /// <returns>Значение интеграла функции</returns>
        [Copyright("MachineLearning.ru", url = "http://www.machinelearning.ru/wiki/index.php?title=Применение_сплайнов_для_численного_интегрирования")]
        public static double GetIntegralValue_Spline([NotNull] this Function f, double x1, double x2, int N = 1000)
        {
            if (x1.Equals(x2)) return 0;

            var N1 = N - 1;
            var dx = (x2 - x1) / N;
            var _data = new double[N + 1];
            double x;
            int i;

            for (i = 0; i <= N; i++)
            {
                x = x1 + dx * i;
                _data[i] = f(x);
            }

            var alpha = new double[N1];
            alpha[0] = -1d / 4;
            var beta = new double[N1];
            beta[0] = _data[2] - 2 * _data[1] + _data[0];
            //метод прогонки, прямой ход
            for (i = 1; i < N1; i++)
            {
                alpha[i] = -1 / (alpha[i - 1] + 4);
                beta[i] = (_data[i + 2] - 2 * _data[i + 1] + _data[i] - beta[i - 1]) / (alpha[i - 1] + 4);
            }

            var c = new double[N1];
            c[N1 - 1] = (_data[N] - 2 * _data[N - 1] + _data[N - 2] - beta[N1 - 1]) / (4 + alpha[N1 - 1]);
            //обратный ход
            for (i = N1 - 2; i >= 0; i--)
                c[i] = alpha[i + 1] * c[i + 1] + beta[i + 1];

            for (i = 0; i < N1; i++) c[i] = c[i] * 3 / (dx * dx);

            //считаем приближенное значение интеграла по формуле (9):
            x = (5 * _data[0] + 13 * _data[1] + 13 * _data[N - 1] + 5 * _data[N]) / 12;
            var tmp = 0.0;

            for (i = 2; i < N - 1; i++) tmp += _data[i];
            x = (x + tmp) * dx - (c[0] + c[N1 - 1]) * dx * dx * dx / 36;
            return x;
        }

        /// <summary>Значение интеграла функции методом сплайнов</summary>
        /// <param name="f">Интегрируемая функция</param>
        /// <param name="x1">Начало отрезка интегрирования</param>
        /// <param name="x2">Конец отрезка интегрирования</param>
        /// <param name="N">Разбиение интервала интегрирования</param>
        /// <returns>Значение интеграла функции</returns>
        [NotNull]
        public static Task<double> GetIntegralValue_SplineAsync([NotNull] this Function f, double x1, double x2, int N = 1000) => x1.Equals(x2) ? Task.FromResult(0d) : Task.Factory.StartNew(() => f.GetIntegralValue_Spline(x1, x2, N));

        /// <summary>Получить функцию-интеграл от функции</summary>
        /// <param name="f">Подинтегральная функция</param>
        /// <param name="x0">Начальное положение интегрирования</param>
        /// <param name="C">Константа интегрирования</param>
        /// <param name="eps">Точность интегрирования</param>
        /// <returns>Функция-интеграл от исходной функции</returns>
        [NotNull]
        public static Function GetIntegral([NotNull] this Function f, double x0 = 0, double C = 0, double eps = 1e-6) => new Integrator(f, x0, C).GetIntegral(eps);

        #endregion 

        /// <summary>Создать периодическую функцию на основе исходной</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="T">Период</param>
        /// <param name="x0">Смещение</param>
        /// <returns>Периодическая функция</returns>
        [NotNull]
        public static Function GetPeriodic([NotNull] this Function f, double T, double x0 = 0.0)
        {
            if (!x0.Equals(0.0)) f = f.ArgumentShift(x0);
            return x => f(x % T + (x < 0 ? T : 0));
        }

        /// <summary>Определить мощность функции</summary>
        /// <param name="f">Исследуемая функция</param>
        /// <param name="x1">Начало интервала</param>
        /// <param name="x2">Конец интервала</param>
        /// <returns>Значение интеграла от квадрата функции</returns>
        public static double GetPower([NotNull] this Function f, double x1, double x2)
        {
            if (x1.Equals(x2)) return 0;
            return ((Function)(x =>
           {
               var num = f(x);
               return num * num;
           })).GetIntegralValue_Adaptive(x1, x2) / (x2 - x1);
        }

        /// <summary>Дискретизация функции</summary>
        /// <typeparam name="T">Тип значения функции</typeparam>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="dx">ШАг дискретизации</param>
        /// <returns>Перечисление дискретных значений функции</returns>
        [NotNull]
        public static IEnumerable<T> Sampling<T>([NotNull] this Func<double, T> f, double x1, double x2, double dx)
        {
            var x = x1;
            do { yield return f(x += dx); } while (x <= x2);
        }

        [NotNull]
        public static T[] Sampling<T>([NotNull] this Func<double, T> f, double x1, double dx, int SamplesCount)
        {
            if (f is null) throw new ArgumentNullException(nameof(f));
            if (SamplesCount < 0) throw new ArgumentOutOfRangeException(nameof(SamplesCount), "Число отсчётов должно быть больше 0");
            if (dx.Equals(0d)) throw new ArgumentOutOfRangeException(nameof(dx), "Шаг дискретизации не должен быть равен 0");

            var result = new T[SamplesCount];
            for (var i = 0; i < SamplesCount; i++)
                result[i] = f(x1 + dx * i);
            return result;
        }

        /// <summary>Определение значений функции в дискретном перечне значений аргумента</summary>
        /// <typeparam name="TArgument">Тип аргумента</typeparam>
        /// <typeparam name="TResult">Тип значения функции</typeparam>
        /// <param name="f">ДИскретизируемая функция</param>
        /// <param name="args">Массив аргументов функции</param>
        /// <returns>Массив значений функции для указанных значений аргументов</returns>
        [NotNull]
        public static TResult[] GetValues<TArgument, TResult>([NotNull] this Func<TArgument, TResult> f,[NotNull] params TArgument[] args) => args.Function(f);

        /// <summary>Получить массив значений функции на указанном интервале с указанным шагом дискретизации</summary>
        /// <typeparam name="TResult">ТИп значений функции</typeparam>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="dx">Шаг дискретизации</param>
        /// <returns>Массив значений функции</returns>
        [NotNull]
        public static TResult[] GetValues<TResult>([NotNull] this Func<double, TResult> f, double x1, double x2, double dx)
        {
            if (x1.Equals(x2)) return new[] { f(x1) };
            if (Math.Abs(x2 - x1) < dx) return new[] { f(x1), f(x2) };

            var N = (int)((x2 - x1) / dx);
            var result = new List<TResult>(N);

            for (var i = 0; i < N; i++) result.Add(f(x1 + i * dx));

            return result.ToArray();
        }

        /// <summary>Получить функцию, значения которой обратны к значениям исходрной функции g(x) = 1 / f(x)</summary>
        /// <param name="f">Исходная функция</param>
        /// <returns>Функция, значения которой обратны по отношению исходной функции</returns>
        [NotNull]
        public static Function Inverse([NotNull] this Function f) => x => 1 / f(x);

        /// <summary>Произведение двух функций g(x) = f1(x) * f2(x)</summary>
        /// <param name="f1">Фунция - первый сомножитель</param>
        /// <param name="f2">Функция - второй сомножитель</param>
        /// <returns>Функция - произведение двух функций</returns>
        [NotNull]
        public static Function Multiply([NotNull] this Function f1, [NotNull] Function f2) => x => f1(x) * f2(x);

        /// <summary>Произведение функции на число g(x) = f(x) * a</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="a">Число</param>
        /// <returns>Функция, значения которой равны произведению значений исходной функции на указанное число</returns>
        [NotNull]
        public static Function Multiply([NotNull] this Function f, double a) => a.Equals(1d) ? f : x => f(x) * a;

        /// <summary>Возведение функции в вещественную степень</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="a">Вещественная степень</param>
        /// <returns>Функция, значения которой равны возведению в указанную степень значений исходной функции</returns>
        [NotNull]
        public static Function Power([NotNull] this Function f, double a) => a.Equals(1d) ? f : (a.Equals(0d) ? (x => 1) : (Function)(x => Math.Pow(f(x), a)));

        /// <summary>Получение отрицательной функции</summary>
        /// <param name="f">Исходная функция</param>
        /// <returns>Функция, значения которой обратны по знаку к исходной функци</returns>
        [NotNull]
        public static Function Reverse([NotNull] this Function f) => x => -f(x);

        /// <summary>Установка значения параметра функции двух переменных</summary>
        /// <param name="f">Исходная функция двух переменных</param>
        /// <param name="a">Устанавливаемое значение параметра</param>
        /// <param name="IsFirst">Параметром является первый аргумент функции? (по умолчанию - нет)</param>
        /// <returns>Функция одного переменного, получанная на основе исходной функции двух переменных устновкой одного в значение указанного параметра</returns>
        [NotNull]
        public static Function SetParameter([NotNull] this Func<double, double, double> f, double a, bool IsFirst = false) => IsFirst ? (Function)(x => f(a, x)) : x => f(x, a);

        /// <summary>Вычитание одной функции из другой g(x) = f1(x) - f2(x)</summary>
        /// <param name="f1">Функция - уменьшаемое</param>
        /// <param name="f2">Функция - вычитаемое</param>
        /// <returns>Функция, значения которой численно равны разности значений двух исходных функций</returns>
        [NotNull]
        public static Function Substract([NotNull] this Function f1, [NotNull] Function f2) => x => f1(x) - f2(x);

        /// <summary>Вычитание из вункции числа g(x) = f(x) - a</summary>
        /// <param name="f">Исходная функция</param>
        /// <param name="a">вычитаемое из функции число</param>
        /// <returns>Функция, значения которой численно равны разности значений исходной функции и указанного числа</returns>
        [NotNull]
        public static Function Substract([NotNull] this Function f, double a) => a.Equals(0d) ? f : x => f(x) - a;

        ///<summary>Вычислить значения функции параллельно</summary>
        ///<param name="f">Вычисляемая функция</param>
        ///<param name="Arguments">Область определения</param>
        ///<typeparam name="TArgument">Тип аргумента</typeparam>
        ///<typeparam name="TResult">Тип значения</typeparam>
        ///<returns>Массив значений функции</returns>
        [NotNull]
        public static TResult[] GetValuesParralel<TArgument, TResult>(
            [NotNull] this Func<TArgument, TResult> f,
            [NotNull] IEnumerable<TArgument> Arguments)
            => Arguments.Select(X => X.Async(f, (x, ff) => ff(x))).WhenAll().Result;

        /// <summary>Интегратор функции</summary>
        public sealed class Integrator
        {
            /// <summary>Объект синхронизации потоков при доступе к параметрам интегратора</summary>
            [NotNull]
            private readonly object _LockObject = new object();
            /// <summary>Константа интегрирования</summary>
            public double C { get; private set; }
            /// <summary>Начальное положение интегратора</summary>
            public double x0 { get; private set; }

            /// <summary>Интегрируемая функция</summary>
            [NotNull]
            public Function f { get; }

            /// <summary>Инициализация нового интегратора функции</summary>
            /// <param name="f">Интегрируемая функция</param>
            /// <param name="x0">Начальное положение интегратора</param>
            /// <param name="C">Константа интегрирования</param>
            public Integrator([NotNull] Function f, double x0, double C = 0)
            {
                this.f = f;
                this.x0 = x0;
                this.C = C;
            }

            /// <summary>Метод расчёта интеграла от предыдущего положения интегратора до указанного</summary>
            /// <param name="x">Требуемое значение конца интервала интегрирования</param>
            /// <param name="eps">Точность процесса интегрирования</param>
            /// <returns>Значение численного интеграла на интервале от предыдущего положения интегратора до указанного</returns>
            public double GetValue(double x, double eps = 1e-6)
            {
                lock (_LockObject)
                {
                    if (x.Equals(x0)) return C;
                    var result = C += x > x0
                        ? f.GetIntegralValue_Adaptive(x0, x, eps: eps)
                        : -f.GetIntegralValue_Adaptive(x, x0, eps: eps);
                    x0 = x;
                    return result;
                }
            }

            /// <summary>Получить функцию, равную интегралу от интегрируемой функции</summary>
            /// <param name="eps">Точность интегрирования</param>
            /// <returns>Интеграл функци</returns>
            [NotNull]
            public Function GetIntegral(double eps = 1e-6) => x => GetValue(x, eps);
        }

        /// <summary>Делегат функции сложения двух векторов</summary>
        /// <param name="C">Вектор результата сложения</param>
        /// <param name="B">Вектор первого слогаемого</param>
        /// <param name="A">Вектор второго слогаемого</param>
        /// <param name="length">Длина векторов</param>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VectorAddDelegate(float[] C, float[] B, float[] A, int length);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, IntPtr flAllocationType, IntPtr flProtect);

        private static VectorAddDelegate __VectorAddDelegate;
        /// <summary>Метод генерации быстрого сумматора двух векторов вещественных чисел</summary>
        /// <returns>Метод сложения двух векторов вещественных чисел</returns>
        [Copyright("Оптимизация .NET приложения", url = "http://professorweb.ru/my/csharp/optimization/level7/7_8.php")]
        [NotNull]
        public static VectorAddDelegate CreateFastFloatSummator()
        {
            if (__VectorAddDelegate != null) return __VectorAddDelegate;

            // Следующий массив байтов был получен с помощью ассемблера
            // с поддержкой SSE - это законченная функция, принимающая 
            // четыре параметра (три вектора и длину) и складывающая их
            byte[] sseAssemblyBytes =
                {
                    0x8b, 0x5c, 0x24, 0x10, 0x8b, 0x74,
                    0x24, 0x0c, 0x8b, 0x7c, 0x24, 0x08,
                    0x8b, 0x4c, 0x24, 0x04, 0x31, 0xd2,
                    0x0f, 0x10, 0x0c, 0x97, 0x0f, 0x10,
                    0x04, 0x96, 0x0f, 0x58, 0xc8, 0x0f,
                    0x11, 0x0c, 0x91, 0x83, 0xc2, 0x04,
                    0x39, 0xda, 0x7f, 0xea, 0xc2, 0x10,
                    0x00
                };

            var codeBuffer = VirtualAlloc
            (
                IntPtr.Zero,
                new UIntPtr((uint)sseAssemblyBytes.Length),
                new IntPtr(0x1000 | 0x2000), // MEM_COMMIT | MEM_RESERVE
                new IntPtr(0x40)             // EXECUTE_READWRITE
            );
            Marshal.Copy(sseAssemblyBytes, 0, codeBuffer, sseAssemblyBytes.Length);
            return __VectorAddDelegate = (VectorAddDelegate)Marshal.GetDelegateForFunctionPointer(codeBuffer, typeof(VectorAddDelegate));
        }

        #region Адаптивная дискретизация функци

        /// <summary>Результат дескритезации функции</summary>
        /// <typeparam name="TValue">Тип значений функции</typeparam>
        public class SamplingResult<TValue> : IEnumerable<SamplingResult<TValue>.Result>
        {
            /// <summary>Отсчёт функции</summary>
            public struct Result
            {
                /// <summary>Значение аргумента отсчёта</summary>
                public readonly double Argument;
                /// <summary>Значение функции</summary>
                public readonly TValue Value;

                /// <summary>Инициализация нового отсчёта функции</summary>
                /// <param name="Argument">Значение аргумента функции</param>
                /// <param name="Value">Значение функции</param>
                public Result(double Argument, TValue Value)
                {
                    this.Argument = Argument;
                    this.Value = Value;
                }

                /// <summary>Оператор неявного приведения отсчёта функции к картежу двух элементов - значение отсчёта функции - значение функции</summary>
                /// <param name="result">Отсчёт функции</param>
                [NotNull]
                public static implicit operator Tuple<double, TValue>(Result result) =>
                    new Tuple<double, TValue>(result.Argument, result.Value);
            }

            /// <summary>Перечисление отсчётов функции</summary>
            public IEnumerable<Result> Values { get; protected set; }

            /// <summary>Оценка точности дискретизации</summary>
            public double Accuracy { get; protected set; }

            /// <summary>Инициализация нового результата дискретизации функции</summary>
            /// <param name="Values">Перечисление отсчётов функции</param>
            /// <param name="Accuracy">Оценка точности дискретизации</param>
            public SamplingResult(IEnumerable<Result> Values, double Accuracy)
            {
                this.Values = Values;
                this.Accuracy = Accuracy;
            }

            IEnumerator<Result> IEnumerable<Result>.GetEnumerator() => Values.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
        }

        #region Адаптивный метод

        /// <summary>Результат дискретизации вещественной функции</summary>
        public class SimpleSamplingResult : SamplingResult<double>
        {
            /// <summary>Метод дискретизации</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="dx">Шаг дискретизации</param>
            /// <returns>Кортеж, содержащий связанный список дискретов функции и оценку точности дискретизации</returns>
            [NotNull]
            private static Tuple<LinkedList<Result>, double> Sampling
            (
                [NotNull] Function f,
                double x1,
                double x2,
                double dx
            )
            {
                var result = new LinkedList<Result>();
                var accuracy = 0d;
                var x = x1;
                var y = f(x);
                var node = result.AddFirst(new Result(x, y));

                do
                {
                    x += dx;
                    var dy = -y + (y = f(x));

                    var l = Math.Sqrt(dx * dx + dy * dy);
                    l -= dx;
                    accuracy += l * l;
                    node = result.AddAfter(node, new Result(x, y));
                } while (Math.Abs(x2 - x) / dx > 0.25);
                if (!result.Last.Value.Argument.Equals(x2))
                    result.Last.Value = new Result(x2, f(x2));
                accuracy = Math.Sqrt(accuracy);
                return new Tuple<LinkedList<Result>, double>(result, accuracy);
            }

            /// <summary>Дискретизируемая функция</summary>
            [NotNull]
            private readonly Function _F;

            /// <summary>Связанный список отсчётов функции</summary>
            private readonly LinkedList<Result> _List;

            /// <summary>Инициализация нового результата дискретизации по кортежу дискретов функции и оценки точности дискретизации</summary>
            /// <param name="SamplingResult">Кортеж дискретов функции и оценки точности дскретизации</param>
            private SimpleSamplingResult([NotNull] Tuple<LinkedList<Result>, double> SamplingResult)
                : base(SamplingResult.Item1.ToArray(), SamplingResult.Item2) => _List = SamplingResult.Item1;

            /// <summary>Инициализация нового результата дискретизации</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Окончание интервала дискретизации</param>
            /// <param name="dx">Шаг дискретизации</param>
            public SimpleSamplingResult([NotNull] Function f, double x1, double x2, double dx)
                : this(Sampling(f, x1, x2, dx))
            {
                // ReSharper disable once JoinNullCheckWithUsage
                if (f is null) throw new ArgumentNullException(nameof(f));
                if (dx <= 0) throw new ArgumentOutOfRangeException(nameof(dx), $"Error: {nameof(dx)} <= 0");

                _F = f;
            }

            /// <summary>Уточнение результата дискретизации</summary>
            /// <param name="accuracy">Требуемая точность</param>
            /// <returns>Истина, если требуемая точность достигнута</returns>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public double ClarifySampling(double accuracy)
            {
                if (accuracy <= 0)
                    throw new ArgumentOutOfRangeException(nameof(accuracy), $"Error: {nameof(accuracy)} <= 0");

                lock (_List)
                {
                    var current = _List.First;
                    var result = 0d;
                    var next = current.Next;

                    if (next is null) return 0d;
                    do
                    {
                        var current_value = current.Value;
                        var next_value = next.Value;
                        var x0 = current_value.Argument;
                        var y0 = current_value.Value;
                        var y1 = next_value.Value;
                        var x1 = next_value.Argument;
                        var dx = x1 - x0;
                        var dy = y1 - y0;
                        var l = Math.Sqrt(dx * dx + dy * dy);

                        if (l >= accuracy)
                        {
                            var x11 = x0 + (x1 - x0) * accuracy / l;

                            var y11 = _F(x11);
                            dx = x11 - x0;
                            dy = y11 - y0;
                            l = Math.Sqrt(dx * dx + dy * dy);
                            l = accuracy - l;
                            result += l * l;
                            current.AddAfter(new Result(x11, y11));
                        }
                        else if (next.Next != null)
                            _List.Remove(next);
                    } while ((current = next).Next != null);
                    Values = GetValues();
                    return Accuracy = Math.Sqrt(result);
                }
            }

            /// <summary>Метод получения отсчётов функции</summary>
            /// <returns>Массив отсчётов функции</returns>
            [NotNull]
            public Result[] GetValues() { lock (_List) return _List.OrderBy(v => v.Argument).ToArray(); }

            /// <summary>Оператор неявного приведения типа результата дискретизации к типу массива отсчётов функции</summary>
            /// <param name="result">Результат дискретизации</param>
            [NotNull]
            public static implicit operator Result[]([NotNull] SimpleSamplingResult result) => result.GetValues();
        }

        /// <summary>Адаптивный метод дискретизации вещественной функции</summary>
        /// <param name="f">Дискретизируемая вещественная функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Окончание интервала дискретизации</param>
        /// <param name="eps">Точность дискретизации</param>
        /// <returns>Результат дискретизации вещественной функции</returns>
        [NotNull]
        public static SimpleSamplingResult SamplingAdaptive([NotNull] this Function f, double x1, double x2, double eps)
            => new SimpleSamplingResult(f, Math.Min(x1, x2), Math.Max(x1, x2), eps);

        /// <summary>Адаптивный метод дискретизации функции</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="converter">Конвертер преобразования значения функции в вещественное число для оценки точности дискретизации</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Окончание интервала дискретизации</param>
        /// <param name="eps">Точность дискретизации</param>
        /// <returns>Результат дискретизации функции</returns>
        [NotNull]
        public static AdaptiveSamplingResult<T> SamplingAdaptive<T>
        (
            [NotNull] this Func<double, T> f,
            [NotNull] Func<T, double> converter,
            double x1,
            double x2,
            double eps
        ) => new AdaptiveSamplingResult<T>(f, converter, x1, x2, eps);

        /// <summary>Результат адаптивной дискретизации</summary>
        /// <typeparam name="T">Тип значения дискретизируемой функци</typeparam>
        public class AdaptiveSamplingResult<T> : SamplingResult<T>
        {
            /// <summary>Метод дискретизации функции</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="converter">Метод преобразования значений функции в вещественное число для оценки точности дискретизации</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="dx">НАчальный шаг дискретизации</param>
            /// <returns>Кортеж со списком значений функции и вещественным число, оценивающим точность дискретизации</returns>
            [NotNull]
            private static Tuple<LinkedList<Result>, double> Sampling
            (
                [NotNull] Func<double, T> f,
                [NotNull] Func<T, double> converter,
                double x1,
                double x2,
                double dx
            )
            {
                var result = new LinkedList<Result>();
                var x = x1;
                var v = f(x);
                var y = converter(v);
                var node = result.AddFirst(new Result(x, v));
                var accuracy = 0d;

                do
                {
                    x += dx;
                    v = f(x);
                    var dy = -y + (y = converter(v));

                    var l = Math.Sqrt(dx * dx + dy * dy);
                    l -= dx;
                    accuracy += l * l;
                    node = result.AddAfter(node, new Result(x, v));
                } while (Math.Abs(x2 - x) / dx > 0.25);
                if (!result.Last.Value.Argument.Equals(x2))
                    result.Last.Value = new Result(x2, f(x2));
                return new Tuple<LinkedList<Result>, double>(result, Math.Sqrt(accuracy));
            }

            /// <summary>СВязанный список с дискретами функции</summary>
            private readonly LinkedList<Result> _List;

            /// <summary>Дискретизируемая функция</summary>
            [NotNull]
            private readonly Func<double, T> _F;

            /// <summary>Метод преобразования значений функции в вещественное число для оценки качества дискретизации</summary>
            [NotNull]
            private readonly Func<T, double> _Converter;

            /// <summary>Инициализация нового адаптивного дискретизацтора</summary>
            /// <param name="SamplingResult">Список отсчётов дискретизации функции и оценка точности дискретизации</param>
            private AdaptiveSamplingResult([NotNull] Tuple<LinkedList<Result>, double> SamplingResult)
                : base(SamplingResult.Item1.ToArray(), SamplingResult.Item2) => _List = SamplingResult.Item1;

            /// <summary>Инициализация нового адаптивного дискретизацтора</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="converter">Метод преобразования значений функции в вещественное число для оценки качества дискретизации</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="dx">Начальный шаг дискретизации</param>
            public AdaptiveSamplingResult
            (
                [NotNull] Func<double, T> f,
                [NotNull] Func<T, double> converter,
                double x1,
                double x2,
                double dx
            ) : this(Sampling(f, converter, Math.Min(x1, x2), Math.Max(x1, x2), dx))
            {// ReSharper disable once JoinNullCheckWithUsage
                if (f is null) throw new ArgumentNullException(nameof(f));
                // ReSharper disable once JoinNullCheckWithUsage
                if (converter is null) throw new ArgumentNullException(nameof(converter));
                if (dx <= 0) throw new ArgumentOutOfRangeException(nameof(dx), $"Error: {nameof(dx)} <= 0");

                _F = f;
                _Converter = converter;
            }

            /// <summary>Точная дискретизации</summary>
            /// <param name="accuracy">Требуемая точность</param>
            /// <returns>Полученная точность дискретизации</returns>
            [MethodImpl(MethodImplOptions.Synchronized)]
            public double ClarifySampling(double accuracy)
            {
                if (accuracy <= 0)
                    throw new ArgumentOutOfRangeException(nameof(accuracy), $"Error: {nameof(accuracy)} <= 0");

                lock (_List)
                {
                    var current = _List.First;
                    var result = 0d;
                    var next = current.Next;

                    if (next is null) return 0d;
                    do
                    {
                        var current_value = current.Value;
                        var next_value = next.Value;
                        var x0 = current_value.Argument;
                        var v0 = current_value.Value;
                        var y0 = _Converter(v0);
                        var v1 = next_value.Value;
                        var y1 = _Converter(v1);
                        var x1 = next_value.Argument;
                        var dx = x1 - x0;
                        var dy = y1 - y0;
                        var l = Math.Sqrt(dx * dx + dy * dy);

                        if (l >= accuracy)
                        {
                            var x11 = x0 + (x1 - x0) * accuracy / l;
                            var v11 = _F(x11);
                            var y11 = _Converter(v11);
                            dx = x11 - x0;
                            dy = y11 - y0;
                            l = Math.Sqrt(dx * dx + dy * dy);
                            l = accuracy - l;
                            result += l * l;
                            current.AddAfter(new Result(x11, v11));
                        }
                        else if (next.Next != null)
                            _List.Remove(next);
                    } while ((current = next).Next != null);

                    return Accuracy = Math.Sqrt(result);
                }
            }

            /// <summary>Получить отсчёты функции в виде массива значений</summary>
            /// <returns>Массив значений функции</returns>
            [NotNull]
            public Result[] GetValues() { lock (_List) return _List.OrderBy(v => v.Argument).ToArray(); }

            /// <summary>Оператор неявного преобразования результатов адаптивной дискретизации функции в массив её значений</summary>
            /// <param name="result">Результаты адаптивной дискретизации функции</param>
            [NotNull]
            public static implicit operator Result[]([NotNull] AdaptiveSamplingResult<T> result) => result.GetValues();
        }

        #endregion

        #region Однопроходный метод

        /// <summary>Результаты дискретизации функции адаптивным однопроходным методом</summary>
        private sealed class SamplingResultOneWay : SamplingResult<double>
        {
            /// <summary>Инициализация нового результата адаптивной дискретизации однопроходным методом</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="eps">Требуемая точность дискретизации</param>
            /// <param name="dx">Начальный шаг дискретизации</param>
            public SamplingResultOneWay
            (
                [NotNull] Func<double, double> f,
                double x1,
                double x2,
                double eps,
                double dx
            ) : base(null, 0) =>
                Values = f.SamplingAdaptive_OneWay(x1, x2, eps, dx, a => Accuracy = a);
        }

        /// <summary>Однопроходный адаптивный метод дискретизации функции</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность дискретизации</param>
        /// <param name="dx">Начальный шаг дискретизации</param>
        /// <returns>Результат дискретизации</returns>
        [NotNull]
        public static SamplingResult<double> SamplingAdaptive_OneWay
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double eps,
            double dx = 0
        ) =>
            new SamplingResultOneWay(f, Math.Min(x1, x2), Math.Max(x1, x2), eps, dx <= 0 ? eps : dx);

        /// <summary>Метод однопроходной адаптивной дискретизации функции</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность дискретизации</param>
        /// <param name="dx">Начальный шаг дискретизации</param>
        /// <param name="UpdateAccuracy">Метод обновления значения точности дискретизации в процесс самой дискретизации</param>
        /// <returns>Перечисление результатов дискретизации</returns>
        private static IEnumerable<SamplingResult<double>.Result> SamplingAdaptive_OneWay
        (
            [NotNull] this Func<double, double> f,
            double x1,
            double x2,
            double eps,
            double dx,
            Action<double> UpdateAccuracy
        )
        {
            var x = x1;
            var y = f(x);
            yield return new SamplingResult<double>.Result(x, y);
            var accuracy = 0d;

            do
            {
                x += dx;
                var dy = -y + (y = f(x));
                yield return new SamplingResult<double>.Result(x, y);
                var l = Math.Sqrt(dx * dx + dy * dy);
                dx *= eps / l;
                var dl = l - eps;
                dl *= dl;
                UpdateAccuracy(Math.Sqrt(accuracy += dl));
            } while (x + dx < x2);
        }

        /// <summary>Результаты адаптивной однопроходной дискретизации функции значений указанного типа</summary>
        /// <typeparam name="T">ТИп значений функции</typeparam>
        private sealed class SamplingResultOneWayT<T> : SamplingResult<T>
        {
            public SamplingResultOneWayT
            (
                [NotNull] Func<double, T> f,
                [NotNull] Func<T, double> converter,
                double x1,
                double x2,
                double eps,
                double dx
            ) : base(null, 0) =>
                Values = f.SamplingAdaptive_OneWayT(converter, x1, x2, eps, dx, a => Accuracy = a);
        }

        /// <summary>Адаптивная дискретизация функции в один проход</summary>
        /// <typeparam name="T">Тип значений функции</typeparam>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="converter">Метод преобразования значений функции в вещественные числа для оценки качества дискретизации</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность дискретизации</param>
        /// <param name="dx">Начальный шаг дискретизации</param>
        /// <returns>Результат дискретизации функции</returns>
        [NotNull]
        public static SamplingResult<T> SamplingAdaptive_OneWay<T>
        (
            [NotNull] this Func<double, T> f,
            [NotNull] Func<T, double> converter,
            double x1,
            double x2,
            double eps,
            double dx = 0
        ) =>
            new SamplingResultOneWayT<T>(f, converter, Math.Min(x1, x2), Math.Max(x1, x2), eps, dx <= 0 ? eps : dx);

        /// <summary>МЕтод последовательной дискретизации функции с адаптивным шагом</summary>
        /// <typeparam name="T">Тип значений функции</typeparam>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="converter">Метод преобразования значений функции в вещественные числа для оценки качества дискретизации</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность</param>
        /// <param name="dx">Начальный шаг дискретизации</param>
        /// <param name="UpdateAccuracy">Метод обновления значения текущего уровня точности дискретизации</param>
        /// <returns>Перечисление отсчётов функции</returns>
        private static IEnumerable<SamplingResult<T>.Result> SamplingAdaptive_OneWayT<T>
        (
            [NotNull] this Func<double, T> f,
            [NotNull] Func<T, double> converter,
            double x1,
            double x2,
            double eps,
            double dx,
            Action<double> UpdateAccuracy
        )
        {
            var x = x1;
            var v = f(x);
            var y = converter(v);
            yield return new SamplingResult<T>.Result(x, v);
            var accuracy = 0d;

            do
            {
                x += dx;
                var dy = -y + (y = converter(v = f(x)));
                yield return new SamplingResult<T>.Result(x, v);
                var l = Math.Sqrt(dx * dx + dy * dy);
                dx *= eps / l;
                var dl = l - eps;
                dl *= dl;
                UpdateAccuracy(Math.Sqrt(accuracy += dl));
            } while (x + dx < x2);
        }

        #endregion

        #region Метод половинного деления

        /// <summary>Результат дискретизации методом половинного деления</summary>
        private sealed class SamplingResultHalfDivision : SamplingResult<double>
        {
            /// <summary>Инициализация нового экземпляра результата дискретизации методом половинного деления</summary>
            /// <param name="Result">Результаты дискретизации, содержащие массив отсчётов функции и значение точности</param>
            private SamplingResultHalfDivision([NotNull] Tuple<Result[], double> Result) : base(Result.Item1, Result.Item2) { }

            /// <summary>Инициализация нового экземпляра результата дискретизации функции методом половинного деления</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="eps">Требуемая точность</param>
            public SamplingResultHalfDivision(Function f, double x1, double x2, double eps) : this(f.SamplingAdaptive_HalfDivision_(x1, x2, eps)) { }
        }

        /// <summary>Дискретизациия функции методом половинного деления</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Результат дискретизации функции методом половинного деления</returns>
        [NotNull]
        public static SamplingResult<double> SamplingAdaptive_HalfDivision(this Function f, double x1, double x2, double eps) => new SamplingResultHalfDivision(f, Math.Min(x1, x2), Math.Max(x1, x2), eps);

        /// <summary>Метод дискретизации функции методом половиноного деления</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Результаты дискретизации</returns>
        [NotNull]
        private static Tuple<SamplingResult<double>.Result[], double> SamplingAdaptive_HalfDivision_
        (
            [NotNull] this Function f,
            double x1,
            double x2,
            double eps
        )
        {
            var result = new LinkedList<SamplingResult<double>.Result>();
            result.AddFirst(new SamplingResult<double>.Result(x1, f(x1)));
            result.AddLast(new SamplingResult<double>.Result(x2, f(x2)));
            var accuracy = 0d;

            var node = result.First;

            do
            {
                Debug.Assert(node?.Next != null, "node?.Next != null");
                var v1 = node.Value;
                var v2 = node.Next.Value;
                var xx = v2.Argument - v1.Argument;
                var yy = v2.Value - v1.Value;
                var l = Math.Sqrt(xx * xx + yy * yy);

                if (l > eps)
                {
                    var x = (v1.Argument + v2.Argument) / 2;
                    result.AddAfter(node, new SamplingResult<double>.Result(x, f(x)));
                }
                else
                {
                    var x = (v1.Argument + v2.Argument) / 2;
                    var y = f(x);
                    var dx = x - v1.Argument;
                    var dy = y - v1.Value;
                    l = Math.Sqrt(dx * dx + dy * dy);
                    if (l > eps)
                        result.AddAfter(node, new SamplingResult<double>.Result(x, y));
                    else
                    {
                        node = node.Next;
                        var dl = eps - l;
                        dl *= dl;
                        accuracy += dl;
                    }
                }

            } while (node?.Next != null);

            return new Tuple<SamplingResult<double>.Result[], double>(result.ToArray(), Math.Sqrt(accuracy));
        }

        /// <summary>Результат дискретизации методом половинного деления</summary>
        /// <typeparam name="T">Тип значений функции</typeparam>
        private sealed class SamplingResultHalfDivisionT<T> : SamplingResult<T>
        {
            /// <summary>Инициализация нового экземпляра результата дискретизации методом половинного деления</summary>
            /// <param name="Result">Результаты дискретизации, содержащие массив отсчётов функции и значение точности</param>
            private SamplingResultHalfDivisionT([NotNull] Tuple<Result[], double> Result) : base(Result.Item1, Result.Item2) { }

            /// <summary>Инициализация нового экземпляра результата дискретизации функции методом половинного деления</summary>
            /// <param name="f">Дискретизируемая функция</param>
            /// <param name="converter">Метод преобразования значений функции в вещественные числа для оценки качества дискретизации</param>
            /// <param name="x1">Начало интервала дискретизации</param>
            /// <param name="x2">Конец интервала дискретизации</param>
            /// <param name="eps">Требуемая точность</param>
            public SamplingResultHalfDivisionT([NotNull] Func<double, T> f, [NotNull] Func<T, double> converter, double x1, double x2, double eps)
                : this(f.SamplingAdaptive_HalfDivision_(converter, x1, x2, eps)) { }
        }

        /// <summary>Дискретизациия функции методом половинного деления</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="converter">Метод преобразования значений функции в вещественные числа для оценки качества дискретизации</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Результат дискретизации функции методом половинного деления</returns>
        [NotNull]
        public static SamplingResult<T> SamplingAdaptive_HalfDivision<T>
        (
            [NotNull] this Func<double, T> f,
            [NotNull] Func<T, double> converter,
            double x1,
            double x2,
            double eps
        ) =>
            new SamplingResultHalfDivisionT<T>(f, converter, Math.Min(x1, x2), Math.Max(x1, x2), eps);

        /// <summary>Метод дискретизации функции методом половиноного деления</summary>
        /// <param name="f">Дискретизируемая функция</param>
        /// <param name="converter">Метод преобразования значений функции в вещественные числа для оценки качества дискретизации</param>
        /// <param name="x1">Начало интервала дискретизации</param>
        /// <param name="x2">Конец интервала дискретизации</param>
        /// <param name="eps">Требуемая точность</param>
        /// <returns>Результаты дискретизации</returns>
        [NotNull]
        private static Tuple<SamplingResult<T>.Result[], double> SamplingAdaptive_HalfDivision_<T>
        (
            [NotNull] this Func<double, T> f,
            [NotNull] Func<T, double> converter,
            double x1,
            double x2,
            double eps
        )
        {
            var result = new LinkedList<SamplingResult<T>.Result>();
            result.AddFirst(new SamplingResult<T>.Result(x1, f(x1)));
            result.AddLast(new SamplingResult<T>.Result(x2, f(x2)));
            var accuracy = 0d;

            var node = result.First;

            do
            {
                var v1 = node.Value;

                var x11 = v1.Argument;

                var y11 = converter(v1.Value);
                Debug.Assert(node?.Next != null, "node?.Next != null");
                var v2 = node.Next.Value;
                var x22 = v2.Argument;
                var y22 = converter(v2.Value);
                var l = Math.Sqrt((x22 - x11) * (x22 - x11) + (y22 - y11) * (y22 - y11));

                if (l > eps)
                {
                    var x = (x11 + x22) / 2;
                    result.AddAfter(node, new SamplingResult<T>.Result(x, f(x)));
                }
                else
                {
                    var x = (x11 + x22) / 2;
                    var v = f(x);
                    var y = converter(v);
                    var dx = x - x11;
                    var dy = y - y11;
                    l = Math.Sqrt(dx * dx + dy * dy);
                    if (l > eps)
                        result.AddAfter(node, new SamplingResult<T>.Result(x, v));
                    else
                    {
                        node = node.Next;
                        var dl = eps - l;
                        accuracy += dl * dl;
                    }
                }

            } while (node?.Next != null);

            return new Tuple<SamplingResult<T>.Result[], double>(result.ToArray(), Math.Sqrt(accuracy));
        }

        #endregion

        #endregion
    }
}