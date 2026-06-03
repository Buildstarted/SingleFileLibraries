public static class MathMinMaxExtensions
{
    extension(System.Math)
    {
        /// <summary>
        /// Returns the smaller of two values.
        /// </summary>
        /// <param name="value">The value to compare</param>
        /// <param name="min">The minimum value</param>
        /// <returns>value or min. Whichever is smaller.</returns>
        public static T AtMost<T>(T value, T min) where T : System.Numerics.INumber<T> => T.Min(value, min);

        /// <summary>
        /// Returns the larger of two values.
        /// </summary>
        /// <param name="value">The value to compare</param>
        /// <param name="max">The maximum value</param>
        /// <returns>value or max. Whichever is larger.</returns>
        public static T AtLeast<T>(T value, T max) where T: System.Numerics.INumber<T> => T.Max(value, max);
    }
}