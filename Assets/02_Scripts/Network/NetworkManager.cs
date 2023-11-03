using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public enum SENDTYPE
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class NetworkManager
    {
        protected static double timeout = 5; //5초 타임아웃.
        public static string DOMAIN = "";
        /// <summary>
        /// 서버 전송 함수.
        /// </summary>
        /// <typeparam name="T">Return Class</typeparam>
        /// <param name="url">api url 정보</param>
        /// <param name="sendType">Get,Post,Put,Delete</param>
        /// <param name="jsonBody">body 정보</param>
        /// <returns></returns>
        public static async UniTask<T> SendToServer<T>(string url, SENDTYPE sendType, string jsonBody = null)
        {
            //1. 네트워크 체크.
            await CheckNetwork();
            //2. API URL 생성.
            string requestURL = DOMAIN + url;

            //3. Timeout 설정.
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

            //4. 웹 요청 생성(Get,Post,Delete,Update)
            UnityWebRequest request = new UnityWebRequest(requestURL, sendType.ToString());
            //5. Body 정보 입력
            request.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            //6. Header 정보 입력
            SetHeaders(request);
            try
            {
                var res = await request.SendWebRequest().WithCancellation(cts.Token);
                T result = JsonUtility.FromJson<T>(res.downloadHandler.text);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == cts.Token)
                {
                    Debug.Log("Timeout");
                    //TODO: 네트워크 재시도 팝업 호출.

                    //재시도.
                    return await SendToServer<T>(url, sendType, jsonBody);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return default;
            }
            return default;
        }

        private static async UniTask CheckNetwork()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //TODO: 네트워크 오류 팝업 호출.
                Debug.LogError("The network is not connected.");
                await UniTask.WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
                Debug.Log("The network is connected.");
            }
        }

        private static void SetHeaders(UnityWebRequest request)
        {
            //필요한 Header 추가.
            request.SetRequestHeader("Content-Type", "application/json");
        }

        public static async UniTask<string> GetNetworkTime()
        {
            using (var request = UnityWebRequest.Get("http://google.com"))
            {
                var webRequest = await request.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"result.isNetworkError");
                    return string.Empty;
                }
                else
                {
                    return request.GetResponseHeader("date");
                    
                }

            }
        }

    }
}