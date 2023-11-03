using System;
using Network;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UserInfo
{
    public string token;
    public string username;
    public string name;
}

[Serializable]
public class RequestSignInData
{
    public string username;
    public string password;

    public RequestSignInData(string newUsername, string newPassword)
    {
        username = newUsername;
        password = newPassword;
    }
}

public class NetworkTest : MonoBehaviour
{
    public async UniTask Login(RequestSignInData data)
    {
        string json = JsonUtility.ToJson(data);
        UserInfo info = await NetworkManager.SendToServer<UserInfo>("/login", SENDTYPE.POST, json);
        Debug.Log(info);
    }
}