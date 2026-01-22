using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// 파괴시 고정될 면 설정용 enum
/// </summary>
[Flags]
public enum Anchor
{
    None = 0,
    Left = 1,
    Right = 2,
    Bottom = 4,
    Top = 8,
    Front = 16,
    Back = 32
}

// 에디터에서 Enum을 여러 개 선택할 수 있게 그려주는 드로어
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Anchor))]
public class AnchorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        // EnumFlagsField를 사용하여 비트 연산 기반의 다중 선택 UI 제공
        property.intValue = (int)(Anchor)EditorGUI.EnumFlagsField(position, label, (Anchor)property.intValue);
        EditorGUI.EndProperty();
    }
}
#endif