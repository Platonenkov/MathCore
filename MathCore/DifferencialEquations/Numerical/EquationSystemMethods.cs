﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using MathCore.Vectors;

namespace MathCore.DifferencialEquations.Numerical
{
    public delegate double[] DifferentialEquationSystem(double x, double[] Y);
    public delegate Complex[] DifferentialEquationSystem_Complex(double x, Complex[] Y);
    public delegate Vector2D[] DifferentialEquationSystem_Vector2D(double x, Vector2D[] Y);
    public delegate Vector3D[] DifferentialEquationSystem_Vector3D(double x, Vector3D[] Y);

    public static class EquationSystemMethods
    {
        public struct SystemResultItem<TValue>
        {
            public readonly double x;
            public readonly ReadOnlyCollection<TValue> y;

            public SystemResultItem(double x, TValue[] y)
            {
                this.x = x;
                this.y = new ReadOnlyCollection<TValue>(y);
            }

            public override string ToString() => $"{x}:{{{string.Join(",", y)}}}";
        }

        static double[] Add(this double[] X, double[] Y, double k = 1)
        {
            var result = new double[X.Length];
            for(var i = 0; i < result.Length; i++)
                result[i] = X[i] + Y[i] * k;
            return result;
        }

        static Complex[] Add(this Complex[] X, Complex[] Y, double k = 1)
        {
            var result = new Complex[X.Length];
            for(var i = 0; i < result.Length; i++)
                result[i] = X[i] + Y[i] * k;
            return result;
        }

        static Vector2D[] Add(this Vector2D[] X, Vector2D[] Y, double k = 1)
        {
            var result = new Vector2D[X.Length];
            for(var i = 0; i < result.Length; i++)
                result[i] = X[i] + Y[i] * k;
            return result;
        }

        static Vector3D[] Add(this Vector3D[] X, Vector3D[] Y, double k = 1)
        {
            var result = new Vector3D[X.Length];
            for(var i = 0; i < result.Length; i++)
                result[i] = X[i] + Y[i] * k;
            return result;
        }

        static double[] GetRungeKuttaResult(this double[] Y, double[] K1, double[] K2, double[] K3, double[] K4, double dx)
        {
            for(var i = 0; i < Y.Length; i++)
            {
                K1[i] += 2 * K2[i] + 2 * K3[i] + K4[i];
                K1[i] *= dx / 6;
                K1[i] += Y[i];
            }
            return K1;
        }

        static Vector2D[] GetRungeKuttaResult(this Vector2D[] Y, Vector2D[] K1, Vector2D[] K2, Vector2D[] K3, Vector2D[] K4, double dx)
        {
            for(var i = 0; i < Y.Length; i++)
            {
                K1[i] += 2 * K2[i] + 2 * K3[i] + K4[i];
                K1[i] *= dx / 6;
                K1[i] += Y[i];
            }
            return K1;
        }

        static Vector3D[] GetRungeKuttaResult(this Vector3D[] Y, Vector3D[] K1, Vector3D[] K2, Vector3D[] K3, Vector3D[] K4, double dx)
        {
            for(var i = 0; i < Y.Length; i++)
            {
                K1[i] += 2 * K2[i] + 2 * K3[i] + K4[i];
                K1[i] *= dx / 6;
                K1[i] += Y[i];
            }
            return K1;
        }

        static Complex[] GetRungeKuttaResult(this Complex[] Y, Complex[] K1, Complex[] K2, Complex[] K3, Complex[] K4, double dx)
        {
            for(var i = 0; i < Y.Length; i++)
            {
                K1[i] += 2 * K2[i] + 2 * K3[i] + K4[i];
                K1[i] *= dx / 6;
                K1[i] += Y[i];
            }
            return K1;
        }

        public static IEnumerable<SystemResultItem<double>> Compute_RungeKutta(this DifferentialEquationSystem system, double x0, double x1, double dx, double[] Y0)
        {
            var x = x0;
            var Y = Y0;
            yield return new SystemResultItem<double>(x, Y);
            var dx2 = dx / 2;

            while(x <= x1)
            {
                var K1 = system(x, Y);
                var K2 = system(x + dx2, Y.Add(K1, dx2));
                var K3 = system(x + dx2, Y.Add(K2, dx2));
                var K4 = system(x + dx, Y.Add(K3, dx));
                Y = Y.GetRungeKuttaResult(K1, K2, K3, K4, dx);
                x += dx;
                yield return new SystemResultItem<double>(x, Y);
            }
        }

        public static IEnumerable<SystemResultItem<Complex>> Compute_RungeKutta(this DifferentialEquationSystem_Complex system, double x0, double x1, double dx, Complex[] Y0)
        {
            var x = x0;
            var Y = Y0;
            yield return new SystemResultItem<Complex>(x, Y);
            var dx2 = dx / 2;

            while(x <= x1)
            {
                var K1 = system(x, Y);
                var K2 = system(x + dx2, Y.Add(K1, dx2));
                var K3 = system(x + dx2, Y.Add(K2, dx2));
                var K4 = system(x + dx, Y.Add(K3, dx));
                Y = Y.GetRungeKuttaResult(K1, K2, K3, K4, dx);
                x += dx;
                yield return new SystemResultItem<Complex>(x, Y);
            }
        }

        public static IEnumerable<SystemResultItem<Vector2D>> Compute_RungeKutta(this DifferentialEquationSystem_Vector2D system, double x0, double x1, double dx, Vector2D[] Y0)
        {
            var x = x0;
            var Y = Y0;
            yield return new SystemResultItem<Vector2D>(x, Y);
            var dx2 = dx / 2;

            while(x <= x1)
            {
                var K1 = system(x, Y);
                var K2 = system(x + dx2, Y.Add(K1, dx2));
                var K3 = system(x + dx2, Y.Add(K2, dx2));
                var K4 = system(x + dx, Y.Add(K3, dx));
                Y = Y.GetRungeKuttaResult(K1, K2, K3, K4, dx);
                x += dx;
                yield return new SystemResultItem<Vector2D>(x, Y);
            }
        }

        public static IEnumerable<SystemResultItem<Vector3D>> Compute_RungeKutta(this DifferentialEquationSystem_Vector3D system, double x0, double x1, double dx, Vector3D[] Y0)
        {
            var x = x0;
            var Y = Y0;
            yield return new SystemResultItem<Vector3D>(x, Y);
            var dx2 = dx / 2;

            while(x <= x1)
            {
                var K1 = system(x, Y);
                var K2 = system(x + dx2, Y.Add(K1, dx2));
                var K3 = system(x + dx2, Y.Add(K2, dx2));
                var K4 = system(x + dx, Y.Add(K3, dx));
                Y = Y.GetRungeKuttaResult(K1, K2, K3, K4, dx);
                x += dx;
                yield return new SystemResultItem<Vector3D>(x, Y);
            }
        }
    }
}
