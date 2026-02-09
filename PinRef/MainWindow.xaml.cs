using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PinRef.Services;
using PinRef.ViewModels;

namespace PinRef;

public partial class MainWindow : Window
{
	private MainViewModel ViewModel => (MainViewModel)DataContext;

	private bool _isPanning;
	private Point _panStart;

	private bool _isDraggingImage;
	private Point _dragStart;
	private double _dragStartX;
	private double _dragStartY;
	private CanvasImageViewModel? _draggedImage;

	public MainWindow()
	{
		InitializeComponent();
		var viewModel = new MainViewModel();
		viewModel.PropertyChanged += OnViewModelPropertyChanged;
		DataContext = viewModel;
		Closed += (_, _) => viewModel.Dispose();
	}

	private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(MainViewModel.IsPinned))
		{
			UpdateWindowStyle();
		}
	}

	private void UpdateWindowStyle()
	{
		if (ViewModel.IsPinned)
		{
			WindowStyle = WindowStyle.None;
			ResizeMode = ResizeMode.CanResizeWithGrip;
		}
		else
		{
			WindowStyle = WindowStyle.SingleBorderWindow;
			ResizeMode = ResizeMode.CanResize;
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Focus();
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (ViewModel.IsPinned && e.Key == Key.Escape)
		{
			ViewModel.IsPinned = false;
			e.Handled = true;
		}
	}

	private void OnDragOver(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			var files = e.Data.GetData(DataFormats.FileDrop) as string[];
			e.Effects = files?.Any(ImageLoader.IsImageFile) == true
				? DragDropEffects.Copy
				: DragDropEffects.None;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}

		e.Handled = true;
	}

	private void OnDrop(object sender, DragEventArgs e)
	{
		if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
			return;

		var dropPoint = e.GetPosition(CanvasBorder);
		var canvasPoint = ViewModel.ScreenToCanvas(dropPoint);

		foreach (var file in files.Where(ImageLoader.IsImageFile))
		{
			ViewModel.AddImageCommand.Execute((file, canvasPoint));
			canvasPoint = new Point(canvasPoint.X + 20, canvasPoint.Y + 20);
		}
	}

	private void OnMouseWheel(object sender, MouseWheelEventArgs e)
	{
		var mousePos = e.GetPosition(CanvasBorder);
		var zoomFactor = e.Delta > 0 ? 1.15 : 1 / 1.15;
		ViewModel.Zoom(zoomFactor, mousePos);
		e.Handled = true;
	}

	private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.MiddleButton == MouseButtonState.Pressed)
		{
			_isPanning = true;
			_panStart = e.GetPosition(CanvasBorder);
			CanvasBorder.CaptureMouse();
			e.Handled = true;
		}
		else if (e.LeftButton == MouseButtonState.Pressed && e.OriginalSource == CanvasBorder)
		{
			ViewModel.DeselectAllCommand.Execute(null);

			if (ViewModel.IsPinned)
			{
				DragMove();
			}
		}
	}

	private void OnCanvasMouseMove(object sender, MouseEventArgs e)
	{
		if (_isPanning && e.MiddleButton == MouseButtonState.Pressed)
		{
			var current = e.GetPosition(CanvasBorder);
			ViewModel.PanOffsetX += current.X - _panStart.X;
			ViewModel.PanOffsetY += current.Y - _panStart.Y;
			_panStart = current;
		}
		else if (_isDraggingImage && _draggedImage is not null)
		{
			var current = e.GetPosition(CanvasBorder);
			var delta = new Point(
				(current.X - _dragStart.X) / ViewModel.ZoomScale,
				(current.Y - _dragStart.Y) / ViewModel.ZoomScale
			);

			_draggedImage.X = _dragStartX + delta.X;
			_draggedImage.Y = _dragStartY + delta.Y;
		}
	}

	private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (_isPanning)
		{
			_isPanning = false;
			CanvasBorder.ReleaseMouseCapture();
		}
	}

	private void OnImageMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed)
			return;

		if (sender is FrameworkElement element && element.DataContext is CanvasImageViewModel imageVm)
		{
			ViewModel.SelectImageCommand.Execute(imageVm);

			_isDraggingImage = true;
			_draggedImage = imageVm;
			_dragStart = e.GetPosition(CanvasBorder);
			_dragStartX = imageVm.X;
			_dragStartY = imageVm.Y;

			element.CaptureMouse();
			e.Handled = true;
		}
	}

	private void OnImageMouseMove(object sender, MouseEventArgs e)
	{
		if (!_isDraggingImage || _draggedImage is null)
			return;

		var current = e.GetPosition(CanvasBorder);
		var delta = new Point(
			(current.X - _dragStart.X) / ViewModel.ZoomScale,
			(current.Y - _dragStart.Y) / ViewModel.ZoomScale
		);

		_draggedImage.X = _dragStartX + delta.X;
		_draggedImage.Y = _dragStartY + delta.Y;
	}

	private void OnImageMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (_isDraggingImage)
		{
			_isDraggingImage = false;
			_draggedImage = null;

			if (sender is FrameworkElement element)
			{
				element.ReleaseMouseCapture();
			}
		}
	}

	private CanvasImageViewModel? GetImageFromContextMenu(object sender)
	{
		if (sender is MenuItem menuItem &&
			menuItem.Parent is ContextMenu contextMenu &&
			contextMenu.PlacementTarget is FrameworkElement element)
		{
			return element.DataContext as CanvasImageViewModel;
		}
		return null;
	}

	private void OnBringToFrontClick(object sender, RoutedEventArgs e)
	{
		var image = GetImageFromContextMenu(sender);
		if (image is not null)
			ViewModel.BringToFrontCommand.Execute(image);
	}

	private void OnSendToBackClick(object sender, RoutedEventArgs e)
	{
		var image = GetImageFromContextMenu(sender);
		if (image is not null)
			ViewModel.SendToBackCommand.Execute(image);
	}

	private void OnRemoveImageClick(object sender, RoutedEventArgs e)
	{
		var image = GetImageFromContextMenu(sender);
		if (image is not null)
			ViewModel.RemoveImageCommand.Execute(image);
	}
}
