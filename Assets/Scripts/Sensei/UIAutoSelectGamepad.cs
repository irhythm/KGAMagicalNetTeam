using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIAutoSelectGamepad : MonoBehaviour
{
    //[SerializeField] Stack<GameObject> _panels;

    [SerializeField] bool _panel = false;
    [SerializeField] GameObject _mainCanvas;

    void OnEnable()
    {


        
        //if (Gamepad.all.Count > 0)
        //{
        //    EventSystem.current.SetSelectedGameObject(null);
        //    EventSystem.current.SetSelectedGameObject(GetComponentInChildren<Button>().gameObject);
        //}

    }

    void Update()
    {
        DefaultSelection();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //DefaultSelection();
    }

    public void DefaultSelection()
    {
        if (Gamepad.all.Count > 0 && EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Button temp = GetComponentInChildren<Button>();

            EventSystem.current.SetSelectedGameObject(temp.gameObject);

        }
        else if (Gamepad.all.Count > 0 && _panel)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Button temp = GetComponentInChildren<Button>();
            EventSystem.current.SetSelectedGameObject(temp.gameObject);
        }
    }

    // Update is called once per frame
    

    void OnDisable()
    {
        if (!_panel)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
        else
        {
            if (_mainCanvas != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                _mainCanvas.GetComponent<UIAutoSelectGamepad>().DefaultSelection();
            }
        }
    }

}
