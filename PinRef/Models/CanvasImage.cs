using System.Windows.Media.Imaging;

namespace PinRef.Models;

public sealed class CanvasImage
{
	public required string FilePath { get; init; }
	public required BitmapSource ImageSource { get; init; }
	public double OriginalWidth { get; init; }
	public double OriginalHeight { get; init; }
}
