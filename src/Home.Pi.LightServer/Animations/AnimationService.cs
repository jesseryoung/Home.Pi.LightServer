namespace Home.Pi.LightServer.Animations;

internal class AnimationService : BackgroundService
{
    private readonly AnimationController animationController;

    public AnimationService(AnimationController animationController)
    {
        this.animationController = animationController;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this.animationController.Run(stoppingToken);
    }
}