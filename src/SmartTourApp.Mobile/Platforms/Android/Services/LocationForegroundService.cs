#if ANDROID
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Speech.Tts;
using AndroidX.Core.App;
using Java.Util;
using System.Globalization;
using Microsoft.Maui.Storage;
using SmartTourApp.Mobile.Models;
using SmartTourApp.Mobile.Services;

namespace SmartTourApp.Mobile.Platforms.Android.Services;

[Service(Name = "com.smarttour.app.LocationForegroundService",
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation,
    Exported = false)]
public class LocationForegroundService : Service, ILocationListener, global::Android.Speech.Tts.TextToSpeech.IOnInitListener
{
    private const int NotificationId = 7001;
    private const string ChannelId = "smarttour_location_channel";
    private LocationManager? _locationManager;
    private readonly LocalDbService _localDb = new();
    private readonly Dictionary<string, DateTime> _lastTrigger = new();
    private global::Android.Speech.Tts.TextToSpeech? _tts;
    private bool _ttsReady;
    private string? _pendingSpeech;

    public override void OnCreate()
    {
        base.OnCreate();
        CreateNotificationChannel();
        _tts = new global::Android.Speech.Tts.TextToSpeech(this, this);
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        StartForeground(NotificationId, BuildNotification("Dang theo doi vi tri"));
        StartLocationUpdates();
        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        StopLocationUpdates();
        if (_tts is not null)
        {
            _tts.Stop();
            _tts.Shutdown();
            _tts.Dispose();
            _tts = null;
        }
        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    public void OnLocationChanged(global::Android.Locations.Location location)
    {
        _ = HandleLocationAsync(location);
    }

    public void OnProviderDisabled(string provider) { }
    public void OnProviderEnabled(string provider) { }
    public void OnStatusChanged(string? provider, [GeneratedEnum] Availability status, Bundle? extras) { }
    public void OnInit([GeneratedEnum] OperationResult status)
    {
        if (status != OperationResult.Success || _tts is null)
        {
            _ttsReady = false;
            return;
        }

        _ttsReady = true;
        var locale = global::Java.Util.Locale.ForLanguageTag("vi-VN") ?? global::Java.Util.Locale.Default;
        _tts.SetLanguage(locale);
        if (!string.IsNullOrWhiteSpace(_pendingSpeech))
        {
            SpeakText(_pendingSpeech);
            _pendingSpeech = null;
        }
    }

    private void StartLocationUpdates()
    {
        try
        {
            _locationManager = (LocationManager?)GetSystemService(LocationService);
            if (_locationManager is null)
            {
                return;
            }

            if (_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                _locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 15000, 5, this);
            }

            if (_locationManager.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                _locationManager.RequestLocationUpdates(LocationManager.NetworkProvider, 30000, 10, this);
            }
        }
        catch
        {
            // Ignore if permission not granted
        }
    }

    private void StopLocationUpdates()
    {
        try
        {
            _locationManager?.RemoveUpdates(this);
        }
        catch
        {
            // ignore
        }
    }

    private async Task HandleLocationAsync(global::Android.Locations.Location location)
    {
        if (!Preferences.Get("tracking_enabled", false))
        {
            return;
        }

        var routeSessionId = Preferences.Get("route_session_id", string.Empty);
        var cellKey = BuildCellKey(location.Latitude, location.Longitude);
        await _localDb.LogLocationAsync(new LocalLocationLog
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            CellKey = cellKey,
            RouteSessionId = routeSessionId,
            LoggedAt = DateTime.UtcNow
        });

        var pois = await _localDb.GetAllPoisAsync();
        if (pois.Count == 0)
        {
            return;
        }

        var inRange = pois
            .Select(p => new
            {
                Poi = p,
                DistanceKm = GeoUtils.HaversineKm(location.Latitude, location.Longitude, p.Latitude, p.Longitude)
            })
            .Where(x => x.Poi.GeofenceRadius > 0 && x.DistanceKm * 1000 <= x.Poi.GeofenceRadius)
            .OrderBy(x => x.DistanceKm)
            .FirstOrDefault();

        if (inRange is null)
        {
            return;
        }

        var lastTime = _lastTrigger.TryGetValue(inRange.Poi.Id, out var value) ? value : DateTime.MinValue;
        if (DateTime.UtcNow - lastTime < TimeSpan.FromMinutes(10))
        {
            return;
        }

        _lastTrigger[inRange.Poi.Id] = DateTime.UtcNow;
        await _localDb.LogVisitAsync(new LocalVisitLog
        {
            PoiId = inRange.Poi.Id,
            PoiName = inRange.Poi.Name,
            VisitedAt = DateTime.UtcNow,
            NarrationSeconds = null,
            TriggeredByQr = false
        });

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.Notify(NotificationId + 1, BuildNotification($"Ban da vao khu vuc: {inRange.Poi.Name}"));
        SpeakText(inRange.Poi.Description ?? inRange.Poi.Name);
    }

    private Notification BuildNotification(string message)
    {
        return new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("SmartTour")
            .SetContentText(message)
            .SetSmallIcon(global::Android.Resource.Drawable.IcDialogMap)
            .SetOngoing(true)
            .Build();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var channel = new NotificationChannel(ChannelId, "SmartTour Tracking", NotificationImportance.Low)
        {
            Description = "Theo doi vi tri va geofence"
        };

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.CreateNotificationChannel(channel);
    }

    private static string BuildCellKey(double lat, double lng)
    {
        var latKey = Math.Round(lat, 2).ToString("0.00", CultureInfo.InvariantCulture);
        var lngKey = Math.Round(lng, 2).ToString("0.00", CultureInfo.InvariantCulture);
        return $"{latKey},{lngKey}";
    }

    private void SpeakText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (_tts is null || !_ttsReady)
        {
            _pendingSpeech = text;
            return;
        }

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            _tts.Speak(text, QueueMode.Flush, null, Guid.NewGuid().ToString());
        }
        else
        {
            _tts.Speak(text, QueueMode.Flush, null);
        }
    }
}
#endif
