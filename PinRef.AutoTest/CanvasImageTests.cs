using OpenQA.Selenium.Appium.Windows;

namespace PinRef.AutoTest;

[TestClass]
public class CanvasImageTests
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

    // ------------------------------------------------------------------
    //  Helpers
    // ------------------------------------------------------------------

    private static void ActivateButton(WindowsElement button) => button.SendKeys(" ");

    // ------------------------------------------------------------------
    //  Image presence
    // ------------------------------------------------------------------

    [TestMethod]
    public void Image_IsDisplayed_OnCanvas()
    {
        var images = Driver.FindElementsByAccessibilityId("CanvasImage");
        Assert.IsNotEmpty(images, "Expected at least one image on the canvas.");
    }

    [TestMethod]
    public void DropHint_IsHidden_WhenImagePresent()
    {
        // Collapsed elements are removed from the UIA tree.
        // Use zero implicit wait so we don't block for 5 seconds.
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        try
        {
            var hints = Driver.FindElementsByAccessibilityId("DropHintText");
            Assert.IsEmpty(hints,
                "Drop hint should not be visible when an image is loaded.");
        }
        finally
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }
    }
}
