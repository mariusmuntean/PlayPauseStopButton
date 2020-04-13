namespace PlayPauseStopButton
{
    public class PlayPauseStopButtonEventArgs
    {
        public DisplayMode DisplayMode { get; }
        public State PreviousState { get; }
        public State NewState { get; }

        public PlayPauseStopButtonEventArgs(DisplayMode displayMode, State previousState, State newState)
        {
            DisplayMode = displayMode;
            PreviousState = previousState;
            NewState = newState;
        }
    }
}