using UnityEngine;
using Fusion;

[System.Flags]
public enum InputButtons : ushort
{
    None   = 0,
    Attack = 1 << 0,
    Jump   = 1 << 1,
    Roll   = 1 << 2,
    Dash   = 1 << 3,
    Skill1 = 1 << 4,
    Skill2 = 1 << 5,
    Skill3 = 1 << 6,
    HealHP = 1 << 7,
    HealMP = 1 << 8,
}

public struct NetworkInputData : INetworkInput
{
    public InputButtons buttons;        // Giữ trạng thái nút
    public InputButtons clickedButtons; // Nhấn 1 lần
    public Vector2 movement;            // Joystick

    public void Click(InputButtons button) => clickedButtons |= button;
    public void Reset() => clickedButtons = 0;
    public bool Has(InputButtons button) => (buttons & button) != 0;
    public bool Clicked(InputButtons button) => (clickedButtons & button) != 0;
}
