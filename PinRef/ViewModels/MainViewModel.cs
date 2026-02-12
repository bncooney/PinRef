using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PinRef.Services;

namespace PinRef.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
	[ObservableProperty]
	private bool _isPinned;

	[ObservableProperty]
	private double _zoomScale = 1.0;

	[ObservableProperty]
	private double _panOffsetX;

	[ObservableProperty]
	private double _panOffsetY;

	[ObservableProperty]
	private CanvasImageViewModel? _selectedImage;

	public ObservableCollection<CanvasImageViewModel> Images { get; }

	public bool HasNoImages => Images.Count == 0;

	private int _nextZIndex;

	private readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, DateTime> _lastReloadTimes = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dispatcher _dispatcher;

	public MainViewModel()
	{
		_dispatcher = Dispatcher.CurrentDispatcher;
		Images = [];
		Images.CollectionChanged += OnImagesCollectionChanged;
	}

	private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(HasNoImages));

		if (e.NewItems is not null)
		{
			foreach (CanvasImageViewModel image in e.NewItems)
			{
				WatchFile(image.FilePath);
			}
		}

		if (e.OldItems is not null)
		{
			foreach (CanvasImageViewModel image in e.OldItems)
			{
				TryUnwatchFile(image.FilePath);
			}
		}

		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			DisposeWatchers();
		}
	}

	private void WatchFile(string filePath)
	{
		var directory = Path.GetDirectoryName(filePath);
		if (string.IsNullOrEmpty(directory) || _watchers.ContainsKey(directory))
			return;

		var watcher = new FileSystemWatcher(directory)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
			EnableRaisingEvents = true
		};

		watcher.Changed += OnFileChanged;
		watcher.Created += OnFileChanged;
		watcher.Renamed += OnFileRenamed;
		_watchers[directory] = watcher;
	}

	private void OnFileRenamed(object sender, RenamedEventArgs e)
	{
		OnFileChanged(sender, e);
	}

	private void TryUnwatchFile(string filePath)
	{
		var directory = Path.GetDirectoryName(filePath);
		if (string.IsNullOrEmpty(directory))
			return;

		var hasOtherFilesInDirectory = Images.Any(img =>
			!string.Equals(img.FilePath, filePath, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(Path.GetDirectoryName(img.FilePath), directory, StringComparison.OrdinalIgnoreCase));

		if (!hasOtherFilesInDirectory && _watchers.TryGetValue(directory, out var watcher))
		{
			watcher.EnableRaisingEvents = false;
			watcher.Changed -= OnFileChanged;
			watcher.Created -= OnFileChanged;
			watcher.Renamed -= OnFileRenamed;
			watcher.Dispose();
			_watchers.Remove(directory);
		}
	}

	private void OnFileChanged(object sender, FileSystemEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"[FileWatcher] Event: {e.ChangeType}, Path: {e.FullPath}");

		if (!ImageLoader.IsImageFile(e.FullPath))
		{
			System.Diagnostics.Debug.WriteLine($"[FileWatcher] Skipped - not an image file");
			return;
		}

		// Debounce: ignore events within 500ms of last reload
		if (_lastReloadTimes.TryGetValue(e.FullPath, out var lastTime) &&
			(DateTime.Now - lastTime).TotalMilliseconds < 500)
		{
			System.Diagnostics.Debug.WriteLine($"[FileWatcher] Skipped - debounced");
			return;
		}

		_lastReloadTimes[e.FullPath] = DateTime.Now;

		var matchingImages = Images.Where(img =>
			string.Equals(img.FilePath, e.FullPath, StringComparison.OrdinalIgnoreCase)).ToList();

		System.Diagnostics.Debug.WriteLine($"[FileWatcher] Found {matchingImages.Count} matching images");

		_dispatcher.BeginInvoke(() =>
		{
			foreach (var image in matchingImages)
			{
				System.Diagnostics.Debug.WriteLine($"[FileWatcher] Reloading: {image.FilePath}");
				image.Reload();
			}
		});
	}

	[RelayCommand]
	private void TogglePin()
	{
		IsPinned = !IsPinned;
	}

	[RelayCommand]
	private void AddImage((string FilePath, Point Position) args)
	{
		var image = ImageLoader.TryLoad(args.FilePath);
		if (image is null)
			return;

		var viewModel = new CanvasImageViewModel(image, args.Position.X, args.Position.Y)
		{
			ZIndex = _nextZIndex++
		};

		Images.Add(viewModel);
		SelectImage(viewModel);
	}

	[RelayCommand]
	private void RemoveImage(CanvasImageViewModel? image)
	{
		if (image is null)
			return;

		Images.Remove(image);

		if (SelectedImage == image)
			SelectedImage = null;
	}

	[RelayCommand]
	private void RemoveSelectedImage()
	{
		RemoveImage(SelectedImage);
	}

	[RelayCommand]
	private void SelectImage(CanvasImageViewModel? image)
	{
		SelectedImage?.IsSelected = false;

		SelectedImage = image;

		if (image is not null)
		{
			image.IsSelected = true;
			BringToFront(image);
		}
	}

	[RelayCommand]
	private void DeselectAll()
	{
		SelectedImage?.IsSelected = false;
		SelectedImage = null;
	}

	[RelayCommand]
	private void BringToFront(CanvasImageViewModel? image)
	{
		if (image is null)
			return;

		image.ZIndex = _nextZIndex++;
	}

	[RelayCommand]
	private void SendToBack(CanvasImageViewModel? image)
	{
		if (image is null || Images.Count == 0)
			return;

		var minZ = Images.Min(i => i.ZIndex);
		image.ZIndex = minZ - 1;
	}

	[RelayCommand]
	private void ResetCanvas()
	{
		Images.Clear();
		SelectedImage = null;
		_nextZIndex = 0;
		ZoomScale = 1.0;
		PanOffsetX = 0;
		PanOffsetY = 0;
	}

	public void Zoom(double factor, Point center)
	{
		var newScale = Math.Clamp(ZoomScale * factor, 0.1, 10.0);
		var scaleChange = newScale / ZoomScale;

		PanOffsetX = center.X - (center.X - PanOffsetX) * scaleChange;
		PanOffsetY = center.Y - (center.Y - PanOffsetY) * scaleChange;

		ZoomScale = newScale;
	}

	public Point ScreenToCanvas(Point screenPoint)
	{
		return new Point(
			(screenPoint.X - PanOffsetX) / ZoomScale,
			(screenPoint.Y - PanOffsetY) / ZoomScale
		);
	}

	private void DisposeWatchers()
	{
		foreach (var watcher in _watchers.Values)
		{
			watcher.EnableRaisingEvents = false;
			watcher.Changed -= OnFileChanged;
			watcher.Created -= OnFileChanged;
			watcher.Renamed -= OnFileRenamed;
			watcher.Dispose();
		}
		_watchers.Clear();
	}

	public void Dispose()
	{
		DisposeWatchers();
		GC.SuppressFinalize(this);
	}
}
