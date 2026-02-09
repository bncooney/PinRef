using OpenQA.Selenium.Appium.Windows;

namespace PinRef.AutoTest;

[TestClass]
public class CanvasImageTests
{
    private static WindowsDriver<WindowsElement> Driver => PinRefSession.Driver;
    private static string _testImagePath = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        _testImagePath = CreateTestImage();
        PinRefSession.Setup($"\"{_testImagePath}\"");
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        PinRefSession.TearDown();
		try { File.Delete(_testImagePath); } catch { }
	}

    // ------------------------------------------------------------------
    //  Helpers
    // ------------------------------------------------------------------

    private static void ActivateButton(WindowsElement button) => button.SendKeys(" ");

    /// <summary>
    /// Creates a minimal 1×1-pixel BMP file from raw bytes so that no
    /// binary test asset needs to be checked into source control.
    /// </summary>
    private static string CreateTestImage()
    {
        var path = Path.Combine(Path.GetTempPath(), $"PinRefTest_{Guid.NewGuid():N}.bmp");
        byte[] bmp =
        [
            // BMP file header (14 bytes)
            0x42, 0x4D,             // "BM"
            0x3A, 0x00, 0x00, 0x00, // file size = 58
            0x00, 0x00, 0x00, 0x00, // reserved
            0x36, 0x00, 0x00, 0x00, // pixel data offset = 54
            // DIB header — BITMAPINFOHEADER (40 bytes)
            0x28, 0x00, 0x00, 0x00, // header size = 40
            0x01, 0x00, 0x00, 0x00, // width  = 1
            0x01, 0x00, 0x00, 0x00, // height = 1
            0x01, 0x00,             // color planes = 1
            0x18, 0x00,             // bits per pixel = 24
            0x00, 0x00, 0x00, 0x00, // compression = none
            0x04, 0x00, 0x00, 0x00, // image data size = 4
            0xC4, 0x0E, 0x00, 0x00, // x pixels/meter
            0xC4, 0x0E, 0x00, 0x00, // y pixels/meter
            0x00, 0x00, 0x00, 0x00, // colors in table
            0x00, 0x00, 0x00, 0x00, // important colors
            // Pixel data — 1 pixel (BGR) + 1 byte row padding
            0x00, 0x00, 0xFF,       // blue=0, green=0, red=255
            0x00                    // padding to 4-byte boundary
        ];
        File.WriteAllBytes(path, bmp);
        return path;
    }

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
