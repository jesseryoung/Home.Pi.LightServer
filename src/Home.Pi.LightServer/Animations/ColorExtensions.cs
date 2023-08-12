using System.Drawing;

namespace Home.Pi.LightServer.Animations;

internal static class ColorExtensions
{

    public static Color WithBrightness(this Color color, double brightness)
    {
        return Color.FromArgb((int)(color.R * brightness), (int)(color.G * brightness), (int)(color.B * brightness));
    }

    private static readonly Color[] colors = new[] {
        Color.FromArgb(255, 0, 0),
        Color.FromArgb(255, 165, 0),
        Color.FromArgb(255, 255, 0),
        Color.FromArgb(0, 128, 0),
        Color.FromArgb(0, 0, 255),
        Color.FromArgb(75, 0, 130),
        Color.FromArgb(238, 130, 238),
    };

    public static Color GetTransitionStep(this Color start, Color end, double position)
    {
        if (position <= 0.0)
        {
            return start;
        }

        if (position >= 1.0)
        {
            return end;
        }

        // Basic linear transition from one color to another
        // At currentFrame = 0, each component should = StartColor
        // At currentFrame = transitionFrames, each component should 0 EndColor
        var r = (int)(((1.0 - position) * start.R) + (position * end.R));
        var g = (int)(((1.0 - position) * start.G) + (position * end.G));
        var b = (int)(((1.0 - position) * start.B) + (position * end.B));

        return Color.FromArgb(r, g, b);
    }


    public static Color[] Transition(this Color startColor, int transitionSteps)
    {
        var end = startColor.RandomOtherColor();

        return Enumerable
            .Range(0, transitionSteps)
            .Select(e => (double)e / transitionSteps)
            .Select(e => GetTransitionStep(startColor, end, e))
            .ToArray();
    }

    public static Color RandomOtherColor(this Color color)
    {
        var otherColors = colors.Where(c => c != color).ToArray();
        return otherColors[Random.Shared.Next(0, otherColors.Length)];
    }
}