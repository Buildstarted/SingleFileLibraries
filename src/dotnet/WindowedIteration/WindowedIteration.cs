/// <summary>
/// Represents a window of elements in a sequence, including the previous, current, and next elements.
/// </summary>
/// <typeparam name="T">The type of elements in the sequence.</typeparam>
/// <param name="Previous">The previous element in the sequence, or <c>null</c> if there is no previous element.</param>
/// <param name="Current">The current element in the sequence.</param>
/// <param name="Next">The next element in the sequence, or <c>null</c> if there is no next element.</param>
record Window<T>(T? Previous, T Current, T? Next);

static class WindowedIterationExtension
{
	/// <summary>
	/// Iterates over a sequence while providing access to the previous, current, and next elements.
	/// </summary>
	/// <typeparam name="T">The type of elements in the sequence.</typeparam>
	/// <param name="source">The sequence to iterate over.</param>
	/// <returns>An enumerable of <see cref="Window{T}"/> objects.</returns>
	public static IEnumerable<Window<T>> Windowed<T>(this IEnumerable<T> source)
	{
		using var e = source.GetEnumerator();
		if (!e.MoveNext())
		{
			yield break;
		}

		var current = e.Current;

		if (!e.MoveNext())
		{
			yield return new Window<T>(default, current, default);
			yield break;
		}

		var next = e.Current;
		yield return new Window<T>(default, current, next);

		while (e.MoveNext())
		{
			var previous = current;
			current = next;
			next = e.Current;

			yield return new Window<T>(previous, current, next);
		}

		yield return new Window<T>(current, next, default);
	}
}