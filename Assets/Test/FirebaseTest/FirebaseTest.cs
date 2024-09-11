using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Cysharp.Threading.Tasks;

public class FirebaseTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputfiendID;
    [SerializeField] private TMP_InputField inputfiendPassword;
    [SerializeField] private TextMeshProUGUI gpgsText;
    [SerializeField] private TextMeshProUGUI firebaseText;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private bool isSignIn;
    private void Awake()
    {
        isSignIn = false;
        InitializeGPGS();
        InitializeFirebase();
    }

    void InitializeGPGS()
    {
        var config = new PlayGamesClientConfiguration.Builder();
        PlayGamesPlatform.InitializeInstance(
            config.RequestIdToken()
            .RequestEmail()
            .Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    private async UniTask SignInGPGS()
    {
        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        Social.localUser.Authenticate(ret =>
        {
            if (!ret)
            {
                ucs.TrySetCanceled();
                gpgsText.text = "Failed";
                return;
            }

            ucs.TrySetResult();
            
        });
        await ucs.Task;
        gpgsText.text = $"Sucess {name}";
        return;
        //PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, _status =>
        //{
        //    if (_status == SignInStatus.Success)
        //    {
        //        string name = PlayGamesPlatform.Instance.GetUserDisplayName();
        //        string id = PlayGamesPlatform.Instance.GetUserId();
        //        gpgsText.text = $"Sucess {name}";
        //        StartCoroutine(TryFirebaseLogin()); // Firebase Login �õ�
        //    }
        //    else
        //    {

        //        gpgsText.text = "Failed";
        //    }
        //});
    }
    private void SignOut()
    {
        if (Social.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.SignOut();
            auth.SignOut();
        }
    }
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                isSignIn = false;
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                isSignIn = true;
                Debug.Log("Signed in " + user.UserId);
                //displayName = user.DisplayName ?? "";
                //emailAddress = user.Email ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    public void OnClickBtnSignIn()
    {
        if (string.IsNullOrEmpty(inputfiendID.text))
        {
            Debug.LogError("empty inputfiendID");
            return;
        }
        if (string.IsNullOrEmpty(inputfiendPassword.text))
        {
            Debug.LogError("empty inputfiendPassword");
            return;
        }
        SignInFirebase(inputfiendID.text, inputfiendPassword.text);
    }
    public void OnClickBtnSignUP()
    {
        if (string.IsNullOrEmpty(inputfiendID.text))
        {
            Debug.LogError("empty inputfiendID");
            return;
        }
        if (string.IsNullOrEmpty(inputfiendPassword.text))
        {
            Debug.LogError("empty inputfiendPassword");
            return;
        }
        SignUpFirebase(inputfiendID.text, inputfiendPassword.text);
    }

    public void SignUpFirebase(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
    public void SignInFirebase(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }
    IEnumerator TryFirebaseLogin()
    {
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
            yield return null;
        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                firebaseText.SetText("SignInWithCredentialAsync was canceled!!");
                return;
            }
            if (task.IsFaulted)
            {
                firebaseText.SetText("SignInWithCredentialAsync encountered an error: " + task.Exception);

                return;
            }
            user = task.Result;

        });
        
        
    }

    public void OnClickBtnGPGSLogin()
    {
        SignInGPGS();
    }
}


