using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static PlayerController player;

    public static Camera camera;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject creaturePrefab;
    [SerializeField] Transform creatureParent;

    List<GameObject> Creatures = new List<GameObject>();

    Collider2D[] overlappingColliders = new Collider2D[0];
    ContactFilter2D noContactFiler = new ContactFilter2D();

    void Awake()
    {
        EnsureSingleton();
        player = FindObjectOfType<PlayerController>();

        camera = Camera.main;

        noContactFiler.NoFilter();
    }

    void Start()
    {
        for (int i = 0; i < 20; i++)
            InstantiateCreature();
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

    void InstantiateCreature()
    {
        float x = Random.Range(0, camera.pixelWidth);
        float y = Random.Range(0, camera.pixelHeight);
        Vector2 creaturePosition = camera.ScreenToWorldPoint(new Vector3(x, y, 0)).ToVector2();

        GameObject newCreature = Instantiate(creaturePrefab, creaturePosition, Quaternion.identity, creatureParent);
        Collider2D newCollider = newCreature.GetComponent<Collider2D>();

        bool validLocation = newCollider.OverlapCollider(noContactFiler, overlappingColliders) == 0;

        while (!validLocation)
        {
            Vector3 newPosition = new Vector3(Random.Range(0, camera.pixelWidth), Random.Range(0, camera.pixelHeight), 0);
            newCreature.transform.position = newPosition;

            validLocation = newCollider.OverlapCollider(noContactFiler, overlappingColliders) == 0;
        }

        Creatures.Add(newCreature);
    }
}
