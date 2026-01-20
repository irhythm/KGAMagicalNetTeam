using UnityEngine;

// 네트워크 기준 게임 상태, 네트워크 상 현재 룸의 모든 플레이어 상태
public enum Network_GameState
{
    Standby,        // 대기 상태, 모든 플레이어의 로딩 진행 중 → 게임 로직이 실행되면 안됨
    Play            // 시작 상태, 모든 플레이어의 로딩 완료    → 게임 로직 실행 시작
}

public class ProjectManager : Singleton<ProjectManager>
{

}
