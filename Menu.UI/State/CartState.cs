namespace Menu.UI.State;

public sealed class CartState
{
    private readonly List<CartLine> _items = new();

    public event Action? OnChange;

    public IReadOnlyList<CartLine> Items => _items;

    public int TotalItems => _items.Sum(x => x.Quantity);

    public decimal TotalPrice => _items.Sum(x => x.TotalPrice * x.Quantity);

    public void Add(CartLine item)
    {
        _items.Add(item);
        OnChange?.Invoke();
    }

    public void Remove(Guid cartLineId)
    {
        var removed = _items.RemoveAll(x => x.CartLineId == cartLineId) > 0;
        if (removed)
        {
            OnChange?.Invoke();
        }
    }

    public void Clear()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _items.Clear();
        OnChange?.Invoke();
    }
}

public record CartLine(
    Guid CartLineId,
    Guid ItemId,
    string ItemName,
    decimal BasePrice,
    decimal TotalPrice,
    int Quantity,
    IReadOnlyList<CartLineSelection> Selections);

public record CartLineSelection(
    Guid OptionId,
    string OptionName,
    Guid ValueId,
    string ValueName,
    decimal PriceModifier);
