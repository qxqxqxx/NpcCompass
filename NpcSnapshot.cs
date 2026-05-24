using UnityEngine;

namespace NpcCompass;

public enum NpcFixedType
{
    Unknown = -1,
    None = 0,
    Car = 1,
    Talk = 2,
    SitWork = 3,
    SitTalk = 4,
    SitSleep = 5,
    SitBook = 6,
    SitSumaho = 7,
    StandSumaho = 8,
    Pinpon = 9,
    Conbini = 10,
}

public struct NpcSnapshot
{
    public int Id;
    public Vector3 Position;
    public Vector3 Forward;
    public NpcFixedType FixedType;

    public NpcSnapshot(int id, Vector3 position, Vector3 forward, NpcFixedType fixedType)
    {
        Id = id;
        Position = position;
        Forward = forward;
        FixedType = fixedType;
    }
}
