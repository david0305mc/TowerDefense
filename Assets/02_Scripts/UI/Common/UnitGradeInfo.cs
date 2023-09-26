using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitGradeInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI pieceCountText;
    [SerializeField] private Slider pieceCountProgress;

    public void SetData(int _grade, bool _isMaxGrade, int _havePiece, int _reqPiece)
    {
        levelText.SetText(_grade.ToString());
        if (_isMaxGrade)
        {
            // Max Grade
            pieceCountText.SetText($"{_havePiece}");
            pieceCountText.color = Color.white;
            pieceCountProgress.value = 1f;
        }
        else
        {
            pieceCountText.SetText($"{_havePiece}/{_reqPiece}");
            pieceCountProgress.value = _havePiece / (float)_reqPiece;
            if (_havePiece >= _reqPiece)
            {
                pieceCountText.color = Color.white;
            }
            else
            {
                pieceCountText.color = Color.red;
            }
        }
    }
}

