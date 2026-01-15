using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MagicSelectorUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private GameObject itemIconPrefab;

    [Header("Settings")]
    [SerializeField] private float radius = 200f;

    private PlayerInventory _inventory;
    private bool _isOpen;
    private bool _isSelectingLeft;
    private List<ItemData> _currentItems;
    private int _selectedIndex = -1;

    public void Initialize(PlayerInventory inventory)
    {
        _inventory = inventory;
        panelRoot.SetActive(false);
    }

    
    public void Open(bool isLeft)
    {
        if (_isOpen) return;

        _isOpen = true;
        _isSelectingLeft = isLeft;
        _currentItems = new List<ItemData>(_inventory.Inventory);

        RefreshUI();
        panelRoot.SetActive(true);

        // 커서 제어는 PlayerInputHandler나 Camera에서 중앙 관리하는 것이 충돌이 적습니다.
        // 여기서는 UI 로직만 처리합니다.
    }

    public void Close()
    {
        if (!_isOpen) return;

        if (_selectedIndex >= 0 && _selectedIndex < _currentItems.Count)
        {
            _inventory.EquipItem(_currentItems[_selectedIndex], _isSelectingLeft);
        }

        _isOpen = false;
        panelRoot.SetActive(false);
    }

    // RefreshUI 함수는 이전 코드와 동일하므로 생략 (그대로 두시면 됩니다)
    private void RefreshUI()
    {
        // ... (이전 답변의 RefreshUI 코드) ...
        foreach (Transform child in itemContainer) Destroy(child.gameObject);
        int count = _currentItems.Count;
        if (count == 0) return;
        float angleStep = 360f / count;
        float startAngle = 90f;
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle - (i * angleStep);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            GameObject iconObj = Instantiate(itemIconPrefab, itemContainer);
            iconObj.transform.localPosition = pos;
            var img = iconObj.GetComponentInChildren<Image>();
            if (img) img.sprite = _currentItems[i].icon;
        }
    }

    private void Update()
    {
        if (!_isOpen || _currentItems.Count == 0) return;
        CalculateSelection();
    }

    private void CalculateSelection()
    {
        // [수정됨] Input System 방식으로 마우스 좌표 가져오기
        if (Mouse.current == null) return; // 마우스가 연결 안 된 경우 방지
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector2 center = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 dir = mousePos - center;

        if (dir.magnitude < 50f)
        {
            _selectedIndex = -1;
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float clockAngle = 90f - angle;
        if (clockAngle < 0) clockAngle += 360f;

        float step = 360f / _currentItems.Count;
        int index = Mathf.FloorToInt((clockAngle + (step / 2)) / step) % _currentItems.Count;

        if (_selectedIndex != index)
        {
            _selectedIndex = index;
            Debug.Log($"Selected: {_currentItems[index].itemName}");
        }
    }
}