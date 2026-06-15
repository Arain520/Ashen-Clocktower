using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EndingStatueTrigger : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool triggerOnce = true;

    [Header("Ending Sequence")]
    [SerializeField] private FullscreenTextSequence endingSequence;
    [TextArea(2, 4)]
    [SerializeField] private string[] endingLines =
    {
        "石像回应了最后一缕灰烬。",
        "钟塔没有再次响起。",
        "但你听见了远处的风。"
    };

    private bool playerInRange;
    private bool hasTriggered;
    private bool isShowingInteractToolTip;

    private void Reset()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (triggerOnce && hasTriggered)
            return;

        if (endingSequence != null && endingSequence.IsPlaying)
            return;

        if (Input.GetKeyDown(interactKey))
            PlayEnding();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && collision.GetComponent<Player>() == null)
            return;

        playerInRange = true;
        ShowInteractToolTip();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && collision.GetComponent<Player>() == null)
            return;

        playerInRange = false;
        HideInteractToolTip();
    }

    private void PlayEnding()
    {
        if (triggerOnce && hasTriggered)
            return;

        if (endingSequence == null)
        {
            Debug.LogWarning("EndingStatueTrigger has no ending sequence assigned.");
            return;
        }

        hasTriggered = true;
        HideInteractToolTip();
        endingSequence.Play(endingLines);
    }

    private void ShowInteractToolTip()
    {
        if (isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance != null)
        {
            UI_MainScene.instance.SetWhetherShowInteractToolTip(true);
            isShowingInteractToolTip = true;
        }
    }

    private void HideInteractToolTip()
    {
        if (!isShowingInteractToolTip)
            return;

        if (UI_MainScene.instance != null)
            UI_MainScene.instance.SetWhetherShowInteractToolTip(false);

        isShowingInteractToolTip = false;
    }
}
