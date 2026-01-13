using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class LoginManager : MonoBehaviour
{
    PlayerInput playerInput;
    [SerializeField] TMP_InputField[] inputField;
    int curIndex = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Tab"].performed += NextTab;
    }
    private void OnDisable()
    {
        playerInput.actions["Tab"].performed -= NextTab;
    }
    public void NextTab(InputAction.CallbackContext context)
    {
        Debug.Log("탭 클릭");
        SetTabNum();
        if (curIndex == -1)
            return;
        curIndex++;
        if (curIndex == inputField.Length)
        {
            curIndex = 0;
        }
        if (inputField[curIndex].interactable == false)
        {
            curIndex++;
            if (curIndex == inputField.Length)
            {
                curIndex = 0;
            }
        }
        inputField[curIndex].Select();
        inputField[curIndex].ActivateInputField();
    }

    public void SetTabNum()
    {
        for (int i = 0; i < inputField.Length; i++)
        {
            if (inputField[i].isFocused)
            {
                curIndex = i;
                return;
            }
        }
        curIndex = -1;
    }


    //260113 최정욱
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Title");
    }
}
