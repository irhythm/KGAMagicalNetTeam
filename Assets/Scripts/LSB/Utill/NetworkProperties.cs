using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 플레이어 및 룸의 커스텀 프로퍼티를 통합 관리하는 확장 메서드 클래스
/// </summary>
public static class NetworkProperties
{
    [Header("Player")]
    public const string PLAYER_ISWIZARD = "ISWIZARD";

    [Header("Room")]
    public const string ROOM = "ROOM";



    // player.SetProps(NetworkProperties.만들어 놓은 키 변수 명, 1);
    public static void SetProps<T>(this Player player, string key, T value)
    {
        // 이미 같은 값이면 설정하지 않음
        if (player.CustomProperties.ContainsKey(key))
        {
            T current = player.GetProps<T>(key);
            if (current != null && current.Equals(value)) return;
        }

        // 값 설정
        Hashtable props = new Hashtable { { key, value } };
        player.SetCustomProperties(props);
    }

    // 자료형 (담을 변수) = player.GetProps<자료형>(NetworkProperties.만들어 놓은 키 변수 명);
    public static T GetProps<T>(this Player player, string key)
    {
        // 해당 키가 존재하면 값을 반환
        if (player.CustomProperties.TryGetValue(key, out object val))
        {
            return (T)val;
        }
        return default;
    }




    // PhotonNetwork.CurrentRoom.SetProps(NetworkProperties.만들어 놓은 키 변수 명, 저장할 값);
    public static void SetProps<T>(this Room room, string key, T value)
    {
        if (room == null) return;

        // 이미 같은 값이면 설정하지 않음
        if (room.CustomProperties.ContainsKey(key))
        {
            T current = room.GetProps<T>(key);
            if (current != null && current.Equals(value)) return;
        }

        // 값 설정
        Hashtable props = new Hashtable { { key, value } };
        room.SetCustomProperties(props);
    }

    // 자료형 (담을 변수) = PhotonNetwork.CurrentRoom.GetProps<자료형>(NetworkProperties.만들어 놓은 키 변수 명);
    public static T GetProps<T>(this Room room, string key)
    {
        // 룸이 null이 아니고, 해당 키가 존재하면 값을 반환
        if (room != null && room.CustomProperties.TryGetValue(key, out object val))
        {
            return (T)val;
        }
        return default;
    }
}