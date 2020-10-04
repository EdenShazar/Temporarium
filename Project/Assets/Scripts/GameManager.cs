using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    new public static Camera camera;
    
    static PlayerController player;
    public static PlayerController Player
    {
        get
        {
            if (player == null || !player.gameObject.activeSelf)
                player = playerInstances.First(player => player.activeSelf).GetComponent<PlayerController>();
            
            return player;
        }

        set => player = value;
    }

#pragma warning disable CS0649
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject creaturePrefab;
    [SerializeField] GameObject bodyPrefab;
    [SerializeField] Transform creatureParent;
#pragma warning restore CS0649

    static GameObject[] playerInstances;
    static GameObject[] creatureInstances;
    static GameObject[] bodyInstances;

    public GameObject PlayerPrefab { get => playerPrefab; }
    public GameObject CreaturePrefab { get => creaturePrefab; }
    public Transform CreatureParent { get => creatureParent; }

    Collider2D[] overlappingColliders = new Collider2D[0];
    ContactFilter2D noContactFiler = new ContactFilter2D();

    void Awake()
    {
        EnsureSingleton();

        camera = Camera.main;

        playerInstances = new GameObject[Constants.maxPlayerInstances];
        creatureInstances = new GameObject[Constants.maxCreatureInstances];
        bodyInstances = new GameObject[Constants.maxBodyInstances];

        noContactFiler.NoFilter();

        PopulateInstanceArrays();
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

    GameObject InitializeCreatureInstance()
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

        return newCreature;
    }

    void PopulateInstanceArrays()
    {
        player = FindObjectOfType<PlayerController>(includeInactive: true);
        playerInstances[0] = player.gameObject;
        playerInstances[0].GetComponent<CreatureController>().ActivateInstance();

        for (int i = 1; i < Constants.maxPlayerInstances; i++)
            playerInstances[i] = Instantiate(playerPrefab, parent: creatureParent);

        for (int i = 0; i < Constants.maxCreatureInstances; i++)
        {
            creatureInstances[i] = InitializeCreatureInstance();
            if (i < Constants.maxCreatureInstances / 2)
                creatureInstances[i].GetComponent<CreatureController>().ActivateInstance();
        }

        for (int i = 0; i < Constants.maxBodyInstances; i++)
        {
            bodyInstances[i] = Instantiate(bodyPrefab, parent: creatureParent);
            bodyInstances[i].SetActive(false);
        }
    }

    public static GameObject GetFreePlayerInstance(bool forced = false)
    {
        GameObject instance = playerInstances.FirstOrDefault(go => !go.activeSelf);

        if (instance == null)
        {
            PurgeNonVisibleObjects(playerInstances);
            instance = playerInstances.FirstOrDefault(go => !go.activeSelf);
        }

        if (instance == null && !forced)
            return null;

        if (instance == null)
            instance = FindFarthestObjectFromPlayer(playerInstances);

        return instance;
    }

    public static GameObject GetFreeCreatureInstance(bool forced = false)
    {
        GameObject instance = creatureInstances.FirstOrDefault(go => !go.activeSelf);

        if (instance == null)
        {
            PurgeNonVisibleObjects(creatureInstances);
            instance = creatureInstances.FirstOrDefault(go => !go.activeSelf);
        }

        if (instance == null && !forced)
            return null;

        if (instance == null)
            instance = FindFarthestObjectFromPlayer(creatureInstances);

        return instance;
    }

    public static GameObject GetFreeBodyInstance(bool forced = false)
    {
        GameObject instance = bodyInstances.FirstOrDefault(go => !go.activeSelf);

        if (instance == null)
        {
            PurgeNonVisibleObjects(bodyInstances);
            instance = bodyInstances.FirstOrDefault(go => !go.activeSelf);
        }

        if (instance == null && !forced)
            return null;

        if (instance == null)
            instance = FindFarthestObjectFromPlayer(bodyInstances);

        return instance;
    }

    public static void FreeInstance(GameObject instance)
    {
        instance.SetActive(false);
    }

    static void PurgeNonVisibleObjects(GameObject[] array)
    {
        for (int i = 0; i < array.Length; i++)
            if (!array[i].GetComponent<Renderer>().isVisible)
                array[i].SetActive(false);
    }

    static GameObject FindFarthestObjectFromPlayer(GameObject[] array)
    {
        float largestDistance = Mathf.NegativeInfinity;
        int farthestIndex = 0;
        for (int i = 0; i < array.Length; i++)
        {
            float distance = (array[i].transform.position - player.transform.position).sqrMagnitude;
            if (distance > largestDistance)
            {
                largestDistance = distance;
                farthestIndex = i;
            }
        }

        return array[farthestIndex];
    }
}
