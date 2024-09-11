using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Messaging;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthManager : Singleton<AuthManager>
{
    private FirebaseAuth auth;
    private bool initialized = false;

    public UniTaskCompletionSource initialize = new UniTaskCompletionSource();

    public void Initialize()
    {
        if (!initialized)
            return;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                InitializeFirebase();
                //InitializeApple();
                //InitializeGoogle();
                initialize.TrySetResult();
            }
            else
            {
                Debug.LogErrorFormat("[Firebase] Could not resolve all Firebase dependencies: {0}", task.Result);
            }
        });

        initialized = true;
    }

    private void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");

        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnStateChanged;
        auth.IdTokenChanged += OnIdTokenChanged;
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseMessaging.GetTokenAsync().AsUniTask()
            .ContinueWith(x =>
            {
                Debug.LogFormat("[Firebase] FirebaseMessaging Token: {0}", x);
                //pushToken.TrySetResult(x);
            }).Forget();
    }
    void OnStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (sender == null)
            return;

        Debug.LogFormat("[Firebase/OnStateChanged] Sender : {0}", sender.ToString());
    }

    void OnIdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        if (sender == null)
            return;

        Debug.LogFormat("[Firebase/OnIdTokenChanged] Sender : {0}", sender.ToString());
    }
}
