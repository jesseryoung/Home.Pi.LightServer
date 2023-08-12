namespace Home.Pi.LightServer.Animations;

internal abstract class Animation
{
    protected readonly ILightController lightController;
    public Animation(ILightController lightController)
    {
        this.lightController = lightController;
    }

    public async Task Animate(CancellationToken cancellationToken = default)
    {
        this.lightController.Clear();
        await this.lightController.Update();
        try
        {
            await this.RunAnimation(cancellationToken);
        }
        finally
        {
            this.lightController.Clear();
            await this.lightController.Update();
        }
    }

    protected abstract Task RunAnimation(CancellationToken cancellationToken = default);
}
