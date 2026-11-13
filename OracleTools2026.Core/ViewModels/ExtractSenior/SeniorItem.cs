using CommunityToolkit.Mvvm.ComponentModel;
using OracleTools2026.Formatting;
using OracleTools2026.Models.Senior;

namespace OracleTools2026.ViewModels.ExtractSenior;

public sealed partial class SeniorItem : CheckableItem
{
    public SeniorItem(SeniorItemSummary summary)
    {
        Summary = summary;
        IsChecked = true;
        Message = "";
    }

    public SeniorItemSummary Summary { get; }

    public SeniorItemKey Key => Summary.Key;

    public SeniorItemKind Kind => Summary.Key.Kind;

    public string KindText => Kind switch
    {
        SeniorItemKind.WebService => "Webservice",
        SeniorItemKind.Table => "Tabela",
        SeniorItemKind.StandardTableFields => "Campos em tabela padrão",
        SeniorItemKind.List => "Lista",
        SeniorItemKind.View => "Visão",
        SeniorItemKind.RuleLink => "Regra",
        _ => Kind.ToString()
    };

    public string Name => Summary.Key.Name;

    public string Detail => Summary.Detail;

    public DateTime? LastChange => Summary.LastChange;

    public string LastChangeText => DisplayFormat.Date(Summary.LastChange);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    public partial SeniorItemStatus Status { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; }

    public string StatusText => Status switch
    {
        SeniorItemStatus.Pending => "Aguardando",
        SeniorItemStatus.NotChecked => "Não marcado",
        SeniorItemStatus.Written => "Gravado",
        SeniorItemStatus.Unchanged => "Sem alteração",
        SeniorItemStatus.Error => "Erro",
        SeniorItemStatus.NotFound => "Não encontrado",
        _ => ""
    };

    public void SetOutcome(SeniorItemProgress progress)
    {
        Status = progress.Outcome switch
        {
            SeniorItemOutcome.Written => SeniorItemStatus.Written,
            SeniorItemOutcome.Unchanged => SeniorItemStatus.Unchanged,
            _ => SeniorItemStatus.Error
        };
        Message = progress.Message ?? "";
    }
}
