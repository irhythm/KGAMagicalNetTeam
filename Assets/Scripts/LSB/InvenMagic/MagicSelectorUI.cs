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

    private void RefreshUI()
    {
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
        if (Mouse.current == null) return;
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