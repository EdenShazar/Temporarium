using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        EnsureSingleton();
    }

    void EnsureSingleton()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.LogWarning("GameManager instance already exists on " + Instance.gameObject.name +
                ". Deleting instance from " + gameObject.name);

            DestroyImmediate(this);
        }
    }
}
