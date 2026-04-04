using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 설정")]
    public int maxSlots = 20;

    [Header("버리기 설정")]
    public float dropDistance = 1.5f;

    private List<Item> _items = new List<Item>();

    public System.Action OnInventoryChanged;
    public List<Item> Items => _items;

    public bool AddItem(Item item)
    {
        if (_items.Count >= maxSlots) return false;

        _items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void RemoveItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
            OnInventoryChanged?.Invoke();
        }
    }

    public void UseItem(Item item)
    {
        if (!_items.Contains(item)) return;

        Health health = GetComponent<Health>();

        switch (item.itemType)
        {
            case ItemType.Heal:
                if (health != null)
                {
                    health.TakeDamage(-item.healAmount);

                    // ✅ 힐 아이템 사용 시 출혈 멈춤
                    health.StopBleeding();

                    Debug.Log($"{item.itemName} 사용! {item.healAmount} 회복, 출혈 제거");
                }
                RemoveItem(item);
                break;

            case ItemType.Ammo:
                WeaponSystem weapon = GetComponent<WeaponSystem>();
                if (weapon != null)
                {
                    weapon.totalAmmo += (int)item.healAmount;
                    Debug.Log($"{item.itemName} 사용! 탄약 +{(int)item.healAmount}");
                }
                RemoveItem(item);
                break;

            case ItemType.Misc:
                Debug.Log($"{item.itemName}은 사용할 수 없는 아이템입니다.");
                break;
        }
    }

    public void DropItem(Item item, Vector3 dropPosition)
    {
        if (!_items.Contains(item)) return;

        if (item.worldPrefab != null)
        {
            Vector3 forwardDrop = dropPosition + transform.forward * dropDistance + Vector3.up * 0.5f;
            Instantiate(item.worldPrefab, forwardDrop, Quaternion.identity);
        }

        RemoveItem(item);
        Debug.Log($"{item.itemName} 버림!");
    }
}
