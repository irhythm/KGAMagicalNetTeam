using UnityEngine;

public static class DebuffFactory
{
    public static IDebuffBehavior GetBehavior(DebuffType type)
    {
        switch (type)
        {
            case DebuffType.Stun: return new StunBehavior();
            case DebuffType.Slow: return new SlowBehavior();
            case DebuffType.Polymorph: return new PolymorphBehavior();
            default: return null;
        }
    }
}
