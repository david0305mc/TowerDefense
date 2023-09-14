using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScrollTest : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;
    // Start is called before the first frame update
    void Start()
    {
        var itemData = Enumerable.Range(0, 10).Select(i => new ShopItemData(i)).ToArray();
        gridView.UpdateContents(itemData);

        gridView.OnCellClicked(index =>
        {
            //SelectCell(index);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
