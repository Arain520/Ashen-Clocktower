using UnityEngine;
using UnityEngine.Events;

public class AshRewardStatue : MonoBehaviour
{
    [Header("Ash Reward")]
    [Min(0)]
    [SerializeField] private int ashPerHit = 5;
    [Min(0)]
    [SerializeField] private int maxHitCount = 3;

    [Header("Collision")]
    [SerializeField] private bool ignorePlayerCollision = true;

    [Header("Feedback")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hitTriggerName = "hit";
    [SerializeField] private bool playHitSfx;
    [SerializeField] private int hitSfxIndex = 12;
    [SerializeField] private UnityEvent onRewardGranted;
    [SerializeField] private UnityEvent onHitLimitReached;

    private int currentHitCount;

    public int CurrentHitCount => currentHitCount;
    public int MaxHitCount => maxHitCount;
    public bool CanGrantAsh => maxHitCount <= 0 || currentHitCount < maxHitCount;

    private void OnEnable()
    {
        SavesManager.OnGameSaved += ResetHitCount;
        IgnorePlayerCollisionIfNeeded();
    }

    private void OnDisable()
    {
        SavesManager.OnGameSaved -= ResetHitCount;
    }

    private void Start()
    {
        IgnorePlayerCollisionIfNeeded();
    }

    public bool TryGrantAsh()
    {
        if (!CanGrantAsh)
        {
            onHitLimitReached?.Invoke();
            return false;
        }

        if (AshSystem.Instance == null)
        {
            Debug.LogWarning("AshRewardStatue could not find AshSystem.");
            return false;
        }

        currentHitCount++;
        AshSystem.Instance.AddAsh(ashPerHit);
        PlayHitFeedback();
        onRewardGranted?.Invoke();
        return true;
    }

    public void ResetHitCount()
    {
        currentHitCount = 0;
    }

    private void PlayHitFeedback()
    {
        if (animator != null && !string.IsNullOrEmpty(hitTriggerName))
            animator.SetTrigger(hitTriggerName);

        if (playHitSfx && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(hitSfxIndex, null);
    }

    private void IgnorePlayerCollisionIfNeeded()
    {
        if (!ignorePlayerCollision)
            return;

        Player player = PlayerManager.instance != null ? PlayerManager.instance.player : FindObjectOfType<Player>();
        if (player == null)
            return;

        Collider2D[] statueColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D statueCollider in statueColliders)
        {
            if (statueCollider == null)
                continue;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider != null)
                    Physics2D.IgnoreCollision(statueCollider, playerCollider, true);
            }
        }
    }
}
