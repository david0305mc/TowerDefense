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
    //void SelectCell()
    //{
    //    if (scrollView.DataCount == 0)
    //    {
    //        return;
    //    }

    //    TryParseValue(selectIndexInputField, 0, scrollView.DataCount - 1, index =>
    //        scrollView.ScrollTo(index, 0.3f, Ease.InOutQuint, (Alignment)alignmentDropdown.value));
    //}
}
