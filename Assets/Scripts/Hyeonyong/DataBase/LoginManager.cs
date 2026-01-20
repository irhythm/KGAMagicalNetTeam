using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class LoginManager : MonoBehaviour
{
    [SerializeField] private InputActionReference nextTabInput;
    [SerializeField] private InputActionReference enterInput;

    PlayerInput playerInput;
    [SerializeField] TMP_InputField[] inputField;
    [SerializeField] Button[] playGameBtns;

    [SerializeField] AudioClip loginAudio;

    int curIndex = -1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        nextTabInput.action.Enable();
        nextTabInput.action.performed += NextTab;
        enterInput.action.Enable();
        enterInput.action.performed += EnterGame;
        //playerInput.actions["Tab"].performed += NextTab;

        SoundManager.Instance.PlayBGM(loginAudio);
    }
    private void OnDisable()
    {
        nextTabInput.action.Disable();
        enterInput.action.Disable();
        //playerInput.actions["Tab"].performed -= NextTab;
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
    public void EnterGame(InputAction.CallbackContext context)
    {
        foreach (Button btn in playGameBtns)
        {
            if (btn.gameObject.activeSelf)
            {
                Debug.Log("버튼 클릭");
                btn.onClick.Invoke();
                break;
            }
        }
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
