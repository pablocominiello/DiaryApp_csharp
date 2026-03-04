using Android.App;
using Android.Content.PM;

namespace DiaryApp.Mobile;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainWindow = typeof(MainWindow),
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait)]
public class MainActivity : MauiAppCompatActivity
{
}