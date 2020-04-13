using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace PlayPauseStop
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            SymbolColorTxt.TextChanged += SymbolColorTextChanged;
            HighlightColorTxt.TextChanged += BackgroundHighlightColorTextChanged;
            PlayPauseStopBtn.PropertyChanged += OnPPSBtnPropertyChanged;
            YTSwitch.Toggled += YTToggled;
            RefreshUI();

            ModePicker.SelectedItem = PlayPauseStopBtn.CurrentMode;
            StatePicker.SelectedItem = PlayPauseStopBtn.CurrentState;
        }

        private void YTToggled(object sender, ToggledEventArgs e)
        {
            PlayPauseStopBtn.BackgroundColor = YTSwitch.IsToggled ? Color.FromHex("#c4302b") : Color.Transparent;
        }

        private void SymbolColorTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SymbolColorTxt.Text.Length != 9)
                {
                    return;
                }

                var newSymbolColor = Color.FromHex(SymbolColorTxt.Text);
                PlayPauseStopBtn.SymbolColor = newSymbolColor;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void BackgroundHighlightColorTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (HighlightColorTxt.Text.Length != 9)
                {
                    return;
                }

                var newBackgroundHighlightColor = Color.FromHex(HighlightColorTxt.Text);
                PlayPauseStopBtn.BackgroundHighlightColor = newBackgroundHighlightColor;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void RefreshUI()
        {
            SymbolColorTxt.Text = PlayPauseStopBtn.SymbolColor.ToHex();
            SymbolColorTxt.TextColor = PlayPauseStopBtn.SymbolColor;

            HighlightColorTxt.Text = PlayPauseStopBtn.BackgroundHighlightColor.ToHex();
            HighlightColorTxt.TextColor = PlayPauseStopBtn.BackgroundHighlightColor;
        }

        private void OnPPSBtnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshUI();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
