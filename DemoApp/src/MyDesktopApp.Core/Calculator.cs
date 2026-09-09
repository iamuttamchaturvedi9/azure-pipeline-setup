namespace MyDesktopApp.Core;

/// <summary>
/// Simple calculator used by the desktop app UI.
/// Kept in a separate class library so it can be unit tested
/// independently of any WPF/UI code.
/// </summary>
public class Calculator
{
    public double Add(double a, double b) => a + b;

    public double Subtract(double a, double b) => a - b;

    public double Multiply(double a, double b) => a * b;

    public double Divide(double a, double b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        return a / b;
    }
}
