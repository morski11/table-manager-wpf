using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TablesWPF.Controls;

public partial class QuantitySpinner : UserControl
{
    private static readonly Regex _digits = new("^[0-9]+$");

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(QuantitySpinner),
        new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(int), typeof(QuantitySpinner), new PropertyMetadata(1));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(int), typeof(QuantitySpinner), new PropertyMetadata(int.MaxValue));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public QuantitySpinner()
    {
        InitializeComponent();
        PART_TextBox.Text = Value.ToString();
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var ctrl = (QuantitySpinner)d;
        var v = (int)baseValue;
        if (v < ctrl.Minimum) return ctrl.Minimum;
        if (v > ctrl.Maximum) return ctrl.Maximum;
        return v;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (QuantitySpinner)d;
        ctrl.PART_TextBox.Text = ctrl.Value.ToString();
    }

    private void PART_Up_Click(object sender, RoutedEventArgs e)
    {
        if (Value < Maximum) Value++;
    }

    private void PART_Down_Click(object sender, RoutedEventArgs e)
    {
        if (Value > Minimum) Value--;
    }

    private void PART_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !_digits.IsMatch(e.Text);
    }

    private void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(PART_TextBox.Text, out var v))
        {
            if (v < Minimum) v = Minimum;
            if (v > Maximum) v = Maximum;
            if (v != Value) Value = v;
        }
        else
        {
            // if empty or invalid, keep current Value shown
            PART_TextBox.Text = Value.ToString();
            PART_TextBox.CaretIndex = PART_TextBox.Text.Length;
        }
    }
}
