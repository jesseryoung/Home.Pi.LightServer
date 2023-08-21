using System.Diagnostics;
using System.Drawing;

namespace Home.Pi.LightServer.Animations;


internal class LightBarAnimation : Animation
{
    private readonly LightServerConfiguration configuration;

    public LightBarAnimation(ILightController lightController, LightServerConfiguration configuration) : base(lightController)
    {
        this.configuration = configuration;
    }

    protected override async Task RunAnimation(CancellationToken cancellationToken = default)
    {
        var startColor = Color.White.RandomOtherColor();
        var endColor = startColor.RandomOtherColor();
        var transitions = startColor
            .Transition(endColor, this.lightController.TotalLights)
            .Select((c, i) => (i, c.WithBrightness(this.configuration.MaxBrightness)))
            .ToArray();

        await this.LightRunAnimation(transitions, cancellationToken);
        await this.BreathAnimation(transitions, cancellationToken);

        this.lightController.SetLights(transitions);
        await this.lightController.Update();

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task LightRunAnimation((int light, Color color)[] transitions, CancellationToken cancellationToken)
    {
        var transitionTime = TimeSpan.FromSeconds(1);
        var sw = Stopwatch.StartNew();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(.05));

        while (sw.Elapsed < transitionTime && !cancellationToken.IsCancellationRequested)
        {
            var lightsToLight = (int)(sw.Elapsed / transitionTime * transitions.Length);
            var lastLightBrightness = (sw.Elapsed / transitionTime * transitions.Length) - lightsToLight;
            this.lightController.SetLights(transitions[..lightsToLight]);
            this.lightController.SetLight(lightsToLight, transitions[lightsToLight].color.WithBrightness(lastLightBrightness));

            await this.lightController.Update();
            await timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    private async Task BreathAnimation((int light, Color color)[] transitions, CancellationToken cancellationToken)
    {
        const double minBreathBrightness = 0.25;

        var animationLength = TimeSpan.FromSeconds(3);
        var timePerBreath = TimeSpan.FromSeconds(1);
        var sw = Stopwatch.StartNew();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(.05));

        while (!cancellationToken.IsCancellationRequested && sw.Elapsed < animationLength)
        {
            var elapsed = sw.Elapsed;
            var x = elapsed / timePerBreath * 2 * Math.PI;
            var brightness = ((Math.Cos(x) + 1.0) / 2.0 * (1 - minBreathBrightness)) + minBreathBrightness;
            this.lightController.SetLights(transitions.Select(light => (light.light, light.color.WithBrightness(brightness))).ToArray());
            await this.lightController.Update();

            await timer.WaitForNextTickAsync(cancellationToken);

        }
    }
}