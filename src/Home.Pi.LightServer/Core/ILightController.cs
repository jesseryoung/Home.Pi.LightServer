
using System.Drawing;

namespace Home.Pi.LightServer.Core;


internal interface ILightController
{
    void SetLight(int index, Color color);
    void SetLights((int index, Color color)[] lights);
    Task Update();
    void Clear();

    int TotalLights { get; }
}