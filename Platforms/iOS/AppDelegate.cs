using Foundation;
using UIKit;

namespace CompraPublicCercaMobile;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        if (NSBundle.MainBundle.PathForResource("GoogleService-Info", "plist") is not null)
        {
            Firebase.Core.App.Configure();
        }

        return base.FinishedLaunching(application, launchOptions);
    }
}