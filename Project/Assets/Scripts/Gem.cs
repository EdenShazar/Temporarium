using UnityEngine;

public class Gem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform != GameManager.player.transform)
            return;

        GameManager.player.CarryGem();
        gameObject.SetActive(false);
        return;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
