using System.Collections;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField] private float speedBuffDuration = 10f;
    public static BuffManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveSpeedBuff(CharacterControl player, float speedMultiplier)
    {
        Debug.Log("Speed Buff Triggered!");
        StartCoroutine(SpeedBuffCoroutine(player, speedBuffDuration, speedMultiplier));
    }

    private IEnumerator SpeedBuffCoroutine(CharacterControl player, float duration, float multiplier)
    {
        player.SetSpeedBuff(multiplier);
        yield return new WaitForSeconds(duration);
        player.SetSpeedBuff(1f); // 恢复正常速度
    }
}
