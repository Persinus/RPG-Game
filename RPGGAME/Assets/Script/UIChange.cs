using UnityEngine;
using Fusion;
using System.Collections;

public class AvatarSwitcher : NetworkBehaviour
{
    [Header("Avatars")]
    [SerializeField] private PlayerCharacterController avatar1;
    [SerializeField] private PlayerCharacterController avatar2;

    [Header("Transition")]
    [SerializeField] private GameObject swapEffect;
    [SerializeField] private float fadeDuration = 0.25f;

    [Networked] private int ActiveIndex { get; set; }

    private ChangeDetector _changeDetector;
    private bool isSwitching;

    // =========================
    // SPAWN
    // =========================
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        ApplyInstant();
    }

    // =========================
    // INPUT (Authority only)
    // =========================
    public void SwitchAvatar()
    {
        if (!HasStateAuthority || isSwitching)
            return;

        isSwitching = true;

        // ✅ CHỈ ĐỔI STATE – KHÔNG SETACTIVE Ở ĐÂY
        ActiveIndex = ActiveIndex == 0 ? 1 : 0;
    }

    // =========================
    // CHANGE DETECTION (ALL)
    // =========================
    public override void FixedUpdateNetwork()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(ActiveIndex))
            {
                StartCoroutine(ApplySwitchVisual());
            }
        }
    }

    // =========================
    // VISUAL SWAP (LOCAL)
    // =========================
    private IEnumerator ApplySwitchVisual()
    {
        var current = ActiveIndex == 0 ? avatar2 : avatar1;
        var next = ActiveIndex == 0 ? avatar1 : avatar2;

        // Fade out cũ
        yield return Fade(current, 1f, 0f, fadeDuration);
        current.gameObject.SetActive(false);

        // Bật mới
        next.gameObject.SetActive(true);
        next.SetAlpha(0f);

        // ⭐ DÒNG QUAN TRỌNG
        GetComponent<PlayerMotor>().ApplyFacing();

        if (swapEffect)
            swapEffect.SetActive(true);

        yield return Fade(next, 0f, 1f, fadeDuration);

        if (swapEffect)
            swapEffect.SetActive(false);

        next.ResetFSM();
        isSwitching = false;
    }

    // =========================
    // INSTANT APPLY (Late join)
    // =========================
    private void ApplyInstant()
    {
        bool a1 = ActiveIndex == 0;
        bool a2 = !a1;

        avatar1.gameObject.SetActive(a1);
        avatar2.gameObject.SetActive(a2);

        avatar1.SetAlpha(a1 ? 1f : 0f);
        avatar2.SetAlpha(a2 ? 1f : 0f);
    }

    // =========================
    // FADE
    // =========================
    private IEnumerator Fade(
        PlayerCharacterController avatar,
        float from,
        float to,
        float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Runner.DeltaTime;
            avatar.SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        avatar.SetAlpha(to);
    }
}
