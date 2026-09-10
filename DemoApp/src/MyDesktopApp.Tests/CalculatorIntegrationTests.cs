using MyDesktopApp.Core;
using MyDesktopApp.Utilities;
using NUnit.Framework;

namespace MyDesktopApp.Tests;

[TestFixture]
public class CalculatorIntegrationTests
{
    [Test]
    public void CalculateAndFormatResult_ReturnsExpectedOutput()
    {
        var calculator = new Calculator();

        var result = calculator.Add(10, 5);

        var formattedResult = ResultFormatter.FormatResult(result);

        Assert.That(formattedResult, Is.EqualTo("Result: 15"));
    }
}