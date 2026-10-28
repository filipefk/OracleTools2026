using System.ComponentModel;

namespace OracleTools2026.ViewModels;

// Lista com checkbox nas linhas, "Selecionar todos" de três estados e contagem dos marcados.
public sealed class CheckableItemList<T> : ItemList<T> where T : CheckableItem
{
    private bool _updatingAll;

    public int CheckedCount => Items.Count(i => i.IsChecked);

    public IReadOnlyList<T> CheckedItems => Items.Where(i => i.IsChecked).ToList();

    // null = alguns marcados.
    public bool? AllChecked
    {
        get
        {
            var checkedCount = CheckedCount;
            return checkedCount == 0 ? false : checkedCount == Items.Count ? true : null;
        }
        set
        {
            // O CheckBox de três estados passa por null ao ser clicado; trata como "desmarcar todos".
            var check = value == true;
            _updatingAll = true;
            try
            {
                foreach (var item in Items)
                {
                    item.IsChecked = check;
                }
            }
            finally
            {
                _updatingAll = false;
            }

            NotifyCheckedChanged();
        }
    }

    protected override void OnItemAdded(T item) => item.PropertyChanged += Item_PropertyChanged;

    protected override void OnItemRemoved(T item) => item.PropertyChanged -= Item_PropertyChanged;

    protected override void OnItemsReset() => NotifyCheckedChanged();

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_updatingAll && e.PropertyName == nameof(CheckableItem.IsChecked))
        {
            NotifyCheckedChanged();
        }
    }

    private void NotifyCheckedChanged()
    {
        OnPropertyChanged(nameof(CheckedCount));
        OnPropertyChanged(nameof(AllChecked));
    }
}
