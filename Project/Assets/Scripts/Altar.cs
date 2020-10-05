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
        if (GameManager.player == null || other.transform != GameManager.player.transform)
            return;

        if (IsHoldingGem)
        {
            LoseGem();
            GameManager.player.CarryGem();
            return;
        }
    }

    public void StoreGem()
    {
        spriteRenderer.sprite = gemSprite;
        gemLight.enabled = true;
        IsHoldingGem = true;
        GameManager.GemHolder = transform;
        GameManager.NotifyGemOwner(isPlayer: false);
    }

    public void LoseGem()
    {
        spriteRenderer.sprite = emptySprite;
        gemLight.enabled = false;
        IsHoldingGem = false;
        GameManager.NotifyGemOwner(isPlayer: true);
    }
}
