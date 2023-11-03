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
        UnityWebRequest request = new UnityWebRequest();
        using (request = UnityWebRequest.Get("www.google.com"))
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
                DateTime dateTime = DateTime.Parse(date).ToLocalTime();
                Debug.Log($"Korea {dateTime}");
            }

        }

    }
}
