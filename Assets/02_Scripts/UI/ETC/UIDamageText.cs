using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UIDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    private RectTransform rectTransform;
    private CancellationTokenSource cts;
    public RectTransform GetRectTransform => rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void SetData(int _damage)
    {
        damageText.text = _damage.ToString();
        cts?.Cancel();
        cts = new CancellationTokenSource();
        UniTask.Create(async () =>
        {
            await UniTask.Delay(10000, cancellationToken:cts.Token);
            Lean.Pool.LeanPool.Despawn(gameObject);
        });
    }
    private void OnDisable()
    {
        cts?.Cancel();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
