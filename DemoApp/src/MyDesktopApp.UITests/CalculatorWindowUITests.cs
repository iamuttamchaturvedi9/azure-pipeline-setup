using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using NUnit.Framework;

namespace MyDesktopApp.UITests;

[TestFixture]
public class CalculatorWindowUITests
{
    private Application? _app;
    private UIA3Automation? _automation;
    private Window? _mainWindow;

    [SetUp]
    public void Launch()
    {
        // Runs against the compiled app published by the Build stage and
        // downloaded onto the interactive agent by the pipeline's UI test job.
        var exePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyDesktopApp.exe");

        _app = Application.Launch(exePath);
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
    }

    [TearDown]
    public void Cleanup()
    {
        _app?.Close();
        _automation?.Dispose();
    }

    [Test]
    public void AddingTwoNumbers_DisplaysCorrectResult()
    {
        var firstNumberBox = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("FirstNumberTextBox")).AsTextBox();
        var secondNumberBox = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("SecondNumberTextBox")).AsTextBox();
        var addButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("AddButton")).AsButton();
        var resultText = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("ResultTextBlock")).AsLabel();

        firstNumberBox.Text = "4";
        secondNumberBox.Text = "5";
        addButton.Invoke();

        Assert.That(resultText.Text, Is.EqualTo("Result: 9"));
    }
}
