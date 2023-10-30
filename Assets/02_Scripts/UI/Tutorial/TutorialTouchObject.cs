using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTouchObject : MonoBehaviour
{
    [SerializeField] private GameObject arrowObj;
    [SerializeField] private Button nextButton;
    public void SetData(int _tutoID, UnityEngine.Events.UnityAction _action)
    {
        var tutoButtons = FindObjectsOfType<TutorialTouchEvent>();
        var tutoEventButton = tutoButtons.FirstOrDefault(item => item.TutorialID == _tutoID);
        if (tutoEventButton != null)
        {
            arrowObj.transform.position = tutoEventButton.transform.position;
        }
        var tutoInfo = DataManager.Instance.GetTutorialInfoData(_tutoID);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(_action);
    }
}
