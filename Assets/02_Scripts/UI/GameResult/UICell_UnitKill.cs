using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICell_UnitKill : MonoBehaviour
{
    [SerializeField] private Image heroThumbnail;
    [SerializeField] private TextMeshProUGUI killCountText;
    private int slotIndex;
    private System.Action<int> touchAction;
    public void SetData(int _slotIndex, int _unitUID, System.Action<int> _touchAction)
    {
        slotIndex = _slotIndex;
        touchAction = _touchAction;
        var heroBattleData = UserData.Instance.GetBattleHeroData(_unitUID);
        heroThumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(heroBattleData.refData.thumbnailpath);
        killCountText.SetText(heroBattleData.killCount.ToString());
    }
}
