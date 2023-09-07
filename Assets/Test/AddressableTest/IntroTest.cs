using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroTest : MonoBehaviour
{

    public void OnClickGoToMainScene()
    {
        SceneManager.LoadScene("AddressableTest");
    }


}
