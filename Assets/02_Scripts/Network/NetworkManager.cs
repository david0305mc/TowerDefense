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
        protected static double timeout = 5; //5�� Ÿ�Ӿƿ�.
        public static string DOMAIN = "";
        /// <summary>
        /// ���� ���� �Լ�.
        /// </summary>
        /// <typeparam name="T">Return Class</typeparam>
        /// <param name="url">api url ����</param>
        /// <param name="sendType">Get,Post,Put,Delete</param>
        /// <param name="jsonBody">body ����</param>
        /// <returns></returns>
        public static async UniTask<T> SendToServer<T>(string url, SENDTYPE sendType, string jsonBody = null)
        {
            //1. ��Ʈ��ũ üũ.
            await CheckNetwork();
            //2. API URL ����.
            string requestURL = DOMAIN + url;

            //3. Timeout ����.
            var cts = new CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

            //4. �� ��û ����(Get,Post,Delete,Update)
            UnityWebRequest request = new UnityWebRequest(requestURL, sendType.ToString());
            //5. Body ���� �Է�
            request.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            //6. Header ���� �Է�
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
                    //TODO: ��Ʈ��ũ ��õ� �˾� ȣ��.

                    //��õ�.
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
                //TODO: ��Ʈ��ũ ���� �˾� ȣ��.
                Debug.LogError("The network is not connected.");
                await UniTask.WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
                Debug.Log("The network is connected.");
            }
        }

        private static void SetHeaders(UnityWebRequest request)
        {
            //�ʿ��� Header �߰�.
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