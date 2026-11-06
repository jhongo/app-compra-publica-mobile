using Foundation;
using UIKit;

namespace CompraPublicCercaMobile;

[Register("SceneDelegate")]
public class SceneDelegate : MauiUISceneDelegate
{
    [Export("scene:openURLContexts:")]
    public void OpenUrlContexts(UIScene scene, NSSet<UIOpenUrlContext> urlContexts)
    {
        foreach (var context in urlContexts)
        {
            Google.SignIn.SignIn.SharedInstance.HandleUrl(context.Url);
        }
    }
}