using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UIMain : SingletonMono<UIMain>
{
    [SerializeField] private UIDamageText uiDamageTextPref;

    [SerializeField] private Canvas worldCanvas;

    [SerializeField] private RectTransform damageObj;
    [SerializeField] private TextMeshProUGUI damageText;

    private CancellationTokenSource cts;

    public void SetUIWorldPosToCameraPos(RectTransform ui, Vector2 _worldPos)
    {
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        // convert screen coords
        Vector2 adjustedPosition = worldCanvas.worldCamera.WorldToScreenPoint(_worldPos);
        adjustedPosition.x *= canvasRect.rect.width / (float)worldCanvas.worldCamera.pixelWidth;
        adjustedPosition.y *= canvasRect.rect.height / (float)worldCanvas.worldCamera.pixelHeight;
        ui.anchoredPosition = adjustedPosition - canvasRect.sizeDelta / 2f;
    }

    public void ShowDamageText(Vector2 _pos, int _damage)
    {
        UIDamageText damageText = Lean.Pool.LeanPool.Spawn(uiDamageTextPref, _pos, Quaternion.identity, worldCanvas.transform);
        damageText.SetData(_damage);
    }
    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
