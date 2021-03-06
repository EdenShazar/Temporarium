﻿using System.Collections;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    new public static Camera camera;

    public static Gem unheldGem;

    public static Transform GemHolder { get; private set; }

#pragma warning disable CS0649
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject creaturePrefab;
    [SerializeField] GameObject bodyPrefab;
    [SerializeField] Transform creatureParent;
    [SerializeField] Collider2D backgroundCollider;
#pragma warning restore CS0649

    static GameObject[] creatureInstances;
    static GameObject[] bodyInstances;

    public GameObject PlayerPrefab { get => playerPrefab; }
    public GameObject CreaturePrefab { get => creaturePrefab; }
    public Transform CreatureParent { get => creatureParent; }

    static int activeCreatureCount = 0;
    const float creatureScreenBuffer = 0.2f;

    public static bool IsSearchingForPlayer { get; private set; }

    void Awake()
    {
        EnsureSingleton();

        camera = Camera.main;
        unheldGem = FindObjectOfType<Gem>();

        creatureInstances = new GameObject[Constants.maxCreatureInstances];
        bodyInstances = new GameObject[Constants.maxBodyInstances];

        PopulateInstanceArrays();
    }

    void Start()
    {
        SearchForPlayer();

        StartCoroutine(EnsureActiveCreatureInstances());
    }

    IEnumerator EnsureActiveCreatureInstances()
    {
        while (true)
        {
            CountActiveCreatures();
            ReinstantiateCreatures();

            yield return new WaitForSeconds(0.5f);
        }
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

    static void PositionCreatureInstance(GameObject creature)
    {
        bool validLocation = false;
        while (!validLocation)
        {
            float x = Random.Range(-creatureScreenBuffer * 2, 1);
            float y = Random.Range(0, 1 + creatureScreenBuffer * 2);

            Vector3 newPosition = camera.ViewportToWorldPoint(new Vector3(x, y, 0));
            newPosition.z = 0;
            creature.transform.position = newPosition;

            validLocation = !creature.GetComponent<CreatureController>().IsVisible() &&
                Physics2D.OverlapPoint(creature.transform.position, LayerMask.GetMask("Default")) == null;
        }
    }

    void PopulateInstanceArrays()
    {
        for (int i = 0; i < Constants.maxCreatureInstances; i++)
        {
            creatureInstances[i] = Instantiate(creaturePrefab, creatureParent);
            PositionCreatureInstance(creatureInstances[i]);
            if (i < Constants.maxCreatureInstances / 2)
                creatureInstances[i].GetComponent<CreatureController>().ActivateInstance();
        }

        for (int i = 0; i < Constants.maxBodyInstances; i++)
        {
            bodyInstances[i] = Instantiate(bodyPrefab, parent: creatureParent);
            bodyInstances[i].SetActive(false);
        }
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

    public static void NotifyEnabledCreatureInstance()
    {
        activeCreatureCount++;
    }

    public static void NotifyDisabledCreatureInstance()
    {
        activeCreatureCount--;
        ReinstantiateCreatures();
    }

    public static void NotifyDeactivatedPlayer()
    {
        NotifyDisabledCreatureInstance();
        SearchForPlayer();

        Debug.Log("Player has been deactivated!");
    }

    public static void NotifyActivatedPlayer()
    {
        NotifyEnabledCreatureInstance();
        StopSearchingForPlayer();

        Debug.Log("Player has been activated!");
    }

    public static void ClassifyGemHolder()
    {
        if (PlayerModule.CurrentPlayer == null || GemHolder == null || GemHolder != PlayerModule.CurrentPlayer.transform)
            CameraController.ActivateGemCamera();
        else
            CameraController.ActivatePlayerCamera();
    }

    static void CountActiveCreatures()
    {
        activeCreatureCount = 0;

        foreach (var creature in creatureInstances)
            if (creature.activeSelf)
                activeCreatureCount++;
    }

    static void ReinstantiateCreatures()
    {
        while (activeCreatureCount < Constants.maxCreatureInstances / 2)
        {
            GameObject creature = GetFreeCreatureInstance();
            PositionCreatureInstance(creature);
            creature.GetComponent<CreatureController>().ActivateInstance();
            activeCreatureCount++;
        }
    }

    static void SearchForPlayer()
    {
        IsSearchingForPlayer = true;
    }

    static void StopSearchingForPlayer()
    {
        IsSearchingForPlayer = false;
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
            float distance = (array[i].transform.position - PlayerModule.CurrentPlayer.transform.position).sqrMagnitude;
            if (distance > largestDistance)
            {
                largestDistance = distance;
                farthestIndex = i;
            }
        }

        return array[farthestIndex];
    }

    public static bool TryTakeGem(Transform taker)
    {
        if (GemHolder != null)
        {
            Debug.Log(taker.name + " failed to take the gem.");
            return false;
        }

        GemHolder = taker;
        ClassifyGemHolder();

        PlayerModule.RecordTakenGem(taker);

        Debug.Log(taker.name + " took the gem.");

        return true;
    }

    public static bool TakeGemFrom(Transform from, Transform taker)
    {
        if (GemHolder != from)
        {
            Debug.Log(taker.name + " failed to take the gem from " + from.name);
            return false;
        }

        GemHolder = taker;
        ClassifyGemHolder();

        PlayerModule.RecordTakenGem(taker);
        PlayerModule.RecordLostGem(from);

        Debug.Log(taker.name + " took the gem from " + from.name);

        return true;
    }
}
