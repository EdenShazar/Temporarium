using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Altar : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] Sprite emptySprite;
    [SerializeField] Sprite gemSprite;
#pragma warning restore CS0649

    SpriteRenderer spriteRenderer;
    Light2D gemLight;

    public bool IsHoldingGem { get; private set; }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gemLight = transform.GetChild(0).GetComponent<Light2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayerModule.CurrentPlayer == null || other.transform != PlayerModule.CurrentPlayer.transform)
            return;

        if (IsHoldingGem && PlayerModule.CurrentPlayer.TryTakeGemFrom(transform))
            LoseGem();
    }

    public void StoreGem(Transform from)
    {
        if (!GameManager.TakeGemFrom(from, taker: transform))
            return;

        spriteRenderer.sprite = gemSprite;
        gemLight.enabled = true;
        IsHoldingGem = true;
    }

    public void LoseGem()
    {
        spriteRenderer.sprite = emptySprite;
        gemLight.enabled = false;
        IsHoldingGem = false;
    }
}
