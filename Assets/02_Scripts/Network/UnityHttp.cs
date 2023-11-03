using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class ResponseData
{
    readonly byte[] bytes;
    public Dictionary<string, string> ResponseHeaders { get; }
    public T GetResult<T>() => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));

    public ResponseData(byte[] bytes, Dictionary<string, string> responseHeaders)
    {
        this.bytes = bytes;
        this.ResponseHeaders = responseHeaders;
    }
}

public static class UnityHttp
{
    private static readonly int timeout = 30;
    private static readonly int retryTimeout = 8;
    private static bool retryChecking = false;
    private static double retryLastTime;

    private static double retryElapsedTime => GameTime.Get() - retryLastTime;
    private static bool isRetryTimeOut => retryElapsedTime > retryTimeout;

    //ResponseHeader 정보가 필요한 경우에 사용        
    public static async UniTask<ResponseData> GetData(string url,
        Dictionary<string, string> headers = null,
        IProgress<float> progress = null,
        CancellationToken cancellationToken = default)
    {
        RETRY:
        using (UnityWebRequest req = UnityWebRequest.Get(URLAntiCacheRandomizer(url)))
        {
            req.timeout = timeout;

            if (headers != null)
            {
                foreach (var header in headers)
                    req.SetRequestHeader(header.Key, header.Value);
            }

            try
            {
                await req.SendWebRequest().ToUniTask(progress, cancellationToken: cancellationToken);
#if ENABLE_HTTP_LOG
                Debug.LogFormat("[UnityHttp/Get/Recv] {0}", req.downloadHandler.text);
#endif
                return new ResponseData(req.downloadHandler.data, req.GetResponseHeaders());
            }
            catch (UnityWebRequestException e)
            {
                Debug.LogErrorFormat("[UnityHttp] {0}", e);
                await CheckRetryTimeout(cancellationToken);
                goto RETRY;
            }
        }
    }

    static async UniTask CheckRetryTimeout(CancellationToken cancellationToken)
    {
        await UniTask.WaitWhile(() => retryChecking, cancellationToken: cancellationToken);

        if (!isRetryTimeOut)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2.0), cancellationToken: cancellationToken);
            return;
        }

        try
        {
            retryChecking = true;
            Debug.LogError("Retry");
            //await MessageDispather.PublishAsync(EMessage.Confirm, Localization.Get("system_error_check_network"), cancellationToken);
        }
        finally
        {
            retryChecking = false;
            retryLastTime = GameTime.Get();
        }
    }

    public static string URLAntiCacheRandomizer(string url)
    {
        string r = "";
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        r += UnityEngine.Random.Range(1000000, 8000000).ToString();
        return url + "?p=" + r;
    }

    public static string RandomId8Bytes()
    {
        byte[] arr = new byte[4];
        for (int idx = 0; idx < arr.Length; ++idx)
        {
            int i = UnityEngine.Random.Range(0, 256);
            arr[idx] = (byte)i;
        }
        return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", arr[0], arr[1], arr[2], arr[3]).ToLower();
    }


}
