using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("아이템 데이터")]
    public Item item;

    [Header("줍기 설정")]
    public float pickupRange = 2.5f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("UI 안내 (선택)")]
    public GameObject interactPrompt; // "E 키로 줍기" 텍스트 오브젝트

    private Transform _player;
    private Inventory _inventory;
    private bool _inRange = false;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            _player = p.transform;
            _inventory = p.GetComponent<Inventory>();
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        _inRange = dist <= pickupRange;

        // 안내 텍스트 켜고 끄기
        if (interactPrompt != null)
            interactPrompt.SetActive(_inRange);

        // E 키로 줍기
        if (_inRange && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (_inventory == null)
        {
            Debug.LogWarning("플레이어에 Inventory 컴포넌트가 없습니다!");
            return;
        }

        bool success = _inventory.AddItem(item);

        if (success)
        {
            Debug.Log($"{item.itemName} 획득!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("인벤토리가 꽉 찼습니다!");
        }
    }

    // 에디터에서 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
