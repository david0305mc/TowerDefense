using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;

public class WorldMapStageSlot : MonoBehaviour
{
    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject battleObject;
    [SerializeField] private GameObject occupationObject;
    [SerializeField] private GameObject selectedMark;

    [SerializeField] private Slider goldProgressBar;
    [SerializeField] private Button harvestButton;
    [SerializeField] private TextMeshProUGUI harvestGoldText;

    public GameObject CameraPivot;

    private CancellationTokenSource goldHarvestCTS;
    public int stage;

    private void Awake()
    {
        selectedMark.SetActive(false);
        harvestButton.onClick.AddListener(() =>
        {
          MGameManager.Instance.CheckStageGold(stage, transform.position);
        });
    }
    public void SetSelected(bool _value)
    {
        selectedMark.SetActive(_value);
    }
    public void UpdateData()
    {
        Game.StageStatus stageStatus = UserData.Instance.GetStageStatus(stage);
        switch (stageStatus)
        {
            case Game.StageStatus.Normal:
                {
                    defaultObject.SetActive(true);
                    lockObject.SetActive(false);
                    battleObject.SetActive(true);
                    occupationObject.SetActive(false);
                    goldProgressBar.SetActive(false);
                    harvestButton.SetActive(false);
                }
                break;
            case Game.StageStatus.Occupation:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(false);
                    battleObject.SetActive(false);
                    occupationObject.SetActive(true);
                    goldProgressBar.SetActive(true);
                    harvestButton.SetActive(false);
                    CheckGoldHarvest();
                }
                break;
            case Game.StageStatus.Lock:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(true);
                    battleObject.SetActive(false);
                    occupationObject.SetActive(false);
                    goldProgressBar.SetActive(false);
                    harvestButton.SetActive(false);
                }
                break;
        }
    }

    private void CheckGoldHarvest()
    {
        var stageData = UserData.Instance.GetStageData(stage);
        if (stageData == null)
        {
            Debug.LogError("stageData == null");
            return;
        }

        goldHarvestCTS?.Cancel();
        goldHarvestCTS = new CancellationTokenSource();

        UserData.Instance.GetStageData(stage).goldharvestTime.Skip(1).Subscribe(_value =>
        {
            CheckGoldHarvest();
        }).AddTo(goldHarvestCTS.Token);

        harvestGoldText.SetText(stageData.refData.goldproductamount.ToString());
        UniTask.Create(async () =>
        {
            while (GameTime.Get() < stageData.goldharvestTime.Value)
            {
                goldProgressBar.SetActive(true);
                harvestButton.SetActive(false);

                long timeLeft = stageData.goldharvestTime.Value - GameTime.Get();
                long elapse = stageData.refData.goldproductterm - timeLeft;
                goldProgressBar.value = elapse / (float)stageData.refData.goldproductterm;
                await UniTask.Delay(1000, cancellationToken : goldHarvestCTS.Token);
            }
            goldProgressBar.SetActive(false);
            harvestButton.SetActive(true);
        });
    }

    private void OnDisable()
    {
        goldHarvestCTS?.Cancel();
    }

    private void OnDestroy()
    {
        goldHarvestCTS?.Cancel();
        goldHarvestCTS?.Dispose();
    }
}
