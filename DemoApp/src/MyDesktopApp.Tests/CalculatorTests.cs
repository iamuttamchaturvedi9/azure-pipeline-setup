using MyDesktopApp.Core;
using NUnit.Framework;

namespace MyDesktopApp.Tests;

[TestFixture]
public class CalculatorTests
{
    private Calculator _calculator = null!;

    [SetUp]
    public void SetUp()
    {
        _calculator = new Calculator();
    }

    [Test]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        Assert.That(_calculator.Add(2, 3), Is.EqualTo(5));
    }

    [Test]
    public void Subtract_ReturnsDifference()
    {
        Assert.That(_calculator.Subtract(5, 3), Is.EqualTo(2));
    }

    [TestCase(2, 3, 6)]
    [TestCase(-2, 3, -6)]
    [TestCase(0, 5, 0)]
    public void Multiply_ReturnsExpectedResult(double a, double b, double expected)
    {
        Assert.That(_calculator.Multiply(a, b), Is.EqualTo(expected));
    }

    [Test]
    public void Divide_ByZero_ThrowsException()
    {
        Assert.Throws<DivideByZeroException>(() => _calculator.Divide(1, 0));
    }

    [Test]
    public void Divide_ValidNumbers_ReturnsQuotient()
    {
        Assert.That(_calculator.Divide(10, 2), Is.EqualTo(5));
    }
}
