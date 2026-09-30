using System.Windows;

namespace BimBlaster.Vergas;

public partial class VergasWindow : Window
{
    public bool UseRectangle => ModeWindowRadio.IsChecked == true;

    public VergasWindow(AppLanguage lang)
    {
        Resources.MergedDictionaries.Add(StyleLoader.LoadBimBlasterStyles());
        InitializeComponent();

        string T(string k) => LocalizedStrings.Get(lang, k);
        Title = T("WindowTitle");
        TitleText.Text = T("WindowTitle");
        SelectionLabel.Text = T("SelectionLabel");
        ModeWindowText.Text = T("ModeWindow");
        ModeWindowHint.Text = T("ModeWindowHint");
        ModePickText.Text = T("ModePick");
        ModePickHint.Text = T("ModePickHint");
        CancelButton.Content = T("Cancel");
        RunButton.Content = T("Run");
    }

    private void Run_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}