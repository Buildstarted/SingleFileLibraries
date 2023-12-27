using System;

public static class MathMinMaxExtensions
{
    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static int AtMost(this int value, int min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static int AtLeast(this int value, int max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static double AtMost(this double value, double min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static double AtLeast(this double value, double max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static float AtMost(this float value, float min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static float AtLeast(this float value, float max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static byte AtMost(this byte value, byte min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static byte AtLeast(this byte value, byte max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static long AtMost(this long value, long min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static long AtLeast(this long value, long max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static ulong AtMost(this ulong value, ulong min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static ulong AtLeast(this ulong value, ulong max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static sbyte AtMost(this sbyte value, sbyte min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static sbyte AtLeast(this sbyte value, sbyte max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static uint AtMost(this uint value, uint min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static uint AtLeast(this uint value, uint max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static short AtMost(this short value, short min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static short AtLeast(this short value, short max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static ushort AtMost(this ushort value, ushort min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static ushort AtLeast(this ushort value, ushort max) => Math.Max(value, max);

    /// <summary>
    /// Returns the smaller of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="min">The minimum value</param>
    /// <returns>value or min. Whichever is smaller.</returns>
    public static decimal AtMost(this decimal value, decimal min) => Math.Min(value, min);

    /// <summary>
    /// Returns the larger of two values.
    /// </summary>
    /// <param name="value">The value to compare</param>
    /// <param name="max">The maxiumum value</param>
    /// <returns>value or max. Whichever is larger.</returns>
    public static decimal AtLeast(this decimal value, decimal max) => Math.Max(value, max);
}