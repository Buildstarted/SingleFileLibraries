namespace HistoryManager;

/// <summary>
/// Undo/Redo functionality
/// </summary>
/// <typeparam name="T"></typeparam>
class History<T>
{
    private List<T> history;
    private int index;

    public History()
    {
        history = new List<T>();
        index = -1;
    }

    public History(T current) : this()
    {
        Push(current);
    }

    public History(IEnumerable<T> items) : this()
    {
        foreach (var item in items)
        {
            Push(item);
        }
    }

    /// <summary>
    /// The number of total items in the history
    /// </summary>
    public int Count => history.Count;

    /// <summary>
    /// The current position in the history
    /// </summary>
    public int Index => index;

    /// <summary>
    /// Remove all items from the history
    /// </summary>
    public void Clear()
    {
        history.Clear();
        index = -1;
    }

    /// <summary>
    /// Pushes items onto the history. This method also removes future entries
    /// if the Index is less than the number of entries
    /// </summary>
    /// <param name="item">The item to push onto the history</param>
    public void Push(T item)
    {
        if (index < history.Count - 1)
        {
            var trimto = index + 1;
            //we went back in time
            //so trim the history 
            history.RemoveRange(trimto, history.Count - trimto);
        }

        history.Add(item);
        index = history.Count - 1;
    }

    /// <summary>
    /// Advance the current position in the history by one slot
    /// </summary>
    /// <returns>The current item in the history advanced to</returns>
    /// <exception cref="InvalidOperationException">If attempting to move beyond the number of items in the history</exception>
    public T Advance()
    {
        if (TryAdvance(out var item))
        {
            return item;
        }

        throw new InvalidOperationException("Unable to advance. Reached end of list.");
    }

    /// <summary>
    /// Reverses the current position in the history by one slot
    /// </summary>
    /// <returns>The current item in the history reversed to</returns>
    /// <exception cref="InvalidOperationException">If attempting to move beyond the first item in the history</exception>
    public T Reverse()
    {
        if (TryReverse(out var item))
        {
            return item;
        }

        throw new InvalidOperationException("Unable to reverse. Reached end of list.");
    }

    /// <summary>
    /// Helper method to advance 
    /// </summary>
    /// <param name="result">The current item in the history advanced to</param>
    /// <returns>Boolean whether or not the operation succeeded</returns>
    public bool TryAdvance(out T result)
    {
        if (index < history.Count - 1)
        {
            index++;
            result = history[index];
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Helper method to reverse
    /// </summary>
    /// <param name="result">The current item in the history reversed to</param>
    /// <returns>Boolean whether or not the operation succeeded</returns>
    public bool TryReverse(out T result)
    {
        if (index > 0)
        {
            index--;
            var item = history[index];
            result = item;
            return true;
        }

        result = default;
        return false;
    }
}
