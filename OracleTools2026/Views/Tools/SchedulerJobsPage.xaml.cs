using Microsoft.UI.Xaml.Controls;
using OracleTools2026.Models;
using OracleTools2026.UI;
using OracleTools2026.ViewModels;
using OracleTools2026.ViewModels.SchedulerJobs;
using WinUI.TableView;

namespace OracleTools2026.Views.Tools;

public sealed partial class SchedulerJobsPage : ToolPage
{
    public SchedulerJobsPage()
    {
        ViewModel = App.GetService<SchedulerJobsViewModel>();
        InitializeComponent();
        TableViewHelper.Attach(JobsTable, ViewModel.Jobs);
        JobsTable.RowContextFlyoutOpening += JobsTable_RowContextFlyoutOpening;
        JobsTable.CellDoubleTapped += JobsTable_CellDoubleTapped;
    }

    public SchedulerJobsViewModel ViewModel { get; }

    public override ToolViewModelBase ToolViewModel => ViewModel;

    public Predicate<TableViewConditionalCellStyleContext> IsDisabled { get; } = HasHealth(SchedulerJobHealth.Disabled);

    public Predicate<TableViewConditionalCellStyleContext> IsOverdue { get; } = HasHealth(SchedulerJobHealth.Overdue);

    public Predicate<TableViewConditionalCellStyleContext> IsLongRunning { get; } = HasHealth(SchedulerJobHealth.LongRunning);

    private static Predicate<TableViewConditionalCellStyleContext> HasHealth(SchedulerJobHealth health) =>
        context => context.DataItem is SchedulerJobItem item && item.Health == health;

    // Usa o evento da célula: com a tabela somente leitura, a célula marca o duplo clique como
    // tratado e o RowDoubleTapped nunca dispara.
    private void JobsTable_CellDoubleTapped(object? sender, TableViewCellDoubleTappedEventArgs e)
    {
        if (e.Item is SchedulerJobItem job && ViewModel.IsIdle)
        {
            e.Handled = true;
            ViewModel.OpenActionCommand.Execute(job);
        }
    }

    private void JobsTable_RowContextFlyoutOpening(object? sender, TableViewRowContextFlyoutEventArgs e)
    {
        if (ViewModel.IsBusy || e.Flyout is not MenuFlyout menu)
        {
            e.Handled = true;
            return;
        }

        menu.Items.Clear();
        if (ViewModel.Jobs.SelectedItem is { } job)
        {
            menu.Add($"Abrir ação de {job.Job.QualifiedName} no editor", Glyphs.OpenFile, () => ViewModel.OpenActionCommand.Execute(job));
            menu.Add($"Copiar ação de {job.Job.QualifiedName}", Glyphs.Copy, () => ViewModel.CopyActionCommand.Execute(job));
            menu.AddSeparator();
        }

        if (ViewModel.Jobs.HasSelection)
        {
            menu.Add("Copiar linhas selecionadas", Glyphs.Copy, () => TableViewHelper.CopySelected(JobsTable));
        }

        menu.Add("Copiar toda a lista", null, () => TableViewHelper.CopyAll(JobsTable));
        menu.AddSeparator();
        menu.Add("Informações do banco de dados", Glyphs.Info, () => ViewModel.ShowServerInfoCommand.Execute(null));
    }
}
