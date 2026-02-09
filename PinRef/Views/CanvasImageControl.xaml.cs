using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PinRef.ViewModels;

namespace PinRef.Views;

public partial class CanvasImageControl : UserControl
{
	private const double MinSize = 50;

	private CanvasImageViewModel? ViewModel => DataContext as CanvasImageViewModel;

	public CanvasImageControl()
	{
		InitializeComponent();
	}

	private void OnResizeTopLeft(object sender, DragDeltaEventArgs e)
	{
		if (ViewModel is null) return;

		var newWidth = Math.Max(MinSize, ViewModel.Width - e.HorizontalChange);
		var newHeight = GetConstrainedHeight(newWidth, ViewModel.Height - e.VerticalChange);
		newWidth = GetConstrainedWidth(newWidth, newHeight);

		var deltaX = ViewModel.Width - newWidth;
		var deltaY = ViewModel.Height - newHeight;

		ViewModel.X += deltaX;
		ViewModel.Y += deltaY;
		ViewModel.Width = newWidth;
		ViewModel.Height = newHeight;
	}

	private void OnResizeTopRight(object sender, DragDeltaEventArgs e)
	{
		if (ViewModel is null) return;

		var newWidth = Math.Max(MinSize, ViewModel.Width + e.HorizontalChange);
		var newHeight = GetConstrainedHeight(newWidth, ViewModel.Height - e.VerticalChange);
		newWidth = GetConstrainedWidth(newWidth, newHeight);

		var deltaY = ViewModel.Height - newHeight;

		ViewModel.Y += deltaY;
		ViewModel.Width = newWidth;
		ViewModel.Height = newHeight;
	}

	private void OnResizeBottomLeft(object sender, DragDeltaEventArgs e)
	{
		if (ViewModel is null) return;

		var newWidth = Math.Max(MinSize, ViewModel.Width - e.HorizontalChange);
		var newHeight = GetConstrainedHeight(newWidth, ViewModel.Height + e.VerticalChange);
		newWidth = GetConstrainedWidth(newWidth, newHeight);

		var deltaX = ViewModel.Width - newWidth;

		ViewModel.X += deltaX;
		ViewModel.Width = newWidth;
		ViewModel.Height = newHeight;
	}

	private void OnResizeBottomRight(object sender, DragDeltaEventArgs e)
	{
		if (ViewModel is null) return;

		var newWidth = Math.Max(MinSize, ViewModel.Width + e.HorizontalChange);
		var newHeight = GetConstrainedHeight(newWidth, ViewModel.Height + e.VerticalChange);
		newWidth = GetConstrainedWidth(newWidth, newHeight);

		ViewModel.Width = newWidth;
		ViewModel.Height = newHeight;
	}

	private double GetConstrainedHeight(double width, double proposedHeight)
	{
		if (ViewModel is null) return proposedHeight;

		if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
		{
			return width / ViewModel.AspectRatio;
		}

		return Math.Max(MinSize, proposedHeight);
	}

	private double GetConstrainedWidth(double proposedWidth, double height)
	{
		if (ViewModel is null) return proposedWidth;

		if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
		{
			return height * ViewModel.AspectRatio;
		}

		return Math.Max(MinSize, proposedWidth);
	}
}
