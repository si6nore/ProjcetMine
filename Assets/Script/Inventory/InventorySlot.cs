using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("슬롯 UI")]
    public Image iconImage;
    public TMP_Text itemNameText;
    public Button button;

    private Item _item;
    private System.Action<Item> _onClicked;

    public void Setup(Item item, System.Action<Item> onClicked)
    {
        _item = item;
        _onClicked = onClicked;

        if (itemNameText != null) itemNameText.text = item.itemName;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.gameObject.SetActive(item.icon != null);
        }

        button.onClick.AddListener(() => _onClicked?.Invoke(_item));
    }
}
