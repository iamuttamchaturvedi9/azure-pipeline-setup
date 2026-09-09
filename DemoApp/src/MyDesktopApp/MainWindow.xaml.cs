using System.Windows;
using MyDesktopApp.Core;
using MyDesktopApp.Utilities;

namespace MyDesktopApp;

public partial class MainWindow : Window
{
    private readonly Calculator _calculator = new();

    public MainWindow()
    {
        InitializeComponent();
    }

     private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(FirstNumberTextBox.Text, out var first) &&
            double.TryParse(SecondNumberTextBox.Text, out var second))
        {
            var result = _calculator.Add(first, second);
            ResultTextBlock.Text = ResultFormatter.FormatResult(result);
        }
        else
        {
            ResultTextBlock.Text = ResultFormatter.FormatError("please enter valid numbers");
        }
    }
}
