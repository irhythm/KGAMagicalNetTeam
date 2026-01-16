using UnityEngine;
using TMPro;

//스폰타이머 스크립트, 간단한 옵저버패턴
public class TimerUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        //매니저가 없으면 리턴
        if (GuardManager.instance == null) return;

        //이벤트 구독
        GuardManager.instance.OnTimerSecondChanged += UpdateTimerText;

        //초기 값 설정
        UpdateTimerText(GuardManager.instance.GetFormattedTime());
    }

    private void UpdateTimerText(string timeString)
    {
        if (timerText != null)
        {
            timerText.text = timeString;
        }
    }
    private void OnDestroy()
    {
        //메모리 누수 방지
        if (GuardManager.instance != null)
        {
            GuardManager.instance.OnTimerSecondChanged -= UpdateTimerText;
        }
    }


}
