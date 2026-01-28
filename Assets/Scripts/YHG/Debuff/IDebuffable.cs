public interface IDebuffable
{
    //디버프 적용, 인포값 받고
    void ApplyDebuff(DebuffInfo info);

    //해제
    void RemoveDebuff(DebuffType type);
}