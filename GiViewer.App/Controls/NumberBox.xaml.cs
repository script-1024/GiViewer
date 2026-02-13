using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace GiViewer.App.Controls;

[ContentProperty("Value")]
public partial class NumberBox : TextBox
{
    public NumberBox() => InitializeComponent();
    static NumberBox()
        => DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));

    #region Properties
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set
        {
            bool isValid = Domain switch
            {
                ValueDomain.Real => double.IsNormal(value),
                ValueDomain.Integer => double.IsInteger(value),
                ValueDomain.Natural => value >= 0 && double.IsInteger(value),
                _ => false
            };
            if (!isValid) return;
            double clamped = Math.Clamp(value, MinValue, MaxValue);
            Text = clamped.ToString();
            SetValue(ValueProperty, clamped);
        }
    }

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public double MinValue
    {
        get => (double)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public ValueDomain Domain
    {
        get => (ValueDomain)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }
    #endregion

    #region DependencyProperties
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(NumberBox),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(
            nameof(MaxValue), typeof(double), typeof(NumberBox),
            new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(
            nameof(MinValue), typeof(double), typeof(NumberBox),
            new PropertyMetadata(double.NegativeInfinity));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step), typeof(double), typeof(NumberBox),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(
            nameof(Domain), typeof(ValueDomain), typeof(NumberBox),
            new PropertyMetadata(ValueDomain.Real));
    #endregion

    #region Regex
    [GeneratedRegex(@"^[+-]?(\d*\.?\d*|\d+[Ee][+-]?\d*)$")]
    private static partial Regex RealRegex();
    [GeneratedRegex(@"^[+-]?\d*$")]
    private static partial Regex IntegerRegex();
    [GeneratedRegex(@"^\+?\d*$")]
    private static partial Regex NaturalRegex();
    #endregion

    private bool IsInputValid(string text)
    {
        Regex regex = Domain switch
        {
            ValueDomain.Real => RealRegex(),
            ValueDomain.Integer => IntegerRegex(),
            _ => NaturalRegex()
        };
        return regex.IsMatch(text);
    }

    private string GetProposedText(string newInput)
    {
        string text = Text;
        int selectionStart = SelectionStart;
        int selectionLength = SelectionLength;
        if (selectionLength > 0) text = text.Remove(selectionStart, selectionLength);
        return text.Insert(selectionStart, newInput);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        string value = GetProposedText(e.Text);
        if (!IsInputValid(value)) e.Handled = true; // 阻止输入
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }

        string pasteText = (string)e.DataObject.GetData(DataFormats.Text);
        if (!IsInputValid(pasteText)) e.CancelCommand(); // 禁止粘贴
    }

    private void OnSubmit()
    {
        if (!TryGetValue(out double value)) value = double.Max(0, MinValue);
        Text = value.ToString();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        InputMethod.SetIsInputMethodEnabled(this, false);
        DataObject.AddPastingHandler(this, OnPaste);
        PreviewTextInput += OnPreviewTextInput;
        LostFocus += (s, e) => OnSubmit();
        KeyDown += (s, e) => { if (e.Key == Key.Enter) OnSubmit(); };
        Text = Value.ToString();

        (GetTemplateChild("UpSpinButton") as RepeatButton)?
            .Click += (s, e) => Increase();

        (GetTemplateChild("DownSpinButton") as RepeatButton)?
            .Click += (s, e) => Decrease();
    }

    public void Increase()
    {
        if (Value + Step < MaxValue) Value += Step;
        else Value = MaxValue;
    }

    public void Decrease()
    {
        if (Value - Step > MinValue) Value -= Step;
        else Value = MinValue;
    }

    public bool TryGetValue(out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(Text) || !double.TryParse(Text, out double val)) return false;
        if (Domain == ValueDomain.Real) return false;
        val = Math.Round(val, MidpointRounding.ToZero);
        switch (Domain)
        {
            case ValueDomain.Integer:
                value = (int)Math.Clamp(val, int.MinValue, int.MaxValue);
                return true;
            case ValueDomain.Natural:
                value = (int)Math.Clamp(val, 0, int.MaxValue);
                return true;
            default:
                return false;
        }
    }

    public bool TryGetValue(out float value)
        => float.TryParse(Text, out value);

    public bool TryGetValue(out double value)
        => double.TryParse(Text, out value);
}

public enum ValueDomain
{
    // 实数
    Real=0,

    // 整数
    Integer=1,

    // 自然数
    Natural=2
}
