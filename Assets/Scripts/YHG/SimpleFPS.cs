using UnityEngine;

//fps 출력용
public class SimpleFPS : MonoBehaviour
{
    [Header("설정")]
    [Range(10, 100)] public int fontSize = 30;
    public Color color = Color.green;

    private float deltaTime = 0.0f;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f; //보간
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        //폰트 크기 및 정렬
        Rect rect = new Rect(20, 20, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = color;

        //계산
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        
        GUI.Label(rect, text, style);
    }
}
