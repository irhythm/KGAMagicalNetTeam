public interface IState
{
    void Enter();        // 상태 진입시
    void Execute();      // 상태 업데이트 주기
    void FixedExecute(); // 고정 주기 업데이트 주기
    void Exit();         // 상태 나갈 때
}
