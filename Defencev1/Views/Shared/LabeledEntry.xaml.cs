namespace Defencev1.Views.Shared;

public partial class LabeledEntry : Grid
{
	public LabeledEntry()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty LabelTextProperty =
       BindableProperty.Create(
           nameof(LabelText),
           typeof(string),
           typeof(LabeledEntry),
           string.Empty);

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public static readonly BindableProperty EntryTextProperty =
        BindableProperty.Create(
            nameof(EntryText),
            typeof(string),
            typeof(LabeledEntry),
            string.Empty,
            BindingMode.TwoWay);

    public string EntryText
    {
        get => (string)GetValue(EntryTextProperty);
        set => SetValue(EntryTextProperty, value);
    }
}