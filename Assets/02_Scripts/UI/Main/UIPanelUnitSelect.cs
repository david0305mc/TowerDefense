using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPanelUnitSelect : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;

    // Start is called before the first frame update
    void Start()
    {
        var itemData = Enumerable.Range(0, 10).Select(i => new UIUnitData(i)).ToArray();
        gridView.UpdateContents(itemData);

        gridView.OnCellClicked(index =>
        {
            Debug.Log($"OnCellClicked {index}");

            //SelectCell(index);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
