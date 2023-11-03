using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;

public class LocalTimeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CheckTime().Forget();
    }

    private async UniTask CheckTime()
    {
        using (var request = UnityWebRequest.Get("http://google.com"))
        {
            var webRequest = await request.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"result.isNetworkError");
            }
            else
            {
                string date = request.GetResponseHeader("date");
                Debug.Log($"date {date}");


                DateTime localTime = DateTime.Parse(date).ToLocalTime();
                DateTime minight = new DateTime(localTime.Year, localTime.Month, localTime.Day + 1);
                Debug.Log($"Korea {localTime}");
                Debug.Log($"Korea midnight {minight}");

                var localTimeSpan = minight.Subtract(localTime);

                Debug.Log($"time left {localTimeSpan}");
            }

        }

    }
}
