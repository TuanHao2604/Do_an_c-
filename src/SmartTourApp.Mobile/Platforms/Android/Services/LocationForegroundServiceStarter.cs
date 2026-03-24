#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;

namespace SmartTourApp.Mobile.Platforms.Android.Services;

public static class LocationForegroundServiceStarter
{
    public static void Start()
    {
        var context = global::Android.App.Application.Context;
        var intent = new Intent(context, global::Java.Lang.Class.FromType(typeof(LocationForegroundService)));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(intent);
        }
        else
        {
            context.StartService(intent);
        }
    }

    public static void Stop()
    {
        var context = global::Android.App.Application.Context;
        var intent = new Intent(context, global::Java.Lang.Class.FromType(typeof(LocationForegroundService)));
        context.StopService(intent);
    }
}
#endif
