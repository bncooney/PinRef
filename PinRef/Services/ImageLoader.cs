using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Diagnostics;
using PinRef.Models;

namespace PinRef.Services;

public static class ImageLoader
{
	private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
	{
		".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tiff", ".tif"
	};

	public static bool IsImageFile(string path)
	{
		var extension = Path.GetExtension(path);
		return SupportedExtensions.Contains(extension);
	}

	public static CanvasImage? TryLoad(string filePath)
	{
		Guard.IsNotNullOrWhiteSpace(filePath);

		if (!File.Exists(filePath))
			return null;

		try
		{
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			bitmap.EndInit();
			bitmap.Freeze();

			return new CanvasImage
			{
				FilePath = filePath,
				ImageSource = bitmap,
				OriginalWidth = bitmap.PixelWidth,
				OriginalHeight = bitmap.PixelHeight
			};
		}
		catch
		{
			return null;
		}
	}
}
