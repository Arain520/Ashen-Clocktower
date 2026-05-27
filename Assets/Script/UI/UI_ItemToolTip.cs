using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ItemToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    public void ShowItemToolTip(ItemData _item)
    //传入物品信息
    {
        itemNameText.text = _item.itemName;
        itemTypeText.text = GetChineseItemTypeName(_item.itemType);
        itemDescriptionText.text = _item.itemDescription;

        //显示这个ToolTip
        gameObject.SetActive(true);

        //UI音效
        AudioManager.instance.PlaySFX(5, null);
    }


    private string GetChineseItemTypeName(ItemType _itemType)
    {
        if (_itemType == ItemType.Weapon)
            return "武器";

        if (_itemType == ItemType.Potion)
            return "药水";

        if (_itemType == ItemType.CD)
            return "唱片";

        return "物品";
    }
    public void HideItemToolTip()
    {
        gameObject.SetActive(false);
    }
}
