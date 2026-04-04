using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName = "아이템";
    public Sprite icon;
    [TextArea] public string description = "";

    [Header("아이템 종류")]
    public ItemType itemType = ItemType.Misc;

    [Header("사용 효과 (힐 아이템일 때)")]
    public float healAmount = 0f;

    [Header("맵에 배치될 프리팹")]
    public GameObject worldPrefab;
}

public enum ItemType
{
    Heal,       // 힐 아이템
    Ammo,       // 탄약
    Misc        // 기타
}
