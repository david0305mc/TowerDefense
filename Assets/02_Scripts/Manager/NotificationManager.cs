
#if UNITY_ANDROID
using System;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android;
#else

#endif

public class NotificationManager : Singleton<NotificationManager>
{
#if UNITY_ANDROID
    public void RequestAuthorization()
    {
        try
        {
            string androidInfo = SystemInfo.operatingSystem;
            int apiLevel = int.Parse(androidInfo.Substring(androidInfo.IndexOf("-") + 1, 3), System.Globalization.CultureInfo.InvariantCulture);
            Debug.Log("apiLevel: " + apiLevel);

            if (33 <= apiLevel && !UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void RegisterNotificationChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "devil_Channel",
            Name = "Devil_Invasion",
            Importance = Importance.Default,
            Description = "Generic notifications"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    public void SendNotification(string _title, string _text, int _fireTimeInSeconds)
    {
        var notification = new AndroidNotification();
        notification.Title = _title;
        notification.Text = _text;
        notification.FireTime = System.DateTime.Now.AddSeconds(_fireTimeInSeconds);
        notification.LargeIcon = "icon_1";
        notification.SmallIcon = "icon_0";

        AndroidNotificationCenter.SendNotification(notification, channelId: "devil_Channel");
    }
    public void SendNotification(string _title, string _text, System.DateTime _dateTime)
    {
        var notification = new AndroidNotification();
        notification.Title = _title;
        notification.Text = _text;
        notification.FireTime = _dateTime;
        notification.LargeIcon = "icon_1";
        notification.SmallIcon = "icon_0";
        AndroidNotificationCenter.SendNotification(notification, channelId: "devil_Channel");
    }

    public void FlushNotifications()
    {
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        AndroidNotificationCenter.CancelAllNotifications();
    }

    public void Initialize()
    {
        AndroidNotificationCenter.Initialize();
    
    }
#else
 public void RequestAuthorization()
    {

    }

    public void RegisterNotificationChannel()
    {

    }

    public void SendNotification(string _title, string _text, int _fireTimeInSeconds)
    {
    }
#endif
}
