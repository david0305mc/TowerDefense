using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TutorialTouchObject : MonoBehaviour
{
    [SerializeField] private GameObject arrowObj;
    [SerializeField] private Button nextButton;

    private CancellationTokenSource cts;

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
    public void SetData(int _tutoID, UnityEngine.Events.UnityAction _action)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        var tutoButtons = FindObjectsOfType<TutorialTouchEvent>();
        TutorialTouchEvent tutoEventButton = tutoButtons.FirstOrDefault(item => item.TutorialID == _tutoID);
        if (tutoEventButton != null)
        {
            arrowObj.transform.position = new Vector3(-3000, -3000, 0);

            UniTask.Create(async () =>
            {
                while (true)
                {
                    await UniTask.Yield(cancellationToken: cts.Token);
                    arrowObj.transform.position = tutoEventButton.GetOffsetPosition();
                }
            });
        }
        else
        {
            Debug.LogError($"There is no TutoEventButton {_tutoID}");
        }

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(_action);
    }
}
