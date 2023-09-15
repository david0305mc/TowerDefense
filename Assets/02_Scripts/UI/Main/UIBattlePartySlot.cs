using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattlePartySlot : MonoBehaviour
{
    [SerializeField] private GameObject emptySlot;
    [SerializeField] private Transform characterViewTr;
    [SerializeField] private Button button;

    private MHeroObj heroObj = default;

    private System.Action<int> touchAction;
    private int slotIndex;
    private int unitUID;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            if (unitUID != -1)
            {
                touchAction?.Invoke(slotIndex);
            }
        });
    }

    public void SetData(int _slotIndex, int _unitUID, System.Action<int> _touchAction)
    {
        unitUID = _unitUID;
        slotIndex = _slotIndex;
        touchAction = _touchAction;
        if (_unitUID != -1)
        {
            AddHero(_unitUID);
        }
    }

    public void RemoveHero()
    {
        unitUID = -1;
        Lean.Pool.LeanPool.Despawn(heroObj.gameObject);
        emptySlot.SetActive(true);
    }

    public void AddHero(int _unitUID)
    {
        unitUID = _unitUID;
        emptySlot.SetActive(false);
        var heroData = UserData.Instance.GetHeroData(_unitUID);
        if (heroData != null)
        {
            GameObject unitPrefab = MResourceManager.Instance.GetPrefab(heroData.refData.prefabname);
            heroObj = Lean.Pool.LeanPool.Spawn(unitPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<MHeroObj>();
            heroObj.transform.SetLocalPosition(Vector3.zero);
            heroObj.SetUIMode();
        }
    }

    public void ClearPool()
    {
        if (heroObj != null)
        {
            heroObj.SetBattleMode();
            Lean.Pool.LeanPool.Despawn(heroObj.gameObject);
        }
    }
}
