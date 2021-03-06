﻿using DST = System.Diagnostics.DebuggerStepThroughAttribute;
using DU = System.Func<double, double, double>;
// ReSharper disable UnusedMember.Global

namespace MathCore.DifferentialEquations.Numerical
{
    public static partial class Solver
    {
        public static class Euler
        {
            [DST]
            public static double[] FixedStep(double y0, double start, double stop, int Count, DU f) => FixedStep(y0, new Interval(start, stop), Count, f);

            [DST]
            public static double[] FixedStep(double y0, Interval interval, int Count, DU f)
            {
                var length = interval.Length;
                var dx = length / (Count - 1);

                var x = interval.Min;
                var y = y0;
                var Y = new double[Count];
                Y[0] = y;

                for(var n = 1; n < Count; n++, x += dx)
                    Y[n] = y = NextValue(x, dx, y, f);
                //y = result[n] = y + dx * f(x, y);

                return Y;
            }

            public static double NextValue(double x0, double dx, double y0, DU f) => y0 + dx * f(x0, y0);

            public static double NextValue_Modified(double x0, double dx, double y0, DU f)
            {
                var ff = f(x0, y0);
                var y1 = y0 + dx * ff;
                return y0 + .5 * dx * (ff + f(x0 + dx, y1));
            }
        }
    }
}