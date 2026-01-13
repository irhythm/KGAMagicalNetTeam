using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 코루틴 관리 클래스
/// 캐싱을 이용해 최적화
/// </summary>
public static class CoroutineManager
{
    public static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    private static readonly Dictionary<float, WaitForSeconds> _waitForSeconds = new Dictionary<float, WaitForSeconds>();
    private static readonly Dictionary<float, WaitForSecondsRealtime> _waitForSecondsRealTime = new Dictionary<float, WaitForSecondsRealtime>();

    public static WaitForSeconds waitForSeconds(float seconds)
    {
        if (!_waitForSeconds.TryGetValue(seconds, out var _seconds))
        {
            _waitForSeconds.Add(seconds, _seconds = new WaitForSeconds(seconds));
        }

        return _seconds;
    }

    public static WaitForSecondsRealtime waitForSecondsRealtime(float seconds)
    {
        if (!_waitForSecondsRealTime.TryGetValue(seconds, out var _seconds))
        {
            _waitForSecondsRealTime.Add(seconds, _seconds = new WaitForSecondsRealtime(seconds));
        }

        return _seconds;
    }
}
