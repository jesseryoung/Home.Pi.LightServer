namespace Home.Pi.LightServer.Core;

internal class LightServerConfiguration
{
    public const string ENVIRONMENT_VARIABLE = "LIGHTSERVER_CONFIG";
    public required int TotalLights { get; init; }
    public required double MaxBrightness { get; init; }
    public required LayoutType LayoutType { get; init; }
    public ClockLayoutOptions? ClockLayout { get; init; }

}

internal class ClockLayoutOptions
{
    public required int[] Lights { get; init; }
    public required int[][][] Digits { get; init; }
}

internal enum LayoutType
{
    Invalid = 0,
    Clock = 1
}