using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Test_SHS : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("New Scene", LoadSceneMode.Additive);
    }

    private void Update()
    {
        // 같이 게임을 하는 4명의 플레이어가 모인 룸에서 인게임 씬의 내용을 미리 로드하기
        // 
    }
}
