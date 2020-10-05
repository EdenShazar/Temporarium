using UnityEngine;

public class Gem : MonoBehaviour
{
    void Start()
    {
        GameManager.GemHolder = transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform != GameManager.player.transform)
            return;

        GameManager.player.CarryGem();
        gameObject.SetActive(false);
        return;
    }

    public void DropAtPosition(Vector2 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        GameManager.GemHolder = transform;
        GameManager.NotifyGemOwner(isPlayer: false);
    }
}
