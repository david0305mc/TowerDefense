using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattlePartySlot : MonoBehaviour
{
    [SerializeField] private GameObject emptySlot;
    [SerializeField] private Transform characterViewTr;
    public Transform CharacterViewTR => characterViewTr; 

    public void SetData(int _unitUID)
    {
        emptySlot.SetActive(false);
    }
}
