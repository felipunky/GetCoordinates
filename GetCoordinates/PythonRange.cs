using System;
using System.Collections.Generic;

public static class EnumerableUtilities
{
    public static IEnumerable<double> RangePython(double start, double stop, double step = 1)
    {
        if (step == 0)
            throw new ArgumentException("Parameter step cannot equal zero.");

        if (start < stop && step > 0)
        {
            for (var i = start; i < stop; i += step)
            {
                yield return i;
            }
        }
        else if (start > stop && step < 0)
        {
            for (var i = start; i > stop; i += step)
            {
                yield return i;
            }
        }
    }

    public static IEnumerable<double> RangePython(double stop)
    {
        return RangePython(0, stop);
    }
}