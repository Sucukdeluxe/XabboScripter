using System.Windows.Media;

using ICSharpCode.AvalonEdit.Highlighting;

using RoslynPad.Editor;

namespace Xabbo.Scripter.View;

public class DarkClassificationHighlightColors : ClassificationHighlightColors
{
    public DarkClassificationHighlightColors()
    {
        DefaultBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0xD4, 0xD4, 0xD4)) };
        KeywordBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x56, 0x9C, 0xD6)) };
        TypeBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x4E, 0xC9, 0xB0)) };
        MethodBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0xDC, 0xDC, 0xAA)) };
        StringBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0xCE, 0x91, 0x78)) };
        CommentBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x6A, 0x99, 0x55)) };
        PreprocessorKeywordBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x9B, 0x9B, 0x9B)) };
        XmlCommentBrush = new HighlightingColor { Foreground = new SimpleHighlightingBrush(Color.FromRgb(0x6A, 0x99, 0x55)) };
    }
}
