using System.Diagnostics;
using System.Drawing;

namespace Home.Pi.LightServer.Animations;

internal class ClockAnimation : Animation
{
    private static readonly TimeSpan COLOR_TRANSITION_LENGTH = TimeSpan.FromSeconds(5);
    private readonly ClockLayoutOptions layoutOptions;
    private readonly double brightness;
    private static readonly Dictionary<byte, int[]> digitSegmentMap = new()
    {
        { 0, new[] { 0, 1, 2, 3, 4, 5 } },
        { 1, new[] { 1, 2 } },
        { 2, new[] { 0, 1, 3, 4, 6 } },
        { 3, new[] { 0, 1, 2, 3, 6 } },
        { 4, new[] { 1, 2, 5, 6 } },
        { 5, new[] { 0, 2, 3, 5, 6 } },
        { 6, new[] { 0, 2, 3, 4, 5, 6 } },
        { 7, new[] { 0, 1, 2 } },
        { 8, new[] { 0, 1, 2, 3, 4, 5, 6 } },
        { 9, new[] { 0, 1, 2, 3, 5, 6 } },
    };

    public ClockAnimation(ILightController lightController, LightServerConfiguration options) : base(lightController)
    {
        this.layoutOptions = options.ClockLayout ?? throw new ArgumentNullException(nameof(options));

        this.brightness = options.MaxBrightness;

    }

    protected override async Task RunAnimation(CancellationToken cancellationToken = default)
    {
        // Update animations 10 times/s
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(.1));
        this.TurnOnLights();

        var startColor = Color.White.RandomOtherColor();
        var endColor = startColor.RandomOtherColor();
        var sw = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            if (sw.Elapsed > COLOR_TRANSITION_LENGTH)
            {
                startColor = endColor;
                endColor = startColor.RandomOtherColor();
                sw.Restart();
            }

            var color = startColor
                .GetTransitionStep(endColor, sw.Elapsed / COLOR_TRANSITION_LENGTH)
                .WithBrightness(this.brightness);

            // Reset all the digits to black
            this.ClearDigits();

            // Convert hour to 12 hour clock time
            var hour = ((now.Hour + 11) % 12) + 1;
            if (hour > 9)
            {
                this.SetDigit(0, 1, color);
            }
            this.SetDigit(1, (byte)(hour % 10), color);
            this.SetDigit(2, (byte)(now.Minute / 10), color);
            this.SetDigit(3, (byte)(now.Minute % 10), color);

            await this.lightController.Update();

            await timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    private void SetDigit(int digit, byte number, Color color)
    {
        // Find which segments to light up for that number
        var lights = digitSegmentMap[number]
            .SelectMany(s => this.layoutOptions.Digits[digit][s])
            .Select(i => (i, color))
            .ToArray();

        this.lightController.SetLights(lights);
    }

    private void ClearDigits()
    {
        var lights = this.layoutOptions
            .Digits
            .SelectMany(d => d)
            .SelectMany(s => s)
            .Select(i => (i, Color.Black))
            .ToArray();

        this.lightController.SetLights(lights);
    }

    private void TurnOnLights()
    {
        var color = Color.White.WithBrightness(this.brightness);
        var lights = this.layoutOptions
            .Lights
            .Select(i => (i, color))
            .ToArray();

        this.lightController.SetLights(lights);
    }
}