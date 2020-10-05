using UnityEngine;

public class Gem : MonoBehaviour
{
    void Start()
    {
        GameManager.TryTakeGem(taker: transform);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform != PlayerModule.CurrentPlayer.transform)
            return;

        if (PlayerModule.CurrentPlayer.TryTakeGemFrom(from: transform))
            gameObject.SetActive(false);
    }

    public void DropAtPosition(Vector2 position, Transform dropper)
    {
        if (!GameManager.TakeGemFrom(from: dropper, taker: transform))
            return;

        transform.position = position;
        gameObject.SetActive(true);
    }
}
