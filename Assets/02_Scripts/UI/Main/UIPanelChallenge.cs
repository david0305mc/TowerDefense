using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelChallenge : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private UIChallengeCell cellPrefab;

    private List<UIChallengeCell> challengeCellList;

    private void OnEnable()
    {
        challengeCellList = new List<UIChallengeCell>();
        {
            UIChallengeCell itemObj = Lean.Pool.LeanPool.Spawn(cellPrefab, scrollRect.content);
            itemObj.SetData(Game.GameConfig.WaveStageID_01, ()=> {
                MGameManager.Instance.StartWaveStage(Game.GameConfig.WaveStageID_01);
            });
            challengeCellList.Add(itemObj);
        }

        {
            UIChallengeCell itemObj = Lean.Pool.LeanPool.Spawn(cellPrefab, scrollRect.content);
            itemObj.SetData(Game.GameConfig.WaveStageID_02, ()=> {
                MGameManager.Instance.StartWaveStage(Game.GameConfig.WaveStageID_02);
            });
            challengeCellList.Add(itemObj);
        }
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void OnDisable()
    {
        ClearCell();
    }

    private void ClearCell()
    {
        foreach (var item in challengeCellList)
        {
            Lean.Pool.LeanPool.Despawn(item);
        }
        challengeCellList.Clear();
    }
}
