namespace PinRef.AutoTest;

/// <summary>
/// Manages WinAppDriver lifetime once for the entire test assembly,
/// so individual test classes can create and tear down app sessions
/// without restarting the driver between classes.
/// </summary>
[TestClass]
public static class GlobalSetup
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext _) => PinRefSession.StartWinAppDriver();

    [AssemblyCleanup]
    public static void AssemblyCleanup() => PinRefSession.StopWinAppDriver();
}
