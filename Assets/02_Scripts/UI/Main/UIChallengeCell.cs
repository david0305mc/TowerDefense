using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengeCell : MonoBehaviour
{
    [SerializeField] Button startButton;
         
    public void SetData(int _stageID, System.Action _startAction)
    {
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            _startAction?.Invoke();
        });
    }
}
