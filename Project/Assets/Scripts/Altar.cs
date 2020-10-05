using UnityEngine;

public class Altar : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] Sprite emptySprite;
    [SerializeField] Sprite gemSprite;
#pragma warning restore CS0649

    SpriteRenderer spriteRenderer;
    GemHolder gemHolder;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gemHolder = GetComponent<GemHolder>();
    }

    public void StoreGem()
    {
        spriteRenderer.sprite = gemSprite;
        gemHolder.isHoldingGem = true;
    }

    public void RemoveGem()
    {
        spriteRenderer.sprite = emptySprite;
        gemHolder.isHoldingGem = false;
    }
}
