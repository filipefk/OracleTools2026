using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace OracleTools2026.ViewModels;

// Itens de uma lista da tela e as linhas selecionadas (informadas pela tela).
public class ItemList<T> : ObservableObject where T : class
{
    private IReadOnlyList<T> _selected = [];

    public ObservableCollection<T> Items { get; } = [];

    public int Count => Items.Count;

    public IReadOnlyList<T> Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    // A linha selecionada quando há exatamente uma.
    public T? SelectedItem => _selected.Count == 1 ? _selected[0] : null;

    public bool HasSelection => _selected.Count > 0;

    public virtual void Reset(IEnumerable<T> items)
    {
        foreach (var item in Items)
        {
            OnItemRemoved(item);
        }

        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
            OnItemAdded(item);
        }

        Selected = [];
        OnPropertyChanged(nameof(Count));
        OnItemsReset();
    }

    public void Clear() => Reset([]);

    protected virtual void OnItemAdded(T item)
    {
    }

    protected virtual void OnItemRemoved(T item)
    {
    }

    protected virtual void OnItemsReset()
    {
    }
}
