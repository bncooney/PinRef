using OpenQA.Selenium.Appium.Windows;

namespace PinRef.AutoTest;

[TestClass]
public class ResetCanvasTests
{
	private static WindowsDriver<WindowsElement> Driver => PinRefSession.Driver;

	[ClassInitialize]
	public static void ClassInit(TestContext _)
	{
		var imagePath = Path.Combine(
			PinRefSession.FindSolutionDirectory(),
			"PinRef.Packaging", "Images", "SplashScreen.scale-400.png");

		PinRefSession.Setup($"\"{imagePath}\"");
	}

	[ClassCleanup]
	public static void ClassCleanup() => PinRefSession.TearDown();

	private static void ActivateButton(WindowsElement button) => button.SendKeys(" ");

	[TestMethod]
	public void ResetCanvas_ClearsImagesAndResetsZoom()
	{
		// Verify image is present before reset.
		var images = Driver.FindElementsByAccessibilityId("CanvasImage");
		Assert.IsNotEmpty(images, "Expected at least one image before reset.");

		var resetButton = Driver.FindElementByAccessibilityId("ResetCanvasButton");
		ActivateButton(resetButton);

		// Verify zoom is back to 100%.
		var zoomDisplay = Driver.FindElementByAccessibilityId("ZoomDisplay");
		StringAssert.Contains(zoomDisplay.Text, "100");

		// Verify images are cleared — collapsed elements leave the UIA tree.
		Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
		try
		{
			var remaining = Driver.FindElementsByAccessibilityId("CanvasImage");
			Assert.IsEmpty(remaining, "Expected no images on canvas after reset.");
		}
		finally
		{
			Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
		}

		// Verify the drop hint reappears.
		var dropHint = Driver.FindElementByAccessibilityId("DropHintText");
		Assert.IsNotNull(dropHint);
		Assert.IsTrue(dropHint.Displayed);
	}
}
