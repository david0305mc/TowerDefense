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
    public void SetData(int _slotIndex, int _uid, System.Action<int> _touchAction)
    {
        try
        {
            slotIndex = _slotIndex;
            touchAction = _touchAction;
            var heroData = UserData.Instance.GetHeroData(_uid);
            int killCount = UserData.Instance.GetBattleKillCount(_uid);
            heroThumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(heroData.refData.thumbnailpath);
            killCountText.SetText(killCount.ToString());
        }
        catch
        {
            Debug.LogError("error");
        }
        
    }
}
