using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class CreatureController : MonoBehaviour
{
    // Inspector fields
#pragma warning disable CS0649
    [SerializeField] [Min(1)] float lifespan = 5;
    [SerializeField] [Range(0.1f, 0.9f)] float oldAgeFactor = 0.5f;
    [SerializeField] [Min(1)] float eggDeathDelay = 5;
    [SerializeField] [Min(1)] float minHatchDelay = 2;
    [SerializeField] [Min(2)] float maxHatchDelay = 5;
    [SerializeField] Sprite[] orderedDeathSprites;
    [SerializeField] CreatureMovement movement;
#pragma warning restore CS0649

    // References
    Animator animator;
    ScaleManager scaleManager;
    Transform spitPoint;

    readonly Timer oldAgeTimer = new Timer();
    readonly Timer deathTimer = new Timer();

    // Player-specific
    PlayerController playerController;
    readonly Timer finalDeathTimer = new Timer();

    // Nonplayer-specific
    readonly Timer hatchTimer = new Timer();

    bool isPlayer;
    int deathType;

    #region Animator hashes

    readonly int hatchHash = Animator.StringToHash("Hatch");
    readonly int oldHash = Animator.StringToHash("Old");
    readonly int dieHash = Animator.StringToHash("Die");
    readonly int deathTypeHash = Animator.StringToHash("Death type");

    readonly int eggStateHash = Animator.StringToHash("Egg");

    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();
        scaleManager = FindObjectOfType<ScaleManager>();
        spitPoint = transform.GetChild(0);

        isPlayer = TryGetComponent(out playerController);

        Func<float> getInputDirection;
        if (isPlayer)
            getInputDirection = () => playerController.MoveAngle;
        else
            getInputDirection = null;
        movement.Initialize(lifespan, getInputDirection, transform);

        // Deactivate until needed
        gameObject.SetActive(false);
    }

    void Update()
    {
        movement.Update(GetAge());
        UpdateScale();
    }

    public void ActivateInstance()
    {
        gameObject.SetActive(true);

        InitializeTimers();
        SubscribeToEvents();
        InitializeAnimator();

        WaitToHatch();
    }

    void WaitToHatch()
    {
        if (isPlayer)
            StartCoroutine(WaitForPlayerToHatch());
        else
            hatchTimer.Start();
    }

    IEnumerator WaitForPlayerToHatch()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        Hatch();
    }

    void Hatch()
    {
        animator.SetBool(hatchHash, true);
        hatchTimer.Stop();
    }

    void BeginYoungAge(int gameObjectID)
    {
        if (gameObjectID != gameObject.GetInstanceID())
            return;

        oldAgeTimer.Start();
    }

    void BeginOldAge()
    {
        animator.SetBool(oldHash, true);
        oldAgeTimer.Stop();

        deathTimer.Start();
    }

    void BeginDeath()
    {
        animator.SetBool(dieHash, true);
        deathTimer.Stop();
    }

    void LayEgg(int gameObjectID)
    {
        if (gameObjectID != gameObject.GetInstanceID())
            return;

        GameObject egg = GetNewEggGameObject();

        if (egg == null)
            return;

        // Initialize new egg
        egg.GetComponent<CreatureController>().ActivateInstance();
        egg.transform.position = spitPoint.position;
        egg.transform.rotation = Quaternion.identity;
        if (isPlayer)
            CameraController.SetFollowTarget(egg.transform);

        // TODO: Add trajectory
    }

    void Die(int gameObjectID)
    {
        if (gameObjectID != gameObject.GetInstanceID())
            return;

        DisableInstance();
        TurnIntoDeadBody();
    }

    #region Initialization methods

    void InitializeTimers()
    {
        UnityEngine.Random.InitState(seed: DateTime.Now.Millisecond);

        StopTimers();

        oldAgeTimer.SetPeriod(oldAgeFactor * lifespan);
        deathTimer.SetPeriod((1 - oldAgeFactor) * lifespan);

        finalDeathTimer.SetPeriod(eggDeathDelay);
        hatchTimer.SetPeriod(UnityEngine.Random.Range(minHatchDelay, maxHatchDelay));
    }

    void SubscribeToEvents()
    {
        hatchTimer.OnTick += Hatch;
        StateEventManager.OnFinishedHatching += BeginYoungAge;
        oldAgeTimer.OnTick += BeginOldAge;
        deathTimer.OnTick += BeginDeath;
        StateEventManager.OnLayEgg += LayEgg;
        StateEventManager.OnDeath += Die;
    }

    void InitializeAnimator()
    {
        UnityEngine.Random.InitState(seed: DateTime.Now.Millisecond);
        deathType = UnityEngine.Random.Range(0, 3);

        animator.SetBool(hatchHash, false);
        animator.SetBool(dieHash, false);
        animator.SetBool(oldHash, false);
        animator.SetFloat(deathTypeHash, deathType);

        animator.Play(eggStateHash);
    }

    #endregion

    #region Disabling methods

    void DisableInstance()
    {
        StopTimers();
        UnsubscribeFromEvents();
        gameObject.SetActive(false);
    }

    void UnsubscribeFromEvents()
    {
        hatchTimer.OnTick -= Hatch;
        StateEventManager.OnFinishedHatching -= BeginYoungAge;
        oldAgeTimer.OnTick -= BeginOldAge;
        deathTimer.OnTick -= BeginDeath;
        StateEventManager.OnLayEgg -= LayEgg;
        StateEventManager.OnDeath -= Die;
    }

    #endregion

    #region Helper methods

    void StopTimers()
    {
        deathTimer.Stop();
        finalDeathTimer.Stop();
        hatchTimer.Stop();
        oldAgeTimer.Stop();
    }

    float GetAge()
    {
        return animator.GetBool(oldHash) ? oldAgeFactor * lifespan + deathTimer.ElapsedTime : oldAgeTimer.ElapsedTime;
    }

    GameObject GetNewEggGameObject()
    {
        return isPlayer ? GameManager.GetFreePlayerInstance(forced: true) : GameManager.GetFreeCreatureInstance();
    }

    void TurnIntoDeadBody()
    {
        GameManager.FreeInstance(gameObject);

        GameObject body = GameManager.GetFreeBodyInstance();

        if (body == null)
            return;

        body.SetActive(true);
        body.transform.position = transform.position;

        SpriteRenderer spriteRenderer = body.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = orderedDeathSprites[deathType];
        spriteRenderer.color = GetComponent<SpriteRenderer>().color;
    }

    void UpdateScale()
    {
        float targetScale = scaleManager.GetTargetScale(transform.position);
        transform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 minVector = new Vector3(Mathf.Cos(movement.MinAngle), Mathf.Sin(movement.MinAngle), 0);
        Gizmos.DrawLine(transform.position, transform.position + minVector);

        Gizmos.color = Color.magenta;
        Vector3 maxVector = new Vector3(Mathf.Cos(movement.MaxAngle), Mathf.Sin(movement.MaxAngle), 0);
        Gizmos.DrawLine(transform.position, transform.position + maxVector);
    }

    #endregion
}
