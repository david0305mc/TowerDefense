using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIUnitCell : MonoBehaviour
{
    [SerializeField] private Image iconBGImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkerObject;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private UnitGradeInfo unitGradeInfo;
    [SerializeField] private Button button;

    public void SetData(int _index, int _uid, System.Action<int> _action)
    {
        var heroData = UserData.Instance.GetHeroData(_uid);
        checkerObject.SetActive(UserData.Instance.GetPartySlotIndexByUID(heroData.uid) != -1);
        iconImage.sprite = MResourceManager.Instance.GetSpriteFromAtlas(heroData.refData.thumbnailpath);
        iconBGImage.sprite = MResourceManager.Instance.GetBuildAtlas($"RatingBG_{(int)heroData.refData.unitrarity}");
        unitGradeInfo.SetData(heroData.grade, heroData.IsMaxGrade, heroData.count, heroData.refUnitGradeData.upgradepiececnt);

        if (UserData.Instance.LocalData.CurrTutorialID == 14)
        {
            if(heroData.uid == UserData.Instance.LocalData.TutorialSpawnedUnitUID)
            {
                UniTask.Create(async () =>
                {
                    await UniTask.Yield();
                    TutorialTouchEvent obj = iconImage.AddComponent<TutorialTouchEvent>();
                    obj.TutorialID = 14;
                    MGameManager.Instance.PlayNextTutorial();
                });
            }
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            _action?.Invoke(_index);
        });
    }


}
