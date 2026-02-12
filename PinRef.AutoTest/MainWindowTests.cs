using OpenQA.Selenium.Appium.Windows;

namespace PinRef.AutoTest;

[TestClass]
public class MainWindowTests
{
    private static WindowsDriver<WindowsElement> Driver => PinRefSession.Driver;

    [ClassInitialize]
    public static void ClassInit(TestContext _) => PinRefSession.Setup();

    [ClassCleanup]
    public static void ClassCleanup() => PinRefSession.TearDown();

    // ------------------------------------------------------------------
    //  Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Activates a WPF button via the Space key. WinAppDriver's <c>.Click()</c>
    /// sends a simulated mouse click that does not reliably invoke WPF commands;
    /// the Space key triggers <c>ButtonBase.OnClick</c> directly.
    /// </summary>
    private static void ActivateButton(WindowsElement button) => button.SendKeys(" ");

    // ------------------------------------------------------------------
    //  Launch & window basics
    // ------------------------------------------------------------------

    [TestMethod]
    public void App_Launches_WithCorrectTitle()
    {
        Assert.AreEqual("PinRef", Driver.Title);
    }

    [TestMethod]
    public void Window_IsNotTopmost_ByDefault()
    {
        var pinButton = Driver.FindElementByAccessibilityId("PinButton");
        Assert.IsTrue(pinButton.Displayed);
    }

    // ------------------------------------------------------------------
    //  Toolbar elements
    // ------------------------------------------------------------------

    [TestMethod]
    public void Toolbar_PinButton_IsDisplayed()
    {
        var pinButton = Driver.FindElementByAccessibilityId("PinButton");
        Assert.IsNotNull(pinButton);
        Assert.IsTrue(pinButton.Displayed);
    }

    [TestMethod]
    public void Toolbar_ResetButton_IsDisplayed()
    {
        var resetButton = Driver.FindElementByAccessibilityId("ResetCanvasButton");
        Assert.IsNotNull(resetButton);
        Assert.IsTrue(resetButton.Displayed);
    }

    [TestMethod]
    public void Toolbar_ZoomDisplay_ShowsDefault100Percent()
    {
        var zoomDisplay = Driver.FindElementByAccessibilityId("ZoomDisplay");
        Assert.IsNotNull(zoomDisplay);
        StringAssert.Contains(zoomDisplay.Text, "100");
    }

    // ------------------------------------------------------------------
    //  Empty canvas state
    // ------------------------------------------------------------------

    [TestMethod]
    public void EmptyCanvas_DropHint_IsVisible()
    {
        var dropHint = Driver.FindElementByAccessibilityId("DropHintText");
        Assert.IsNotNull(dropHint);
        Assert.IsTrue(dropHint.Displayed);
        Assert.AreEqual("Drop images here", dropHint.Text);
    }

    // ------------------------------------------------------------------
    //  Pin / Unpin toggle
    // ------------------------------------------------------------------

    [TestMethod]
    public void PinToggle_PinsAndUnpins()
    {
        var pinButton = Driver.FindElementByAccessibilityId("PinButton");
        ActivateButton(pinButton);

        // WindowStyle change may recreate the HWND — reattach to be safe.
        // The implicit wait (5s) on FindElement handles any UI update delay.
        PinRefSession.ReattachToAppWindow();

        try
        {
            var unpinButton = Driver.FindElementByAccessibilityId("UnpinButton");
            Assert.IsNotNull(unpinButton);
            Assert.IsTrue(unpinButton.Displayed);

            // Unpin to restore the normal window state.
            ActivateButton(unpinButton);
            PinRefSession.ReattachToAppWindow();

            pinButton = Driver.FindElementByAccessibilityId("PinButton");
            Assert.IsTrue(pinButton.Displayed);
        }
        catch
        {
            try
            {
                PinRefSession.ReattachToAppWindow();
                var unpin = Driver.FindElementByAccessibilityId("UnpinButton");
                ActivateButton(unpin);
                PinRefSession.ReattachToAppWindow();
            }
            catch { /* already unpinned */ }

            throw;
        }
    }

    // ------------------------------------------------------------------
    //  Reset canvas
    // ------------------------------------------------------------------

    [TestMethod]
    public void ResetCanvas_ViaResetButton_ResetsZoomTo100Percent()
    {
        var resetButton = Driver.FindElementByAccessibilityId("ResetCanvasButton");
        ActivateButton(resetButton);

        var zoomDisplay = Driver.FindElementByAccessibilityId("ZoomDisplay");
        StringAssert.Contains(zoomDisplay.Text, "100");
    }
}
