using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIPanelChallenge : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private UIChallengeCell cellPrefab;

    private List<UIChallengeCell> challengeCellList;

    private void OnEnable()
    {
        challengeCellList = new List<UIChallengeCell>();
        Enumerable.Range(0, 5).ToList().ForEach(i =>
        {
            var itemObj = Lean.Pool.LeanPool.Spawn(cellPrefab, scrollRect.content);
            challengeCellList.Add(itemObj);
        });
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
