using System.IO;
using System.Windows;
using PinRef.Services;
using PinRef.ViewModels;

namespace PinRef;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var mainWindow = new MainWindow();
		mainWindow.Show();

		var viewModel = (MainViewModel)mainWindow.DataContext;
		var position = new Point(20, 20);

		foreach (var arg in e.Args)
		{
			if (ImageLoader.IsImageFile(arg) && File.Exists(arg))
			{
				viewModel.AddImageCommand.Execute((arg, position));
				position = new Point(position.X + 20, position.Y + 20);
			}
		}
	}
}
