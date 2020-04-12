using SkiaSharp;
using SkiaSharp.Extended;
using SkiaSharp.Views.Forms;

namespace PlayPauseStopButton
{
    public class PlayPauseStopButton : SKCanvasView
    {
        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.DarkCyan
        };

        SKPaint fillPaint2 = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.LightCyan
        };

        private float _interpolationValue;

        public PlayPauseStopButton()
        {
            InterpolationValue = 0.0f;
        }

        public float InterpolationValue
        {
            get => _interpolationValue;
            set
            {
                _interpolationValue = value;
                this.InvalidateSurface();
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;
            var surface = e.Surface;
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.DarkGray);

            // Symbol area
            // ToDo: don't use info.Rect - instead use a computed square to maintain aspect ratio
            var symbolRect = new SKRect(
                info.Rect.MidX - (info.Rect.Width * 0.66f * 0.5f),
                info.Rect.MidY - (info.Rect.Height * 0.66f * 0.5f),
                info.Rect.MidX + (info.Rect.Width * 0.66f * 0.5f),
                info.Rect.MidY + (info.Rect.Height * 0.66f * 0.5f)
            );
            canvas.DrawRect(symbolRect, new SKPaint()
            {
                Color = SKColors.Gold,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke
            });

            // play a - a trapeze on its side
            var playA = new SKPath();
            playA.MoveTo(symbolRect.Left, symbolRect.Top);
            var shortEdgeStartX = symbolRect.MidX;
            var shortEdgeStartY = symbolRect.Top + symbolRect.Height * 0.25f;
            playA.LineTo(shortEdgeStartX, shortEdgeStartY);

            var shortEdgeEndX = symbolRect.MidX;
            var shortEdgeEndY = symbolRect.Bottom - symbolRect.Height * 0.25f;
            playA.LineTo(shortEdgeEndX, shortEdgeEndY);

            playA.LineTo(symbolRect.Left, symbolRect.Bottom);
            playA.Close();


            // play b - a small triangle on its side
            var playB = new SKPath();
            var upperVertexX = symbolRect.MidX;
            var upperVertexY = symbolRect.Top + symbolRect.Height * 0.25f;
            playB.MoveTo(upperVertexX, upperVertexY);

            playB.LineTo(symbolRect.Right, symbolRect.Top + 0.5f * symbolRect.Height);

            var lowerVertexX = symbolRect.MidX;
            var lowerVertexY = symbolRect.Bottom - symbolRect.Height * 0.25f;
            playB.LineTo(lowerVertexX, lowerVertexY);

            playB.Close();

            // pause a
            var pauseA = new SKPath();

            pauseA.MoveTo(symbolRect.Left, symbolRect.Top);
            pauseA.LineTo(symbolRect.Left + 0.33f * symbolRect.Width, symbolRect.Top);
            pauseA.LineTo(symbolRect.Left + 0.33f * symbolRect.Width, symbolRect.Bottom);
            pauseA.LineTo(symbolRect.Left, symbolRect.Bottom);
            pauseA.Close();

            // pause b
            var pauseB = new SKPath();
            pauseB.MoveTo(symbolRect.Left + 0.66f * symbolRect.Width, symbolRect.Top);
            pauseB.LineTo(symbolRect.Right, symbolRect.Top);
            pauseB.LineTo(symbolRect.Right, symbolRect.Bottom);
            pauseB.LineTo(symbolRect.Left + 0.66f * symbolRect.Width, symbolRect.Bottom);
            pauseB.Close();

            // Get interpolation
            var interpolationA = new SKPathInterpolation(playA, pauseA);
            var interpolationB = new SKPathInterpolation(playB, pauseB);

            canvas.DrawPath(interpolationA.Interpolate(_interpolationValue), fillPaint);
            canvas.DrawPath(interpolationB.Interpolate(_interpolationValue), fillPaint);
        }
    }
}