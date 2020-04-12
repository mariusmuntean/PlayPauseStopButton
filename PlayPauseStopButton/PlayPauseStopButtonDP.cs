using System;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace PlayPauseStopButton
{
    public partial class PlayPauseStopButton
    {
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            if (propertyName == BackgroundHighlightColorProperty.PropertyName)
            {
                _backgroundPaint.Color = BackgroundHighlightColor.ToSKColor();
                InvalidateSurface();
            }

            if (propertyName == SymbolColorProperty.PropertyName)
            {
                _symbolPaint.Color = SymbolColor.ToSKColor();
                InvalidateSurface();
            }

            if (propertyName == CurrentModeProperty.PropertyName)
            {
                _currentMode = CurrentMode;
            }

            if (propertyName == CurrentStateProperty.PropertyName)
            {
                _currentState = CurrentState;
            }

            // Do the animation just once
            if (propertyName == CurrentModeProperty.PropertyName || propertyName == CurrentStateProperty.PropertyName)
            {
                (_endSymbolPathA, _endSymbolPathB) = (CurrentMode, CurrentState) switch
                {
                    (DisplayMode.PlayPause, State.Paused) => (_pauseA, _pauseB),
                    (DisplayMode.PlayPause, State.Stopped) => (_stopA, _stopB),
                    (DisplayMode.PlayPause, State.Playing) => (_playA, _playB),
                    (DisplayMode.PlayStop, State.Paused) => (_pauseA, _pauseB),
                    (DisplayMode.PlayStop, State.Stopped) => (_stopA, _stopB),
                    (DisplayMode.PlayStop, State.Playing) => (_playA, _playB)
                };

                LaunchAnimation();
            }
        }

        public static readonly BindableProperty CurrentModeProperty = BindableProperty.Create(
            nameof(CurrentMode),
            typeof(DisplayMode),
            typeof(PlayPauseStopButton),
            DisplayMode.PlayPause,
            BindingMode.TwoWay
        );

        public DisplayMode CurrentMode
        {
            get => (DisplayMode) GetValue(CurrentModeProperty);
            set => SetValue(CurrentModeProperty, value);
        }

        public static readonly BindableProperty CurrentStateProperty = BindableProperty.Create(
            nameof(CurrentState),
            typeof(State),
            typeof(PlayPauseStopButton),
            State.Paused,
            BindingMode.TwoWay
        );

        public State CurrentState
        {
            get => (State) GetValue(CurrentStateProperty);
            set => SetValue(CurrentStateProperty, value);
        }


        public static readonly BindableProperty SymbolColorProperty = BindableProperty.Create(
            nameof(SymbolColor),
            typeof(Color),
            typeof(PlayPauseStopButton),
            Color.White,
            BindingMode.OneWay
        );

        public Color SymbolColor
        {
            get => (Color) GetValue(SymbolColorProperty);
            set => SetValue(SymbolColorProperty, value);
        }


        public static readonly BindableProperty BackgroundHighlightColorProperty = BindableProperty.Create(
            nameof(BackgroundHighlightColor),
            typeof(Color),
            typeof(PlayPauseStopButton),
            Color.White,
            BindingMode.OneWay
        );

        public Color BackgroundHighlightColor
        {
            get => (Color) GetValue(BackgroundHighlightColorProperty);
            set => SetValue(BackgroundHighlightColorProperty, value);
        }

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(Command),
            typeof(PlayPauseStopButton),
            null,
            BindingMode.OneWay
        );

        public Command Command
        {
            get => (Command) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public event EventHandler<PlayPauseStopButtonEventArgs> Clicked;
    }
}