
namespace Home.Pi.LightServer.Animations;

internal class AnimationController
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<AnimationController> logger;
    private TaskCompletionSource newAnimationAvailable = new();
    private Type? currentAnimationType;

    public AnimationController(IServiceProvider serviceProvider, ILogger<AnimationController> logger, LightServerConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;

        this.currentAnimationType = configuration.LayoutType switch
        {
            LayoutType.Clock => typeof(ClockAnimation),
            LayoutType.LightBar => typeof(LightBarAnimation),
            LayoutType.Invalid => typeof(LightBarAnimation),
            _ => typeof(LightBarAnimation)
        };
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var animationType = this.currentAnimationType;
            if (animationType != null)
            {
                // Create a new CTS and tie it to the cancellationToken for the animation controller
                using var cts = new CancellationTokenSource();
                using var reg = cancellationToken.Register(cts.Cancel);
                using var scope = this.serviceProvider.CreateAsyncScope();

                this.logger.LogInformation("Starting animation: {}", animationType.FullName);
                try
                {
                    // Create the animation
                    var animation = (Animation)scope.ServiceProvider.GetRequiredService(animationType);

                    var animationTask = animation.Animate(cts.Token);

                    await Task.WhenAny(animationTask, this.newAnimationAvailable.Task.WaitAsync(cts.Token));
                    cts.Cancel();
                    // Await to raise any exceptions
                    await animationTask;
                }
                // Ignore exceptions that come from the CTS be canceled
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    // Log any exceptions but allow the controller to continue to function
                    this.logger.LogError(ex, "Error while running animation {}", animationType.FullName);
                }
                finally
                {
                    // Make sure all tasks are cancelled
                    cts.Cancel();
                }
            }

            this.logger.LogInformation("Waiting for new animation...");
            await this.newAnimationAvailable.Task.WaitAsync(cancellationToken);
            this.newAnimationAvailable = new();
        }
    }

    public void StartAnimation(Type animationType)
    {
        this.currentAnimationType = animationType;
        this.newAnimationAvailable.SetResult();
    }


    public void StopAnimation()
    {
        this.currentAnimationType = null;
        this.newAnimationAvailable.SetResult();
    }
}