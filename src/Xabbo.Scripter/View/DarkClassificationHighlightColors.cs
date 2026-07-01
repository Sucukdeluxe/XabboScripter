using System.Windows.Media;

using ICSharpCode.AvalonEdit.Highlighting;

using RoslynPad.Editor;

namespace Xabbo.Scripter.View;

public class DarkClassificationHighlightColors : ClassificationHighlightColors
{
    public DarkClassificationHighlightColors()
    {
        DefaultBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Colors.White) };
    }
}
