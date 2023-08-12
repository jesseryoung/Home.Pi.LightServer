using System.Device.Spi;
using System.Drawing;
using Iot.Device.Ws28xx;

namespace Home.Pi.LightServer.Led;

internal class Ws2812bLightController : ILightController, IDisposable
{
    private bool disposedValue;
    private readonly SpiDevice spi;
    private readonly Ws2812b lightStrip;

    public int TotalLights { get; }

    public Ws2812bLightController(LightServerConfiguration options)
    {
        var settings = new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 2_400_000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        };

        this.spi = SpiDevice.Create(settings);
        this.TotalLights = options.TotalLights;
        this.lightStrip = new Ws2812b(this.spi, options.TotalLights);
    }

    public void Clear()
    {
        this.lightStrip.Image.Clear();
    }

    public void SetLight(int index, Color color)
    {
        this.lightStrip.Image.SetPixel(index, 0, color);
    }

    public void SetLights((int index, Color color)[] lights)
    {
        foreach (var (i, c) in lights)
        {
            this.SetLight(i, c);
        }
    }

    public Task Update()
    {
        this.lightStrip.Update();
        return Task.CompletedTask;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.Clear();
                this.spi.Dispose();
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}