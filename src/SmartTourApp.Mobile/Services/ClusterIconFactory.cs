using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;

#if ANDROID
using Android.Graphics;
#endif

namespace SmartTourApp.Mobile.Services;

public static class ClusterIconFactory
{
    private static readonly ConcurrentDictionary<int, byte[]> Cache = new();

    public static ImageSource GetClusterIcon(int count)
    {
#if ANDROID
        var key = count > 99 ? 99 : count;
        var bytes = Cache.GetOrAdd(key, CreatePng);
        return ImageSource.FromStream(() => new MemoryStream(bytes));
#else
        return "pin_cluster.svg";
#endif
    }

    private static byte[] CreatePng(int count)
    {
#if ANDROID
        const int size = 96;
        const float center = size / 2f;

        using var bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
        using var canvas = new Canvas(bitmap);
        canvas.DrawARGB(0, 0, 0, 0);

        using var paint = new global::Android.Graphics.Paint(global::Android.Graphics.PaintFlags.AntiAlias);
        paint.Color = new Android.Graphics.Color(243, 194, 68, 255);
        canvas.DrawCircle(center, center, 32, paint);

        paint.Color = new Android.Graphics.Color(15, 23, 42, 255);
        canvas.DrawCircle(center, center, 24, paint);

        paint.Color = new Android.Graphics.Color(243, 194, 68, 255);
        canvas.DrawCircle(center, center, 10, paint);

        var label = count >= 99 ? "99+" : count.ToString(CultureInfo.InvariantCulture);
        paint.Color = Android.Graphics.Color.White;
        paint.TextSize = 26f;
        paint.TextAlign = global::Android.Graphics.Paint.Align.Center;
        var y = center - (paint.Descent() + paint.Ascent()) / 2;
        canvas.DrawText(label, center, y, paint);

        using var ms = new MemoryStream();
        bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
        return ms.ToArray();
#endif
    }
}
