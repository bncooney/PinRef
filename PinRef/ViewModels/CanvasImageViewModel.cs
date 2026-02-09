using System.Windows.Media.Imaging;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using PinRef.Models;
using PinRef.Services;

namespace PinRef.ViewModels;

public partial class CanvasImageViewModel : ObservableObject
{
	[ObservableProperty]
	private double _x;

	[ObservableProperty]
	private double _y;

	[ObservableProperty]
	private double _width;

	[ObservableProperty]
	private double _height;

	[ObservableProperty]
	private bool _isSelected;

	[ObservableProperty]
	private int _zIndex;

	[ObservableProperty]
	private BitmapSource _imageSource;

	public string FilePath { get; }

	public double AspectRatio { get; private set; }

	public CanvasImageViewModel(CanvasImage model, double x, double y)
	{
		Guard.IsNotNull(model);

		FilePath = model.FilePath;
		_imageSource = model.ImageSource;
		AspectRatio = model.OriginalWidth / model.OriginalHeight;

		X = x;
		Y = y;

		var initialWidth = Math.Min(model.OriginalWidth, 400);
		Width = initialWidth;
		Height = initialWidth / AspectRatio;
	}

	public async void Reload()
	{
		// Small delay to ensure the file is fully written and released
		await Task.Delay(100);

		var reloaded = ImageLoader.TryLoad(FilePath);
		if (reloaded is null)
		{
			System.Diagnostics.Debug.WriteLine($"[Reload] FAILED to load: {FilePath}");
			return;
		}

		System.Diagnostics.Debug.WriteLine($"[Reload] SUCCESS - Old hash: {ImageSource.GetHashCode()}, New hash: {reloaded.ImageSource.GetHashCode()}");
		ImageSource = reloaded.ImageSource;
		AspectRatio = reloaded.OriginalWidth / reloaded.OriginalHeight;
		System.Diagnostics.Debug.WriteLine($"[Reload] ImageSource updated for: {FilePath}");
	}
}
