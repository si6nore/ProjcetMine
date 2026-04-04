using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    public GameObject inventoryPanel;
    public Transform slotContainer;
    public GameObject slotPrefab;

    [Header("선택 아이템 정보")]
    public TMP_Text selectedItemName;
    public TMP_Text selectedItemDesc;
    public Image selectedItemIcon;
    public Button useButton;
    public Button dropButton;

    [Header("사운드")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource _audio;

    private Inventory _inventory;
    private Item _selectedItem;
    private bool _isOpen = false;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _inventory = player.GetComponent<Inventory>();
            _inventory.OnInventoryChanged += RefreshUI;
        }

        inventoryPanel.SetActive(false);

        useButton.onClick.AddListener(OnUseClicked);
        dropButton.onClick.AddListener(OnDropClicked);

        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        ClearSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            Toggle();
    }

    public void Toggle()
    {
        _isOpen = !_isOpen;
        inventoryPanel.SetActive(_isOpen);

        Cursor.lockState = _isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isOpen;

        // 열기/닫기 사운드
        if (_isOpen && openSound != null)
            _audio.PlayOneShot(openSound);
        else if (!_isOpen && closeSound != null)
            _audio.PlayOneShot(closeSound);

        if (_isOpen) RefreshUI();
        else ClearSelection();
    }

    public bool IsOpen => _isOpen;

    void RefreshUI()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        if (_inventory == null) return;

        foreach (Item item in _inventory.Items)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.Setup(item, OnSlotClicked);
        }
    }

    void OnSlotClicked(Item item)
    {
        _selectedItem = item;

        if (selectedItemName != null) selectedItemName.text = item.itemName;
        if (selectedItemDesc != null) selectedItemDesc.text = item.description;
        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = item.icon;
            selectedItemIcon.gameObject.SetActive(item.icon != null);
        }

        useButton.interactable = item.itemType != ItemType.Misc;
        dropButton.interactable = true;
    }

    void OnUseClicked()
    {
        if (_selectedItem == null) return;
        _inventory.UseItem(_selectedItem);
        ClearSelection();
    }

    void OnDropClicked()
    {
        if (_selectedItem == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        Vector3 dropPos = player != null ? player.transform.position : Vector3.zero;

        _inventory.DropItem(_selectedItem, dropPos);
        ClearSelection();
    }

    void ClearSelection()
    {
        _selectedItem = null;
        if (selectedItemName != null) selectedItemName.text = "";
        if (selectedItemDesc != null) selectedItemDesc.text = "";
        if (selectedItemIcon != null) selectedItemIcon.gameObject.SetActive(false);
        useButton.interactable = false;
        dropButton.interactable = false;
    }
}
