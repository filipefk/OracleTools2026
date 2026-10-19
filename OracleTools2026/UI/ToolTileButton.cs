using OracleTools2026.Models;

namespace OracleTools2026.UI;

// Botão do menu inicial: ícone, título e descrição da ferramenta. Herda de Button para manter
// foco, teclado (Enter/Espaço) e acessibilidade.
public sealed class ToolTileButton : Button
{
    private static readonly string[] IconFontNames = ["Segoe Fluent Icons", "Segoe MDL2 Assets"];

    private Font? _glyphFont;
    private Font? _titleFont;

    public ToolTileButton(ToolDefinition tool)
    {
        Tool = tool;
        AccessibleName = tool.Title;
        AccessibleDescription = tool.Description;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderColor = SystemColors.ControlDark;
        FlatAppearance.MouseOverBackColor = SystemColors.GradientInactiveCaption;
        FlatAppearance.MouseDownBackColor = SystemColors.GradientActiveCaption;
        BackColor = SystemColors.Window;
        Cursor = Cursors.Hand;
        Margin = new Padding(6);
        UpdateLayoutMetrics();
    }

    public ToolDefinition Tool { get; }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UpdateLayoutMetrics();
    }

    private void UpdateLayoutMetrics()
    {
        _glyphFont?.Dispose();
        _titleFont?.Dispose();
        _glyphFont = CreateIconFont(Font.SizeInPoints * 2.2f);
        _titleFont = new Font(Font, FontStyle.Bold);
        Size = new Size(Font.Height * 22, Font.Height * 4);
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        // Text vazio: o Button desenha só o fundo, a borda e o foco; o conteúdo é desenhado abaixo.
        base.OnPaint(pevent);
        if (_glyphFont is null || _titleFont is null)
        {
            return;
        }

        var graphics = pevent.Graphics;
        var padding = Font.Height / 2;
        var glyphSize = Font.Height * 3;
        var glyphBounds = new Rectangle(padding, (Height - glyphSize) / 2, glyphSize, glyphSize);
        var textLeft = glyphBounds.Right + padding;
        var textWidth = Width - textLeft - padding;

        var accent = Enabled ? Color.FromArgb(0, 95, 184) : SystemColors.GrayText;
        var textColor = Enabled ? SystemColors.ControlText : SystemColors.GrayText;

        TextRenderer.DrawText(graphics, Tool.Glyph, _glyphFont, glyphBounds, accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var titleBounds = new Rectangle(textLeft, padding, textWidth, _titleFont.Height);
        TextRenderer.DrawText(graphics, Tool.Title, _titleFont, titleBounds, textColor,
            TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        var descriptionBounds = new Rectangle(textLeft, titleBounds.Bottom + 2, textWidth, Height - titleBounds.Bottom - padding);
        TextRenderer.DrawText(graphics, Tool.Description, Font, descriptionBounds, SystemColors.GrayText,
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _glyphFont?.Dispose();
            _titleFont?.Dispose();
        }

        base.Dispose(disposing);
    }

    // Se a fonte não existir o GDI+ troca por outra silenciosamente; por isso o nome é conferido.
    private static Font CreateIconFont(float size)
    {
        foreach (var name in IconFontNames)
        {
            var font = new Font(name, size);
            if (font.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return font;
            }

            font.Dispose();
        }

        return new Font(FontFamily.GenericSansSerif, size);
    }
}
