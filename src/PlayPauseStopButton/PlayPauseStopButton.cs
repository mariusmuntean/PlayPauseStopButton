using System;
using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace PlayPauseStopButton
{
    public partial class PlayPauseStopButton : SKCanvasView
    {
        private const string MainAnimationName = "MainAnimation";

        private SKPaint _symbolPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        SKPaint _backgroundPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        private float _interpolationValue;
        private float _backgroundOpacity;
        private double _animationProgress;

        private SKPath _startSymbolPathA;
        private SKPath _startSymbolPathB;
        private SKPath _endSymbolPathA;
        private SKPath _endSymbolPathB;

        private float _dragDistance = 0.0f;
        private SKPoint _previousTouchLocation;
        private Stopwatch _gestureSW = new Stopwatch();
        private long _gestureDurationMillis = 0;
        private bool _gestureCompleted = false;
        private SKPath _playA;
        private SKPath _playB;
        private SKPath _pauseA;
        private SKPath _pauseB;
        private SKPath _stopA;
        private SKPath _stopB;

        public PlayPauseStopButton()
        {
            CurrentMode = DisplayMode.PlayPause;
            CurrentState = State.Paused;

            _interpolationValue = 1.0f;

            this.EnableTouchEvents = true;
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            var w = double.IsInfinity(widthConstraint) ? double.MaxValue : widthConstraint;
            var h = double.IsInfinity(heightConstraint) ? double.MaxValue : heightConstraint;

            var smallerDimension = Math.Min(w, h);

            return new SizeRequest(new Size(smallerDimension, smallerDimension));
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            // The docs mention this
            base.OnPaintSurface(e);

            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;
            canvas.Clear();

            // Compute a square that will always fit inside the available area
            var smallerDimension = Math.Min(info.Rect.Width, info.Rect.Height);
            var aspectRatioRect = new SKRect(
                info.Rect.MidX - (smallerDimension * 0.5f),
                info.Rect.MidY - (smallerDimension * 0.5f),
                info.Rect.MidX + (smallerDimension * 0.5f),
                info.Rect.MidY + (smallerDimension * 0.5f)
            );

            // Draw background
            _backgroundPaint.Color = _backgroundPaint.Color.WithAlpha((byte) (_backgroundOpacity * 255));
            canvas.DrawCircle(aspectRatioRect.MidX, aspectRatioRect.MidY, 0.5f * aspectRatioRect.Width, _backgroundPaint);

            // Compute the symbol area - the paths will be drawn inside this shape
            var symbolRect = new SKRect(
                aspectRatioRect.MidX - (aspectRatioRect.Width * 0.5f * 0.5f),
                aspectRatioRect.MidY - (aspectRatioRect.Height * 0.5f * 0.5f),
                aspectRatioRect.MidX + (aspectRatioRect.Width * 0.5f * 0.5f),
                aspectRatioRect.MidY + (aspectRatioRect.Height * 0.5f * 0.5f)
            );

            // ToDo: maybe cache the values and recompute only when needed
            RecomputeSymbolPaths(symbolRect);
            ReAssignStartEndPaths();

            // Insurance - in case the start/stop symbol paths are not set then fall back to something
            _startSymbolPathA = _startSymbolPathA ?? _playA;
            _startSymbolPathB = _startSymbolPathB ?? _playB;
            _endSymbolPathA = _endSymbolPathA ?? _pauseA;
            _endSymbolPathB = _endSymbolPathB ?? _pauseB;

            // Get interpolated paths from start to stop
            var interpolationA = new SKPathInterpolation(_startSymbolPathA, _endSymbolPathA);
            var interpolationB = new SKPathInterpolation(_startSymbolPathB, _endSymbolPathB);

            // Draw the interpolated paths
            canvas.DrawPath(interpolationA.Interpolate(_interpolationValue), _symbolPaint);
            canvas.DrawPath(interpolationB.Interpolate(_interpolationValue), _symbolPaint);

            // The docs mention this
            canvas.Flush();
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Entered:
                    break;
                case SKTouchAction.Pressed:
                    _gestureCompleted = false;
                    _gestureDurationMillis = 0;
                    _dragDistance = 0.0f;

                    _previousTouchLocation = e.Location;
                    _gestureSW = Stopwatch.StartNew();
                    break;
                case SKTouchAction.Moved:
                    _dragDistance += SKPoint.Distance(e.Location, _previousTouchLocation);
                    _previousTouchLocation = e.Location;
                    break;
                case SKTouchAction.Released:
                    _gestureDurationMillis = _gestureSW.ElapsedMilliseconds;
                    _gestureCompleted = true;
                    break;
                case SKTouchAction.Cancelled:
                    break;
                case SKTouchAction.Exited:
                    break;
                case SKTouchAction.WheelChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_gestureCompleted && _dragDistance < 50.0f && _gestureDurationMillis <= 120)
            {
                // Tap!
                Console.WriteLine($"Tap: distance={_dragDistance}px duration={_gestureDurationMillis}ms");

                OnTap();
            }

            e.Handled = true;
        }

        private void OnTap()
        {
            var previousState = CurrentState;

            // Switch state
            CurrentState = (CurrentMode, CurrentState) switch
            {
                (DisplayMode.PlayPause, State.Paused) => State.Playing,
                (DisplayMode.PlayPause, State.Stopped) => State.Playing,
                (DisplayMode.PlayPause, State.Playing) => State.Paused,
                (DisplayMode.PlayStop, State.Paused) => State.Playing,
                (DisplayMode.PlayStop, State.Stopped) => State.Playing,
                (DisplayMode.PlayStop, State.Playing) => State.Stopped
            };

            // Launch animation
            LaunchAnimation();

            // Invoke click handler
            Clicked?.Invoke(this, new PlayPauseStopButtonEventArgs(CurrentMode, previousState, CurrentState));

            // Execute Command
            Command?.Execute(null);
        }

        private void LaunchAnimation()
        {
            this.AbortAnimation(MainAnimationName);

            var shapeInterpolationAnimation = new Animation(d =>
                {
                    _interpolationValue = (float) d;
                    InvalidateSurface();
                },
                0.0d, 1.0d, Easing.CubicInOut);

            var backgroundFadeInAnimation = new Animation(d => { _backgroundOpacity = (float) d; }, 0.0d, 0.35d);
            var backgroundFadeOutAnimation = new Animation(d => { _backgroundOpacity = (float) d; }, _backgroundOpacity, 0.0d);

            var mainAnimation = new Animation();
            mainAnimation.Add(0.0d, 0.6d, shapeInterpolationAnimation);
            mainAnimation.Add(0.0d, 0.5d, backgroundFadeInAnimation);
            mainAnimation.Add(0.5d, 1.0d, backgroundFadeOutAnimation);

            mainAnimation.Commit(this, MainAnimationName, 1000 / 60, 500);
        }

        private void RecomputeSymbolPaths(SKRect symbolRect)
        {
            // play a - a trapeze on its side
            _playA = new SKPath();
            _playA.MoveTo(symbolRect.Left, symbolRect.Top);
            var shortEdgeStartX = symbolRect.MidX;
            var shortEdgeStartY = symbolRect.Top + symbolRect.Height * 0.25f;
            _playA.LineTo(shortEdgeStartX, shortEdgeStartY);

            var shortEdgeEndX = symbolRect.MidX;
            var shortEdgeEndY = symbolRect.Bottom - symbolRect.Height * 0.25f;
            _playA.LineTo(shortEdgeEndX, shortEdgeEndY);

            _playA.LineTo(symbolRect.Left, symbolRect.Bottom);
            _playA.Close();


            // play b - a small triangle on its side
            _playB = new SKPath();
            var upperVertexX = symbolRect.MidX;
            var upperVertexY = symbolRect.Top + symbolRect.Height * 0.25f;
            _playB.MoveTo(upperVertexX, upperVertexY);

            _playB.LineTo(symbolRect.Right, symbolRect.Top + 0.5f * symbolRect.Height);

            var lowerVertexX = symbolRect.MidX;
            var lowerVertexY = symbolRect.Bottom - symbolRect.Height * 0.25f;
            _playB.LineTo(lowerVertexX, lowerVertexY);

            _playB.Close();

            // pause a - a rectangle; full height and a third of the width
            _pauseA = new SKPath();

            _pauseA.MoveTo(symbolRect.Left, symbolRect.Top);
            _pauseA.LineTo(symbolRect.Left + 0.33f * symbolRect.Width, symbolRect.Top);
            _pauseA.LineTo(symbolRect.Left + 0.33f * symbolRect.Width, symbolRect.Bottom);
            _pauseA.LineTo(symbolRect.Left, symbolRect.Bottom);
            _pauseA.Close();

            // pause b - another rectangle; full height and the last third of the width
            _pauseB = new SKPath();
            _pauseB.MoveTo(symbolRect.Left + 0.66f * symbolRect.Width, symbolRect.Top);
            _pauseB.LineTo(symbolRect.Right, symbolRect.Top);
            _pauseB.LineTo(symbolRect.Right, symbolRect.Bottom);
            _pauseB.LineTo(symbolRect.Left + 0.66f * symbolRect.Width, symbolRect.Bottom);
            _pauseB.Close();

            // stop a - half of a square
            _stopA = new SKPath();
            _stopA.MoveTo(symbolRect.MidX - symbolRect.Width * 0.25f, symbolRect.MidY - symbolRect.Height * 0.25f);
            _stopA.LineTo(symbolRect.MidX, symbolRect.MidY - symbolRect.Height * 0.25f);
            _stopA.LineTo(symbolRect.MidX, symbolRect.MidY + symbolRect.Height * 0.25f);
            _stopA.LineTo(symbolRect.MidX - symbolRect.Width * 0.25f, symbolRect.MidY + symbolRect.Height * 0.25f);
            _stopA.Close();

            // stop a - second of a square
            _stopB = new SKPath();
            _stopB.MoveTo(symbolRect.MidX, symbolRect.MidY - symbolRect.Height * 0.25f);
            _stopB.LineTo(symbolRect.MidX, symbolRect.MidY + symbolRect.Height * 0.25f);
            _stopB.LineTo(symbolRect.MidX + symbolRect.Width * 0.25f, symbolRect.MidY + symbolRect.Height * 0.25f);
            _stopB.LineTo(symbolRect.MidX + symbolRect.Width * 0.25f, symbolRect.MidY - symbolRect.Height * 0.25f);
            _stopB.Close();
        }

        private void ReAssignStartEndPaths()
        {
            (_startSymbolPathA, _startSymbolPathB, _endSymbolPathA, _endSymbolPathB)
                = (CurrentMode, CurrentState) switch
                {
                    (DisplayMode.PlayPause, State.Paused) => (_pauseA, _pauseB, _playA, _playB),
                    (DisplayMode.PlayPause, State.Stopped) => (_pauseA, _pauseB, _playA, _playB),
                    (DisplayMode.PlayPause, State.Playing) => (_playA, _playB, _pauseA, _pauseB),
                    (DisplayMode.PlayStop, State.Paused) => (_stopA, _stopB, _playA, _playB),
                    (DisplayMode.PlayStop, State.Stopped) => (_stopA, _stopB, _playA, _playB),
                    (DisplayMode.PlayStop, State.Playing) => (_playA, _playB, _stopA, _stopB)
                };
        }
    }
}