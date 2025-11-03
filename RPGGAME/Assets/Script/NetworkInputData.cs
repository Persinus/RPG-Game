using Fusion;
using UnityEngine;

[System.Flags]
public enum InputButtons
{
    None        = 0,
    MoveLeft    = 1 << 0,
    MoveRight   = 1 << 1,
    Jump        = 1 << 2,
    Dash        = 1 << 3,   // Tốc biến
    Roll        = 1 << 4,
    Attack      = 1 << 5,
    Skill1      = 1 << 6,
    Skill2      = 1 << 7,
    Skill3      = 1 << 8,
    HealHP      = 1 << 9,
    HealMP      = 1 << 10
}

public struct NetworkInputData : INetworkInput
{
    public InputButtons buttons;
}
