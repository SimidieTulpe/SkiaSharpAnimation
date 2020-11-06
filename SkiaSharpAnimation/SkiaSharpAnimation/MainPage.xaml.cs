using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkiaSharpAnimation
{
    public partial class MainPage : ContentPage
    {
        private const float INITIAL_LENGTH = 1;
        private const float INITIAL_HEIGHT = 0;
        private const float WIND_FREQUENCY = 0.5f; //Hz
        private const float ANIMATION_FREQUENCY = 20f; //Hz
        private const float DISPLAY_USAGE = 0.15f;
        private float randomDeltaX = 0;
        private float randomDeltaY = 0;
        private float angle = 0;
        private int numberOfBranches = 0;
        private int randomFactor = 0;
        private int windForce = 0;
        private bool numberOfBranchesChanged;
        private bool randomFactorChanged;
        private bool windForceChanged;
        private bool pageIsActive;
        private SKPaint groundPaint;
        private SKPaint branchPaint;
        private SKPaint leavesPaint;
        private readonly Random rnd = new Random();

        public MainPage()
        {
            InitializeComponent();
            DefineColors();
        }

        private void DefineColors()
        {
            groundPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.DarkGreen.ToSKColor(),
                StrokeWidth = 50
            };

            branchPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color.Brown.ToSKColor()
            };

            leavesPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color.Olive.ToSKColor()
            };
        }

        public int NumberOfBranches
        {
            get => numberOfBranches;
            set
            {
                if (value == numberOfBranches) return;
                numberOfBranches = value;
                OnPropertyChanged(nameof(NumberOfBranches));
                canvasView.InvalidateSurface();
                numberOfBranchesChanged = true;
            }
        }

        public int RandomFactor
        {
            get => randomFactor;
            set
            {
                if (value == randomFactor) return;
                randomFactor = value;
                OnPropertyChanged(nameof(RandomFactor));
                canvasView.InvalidateSurface();
                randomFactorChanged = true;
            }
        }

        public int WindForce
        {
            get => windForce;
            set
            {
                if (value == windForce) return;
                windForce = value;
                OnPropertyChanged(nameof(WindForce));
                canvasView.InvalidateSurface();
                windForceChanged = true;
            }
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!(WindForce != 0 || numberOfBranchesChanged || randomFactorChanged || windForceChanged)) return;

            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            canvas.DrawLine(0, info.Height, info.Width, info.Height, groundPaint);

            DrawTree(NumberOfBranches, -0.5f * INITIAL_LENGTH, INITIAL_HEIGHT, 0.5f * INITIAL_LENGTH, INITIAL_HEIGHT, info, canvas);
        }

        private void DrawTree(int remainingIterations, float XStart, float YStart, float XEnd, float YEnd, SKImageInfo info, SKCanvas canvas)
        {
            if (remainingIterations <= 0)
            {
                numberOfBranchesChanged = false;
                randomFactorChanged = false;
                windForceChanged = false;
                return;
            }

            var x1 = XStart - (YEnd - YStart);
            var x2 = XStart;
            var x3 = XEnd;
            var x4 = XEnd - (YEnd - YStart);

            var y1 = YStart + (XEnd - XStart);
            var y2 = YStart;
            var y3 = YEnd;
            var y4 = YEnd + (XEnd - XStart);

            var sideLength = (float)Math.Sqrt(Math.Pow(1f / 2f * (x4 - x1) - 1f / 2f * (y4 - y1), 2) + Math.Pow(1f / 2f * (y4 - y1) + 1f / 2f * (x4 - x1), 2));

            if (randomFactorChanged)
            {
                randomDeltaX = (float)Math.Cos(rnd.NextDouble() * 2 * Math.PI) * sideLength * (float)RandomFactor / 150f;
                randomDeltaY = (float)Math.Sin(rnd.NextDouble() * 2 * Math.PI) * sideLength * (float)RandomFactor / 150f;
                randomFactorChanged = false;
            }

            var displacement = (float)Math.Sin(angle * WIND_FREQUENCY) * (float)WindForce / 150f;

            var x5 = x1 + 1f / 2f * (x4 - x1) - 1f / 2f * (y4 - y1) + randomDeltaX + displacement;
            var y5 = y1 + 1f / 2f * (y4 - y1) + 1f / 2f * (x4 - x1) + randomDeltaY;

            DrawPath(x1, x2, x3, x4, x5, y1, y2, y3, y4, y5, info, canvas, remainingIterations);

            DrawTree(remainingIterations - 1, x1, y1, x5, y5, info, canvas);
            DrawTree(remainingIterations - 1, x5, y5, x4, y4, info, canvas);
        }

        private void DrawPath(float x1, float x2, float x3, float x4, float x5, float y1, float y2, float y3, float y4, float y5, SKImageInfo info, SKCanvas canvas, int remainingIterations)
        {
            SKPath path = new SKPath();
            path.MoveTo((0.5f + x1 * DISPLAY_USAGE) * info.Width, (1f - y1 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x2 * DISPLAY_USAGE) * info.Width, (1f - y2 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x3 * DISPLAY_USAGE) * info.Width, (1f - y3 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x4 * DISPLAY_USAGE) * info.Width, (1f - y4 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x1 * DISPLAY_USAGE) * info.Width, (1f - y1 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x5 * DISPLAY_USAGE) * info.Width, (1f - y5 * DISPLAY_USAGE) * info.Height);
            path.LineTo((0.5f + x4 * DISPLAY_USAGE) * info.Width, (1f - y4 * DISPLAY_USAGE) * info.Height);
            path.Close();

            if (remainingIterations <= 1) canvas.DrawPath(path, leavesPaint);
            else canvas.DrawPath(path, branchPaint);
        }

        async Task AnimationLoop()
        {
            Device.StartTimer(TimeSpan.FromSeconds(1f / ANIMATION_FREQUENCY), () =>
              {
                  canvasView.InvalidateSurface();
                  angle += (2f * (float)Math.PI / ANIMATION_FREQUENCY) % (2f * (float)Math.PI);
                  return pageIsActive;
              });

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            pageIsActive = true;
            AnimationLoop();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            pageIsActive = false;
        }
    }
}
