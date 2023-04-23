using System;

public static class MathExtensions
{
    // Clamp the value between the min and max. Value returned will be min or max if it's below min or above max
    public static double Clamp(this Double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}