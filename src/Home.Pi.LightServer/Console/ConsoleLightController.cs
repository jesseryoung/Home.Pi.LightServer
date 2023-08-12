using DrawingColor = System.Drawing.Color;
using Spectre.Console;

namespace Home.Pi.LightServer.Console;


internal class ConsoleLightController : ILightController
{
    // Based off of config/lightserver.console.json
    private const int CLOCK_WIDTH = 49, CLOCK_HEIGHT = 17;
    private readonly DrawingColor[] lights;
    private readonly LightServerConfiguration options;

    public ConsoleLightController(LightServerConfiguration options)
    {
        this.options = options;
        this.lights = new DrawingColor[options.TotalLights];
    }

    public int TotalLights => this.lights.Length;

    public void Clear()
    {
        Array.Clear(this.lights);
    }

    public void SetLight(int index, DrawingColor color)
    {
        this.lights[index] = color;
    }

    public void SetLights((int index, DrawingColor color)[] lights)
    {
        foreach (var (i, c) in lights)
        {
            this.SetLight(i, c);
        }
    }

    public Task Update()
    {
        if (this.options.LayoutType == LayoutType.Clock)
        {
            this.DrawClock();
        }
        return Task.CompletedTask;
    }
    private void DrawClock()
    {
        var canvas = new Canvas(CLOCK_WIDTH, CLOCK_HEIGHT);

        // lightserver.console.json was written such that pixels go from left to right top to bottom
        var i = 0;
        for (var y = 0; y < CLOCK_HEIGHT; y++)
        {
            for (var x = 0; x < CLOCK_WIDTH; x++)
            {
                canvas.SetPixel(x, y, new Color(this.lights[i].R, this.lights[i].G, this.lights[i].B));
                i++;
            }
        }
        AnsiConsole.Clear();
        AnsiConsole.Write(canvas);
    }

}