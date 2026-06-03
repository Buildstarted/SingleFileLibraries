record Window<T>(T? Previous, T Current, T? Next);

static class Extension
{
	public static IEnumerable<Window<T>> Windowed<T>(this IEnumerable<T> list)
	{
		using var e = list.GetEnumerator();
		if(!e.MoveNext()) {
			yield break;
		}
		
		var current = e.Current;
		
		if(!e.MoveNext()) {
			yield return new Window<T>(default, current, default);
			yield break;
		}
		
		var next = e.Current;
		yield return new Window<T>(default, current, next);
		
		while(e.MoveNext()) {
			var previous = current;
			current = next;
			next = e.Current;
			
			yield return new Window<T>(previous, current, next);
		}
		
		yield return new Window<T>(current, next, default);
	}
}