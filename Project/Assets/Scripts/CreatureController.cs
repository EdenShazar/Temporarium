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
    [SerializeField] [Min(1)] float spitStrength = 2;
    [SerializeField] [Min(1)] float spitDistance = 3;
    [SerializeField] Sprite[] orderedDeathSprites;
    [SerializeField] CreatureMovement movement;
#pragma warning restore CS0649

    // References
    Animator animator;
    new Rigidbody2D rigidbody;
    ScaleManager scaleManager;
    Transform spitPoint;
    Collider2D regularCollider;
    Collider2D eggCollider;

    readonly Timer oldAgeTimer = new Timer();
    readonly Timer deathTimer = new Timer();

    // Player-specific
    PlayerController playerController;
    readonly Timer finalDeathTimer = new Timer();

    // Nonplayer-specific
    readonly Timer hatchTimer = new Timer();

    bool isPlayer;
    int deathType;
    float eggCollisionHeight;

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
        rigidbody = GetComponent<Rigidbody2D>();
        regularCollider = GetComponent<Collider2D>();
        scaleManager = FindObjectOfType<ScaleManager>();
        spitPoint = transform.GetChild(0);
        eggCollider = transform.GetChild(1).GetComponent<Collider2D>();

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

    public void ActivateInstance(float eggCollisionHeight = Mathf.NegativeInfinity)
    {
        if (eggCollisionHeight == Mathf.NegativeInfinity)
            this.eggCollisionHeight = transform.position.y;
        else
            this.eggCollisionHeight = eggCollisionHeight;

        gameObject.SetActive(true);

        InitializeTimers();
        SubscribeToEvents();
        InitializeAnimator();
        ActivateEggMode();

        WaitToHatch();
    }

    void WaitToHatch()
    {
        if (isPlayer)
            StartCoroutine(WaitForPlayerinputToHatch());
        else
            hatchTimer.Start();

        StartCoroutine(StopAtEggCollisionHeight());
    }

    IEnumerator WaitForPlayerinputToHatch()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        Hatch();
    }

    IEnumerator StopAtEggCollisionHeight()
    {
        yield return new WaitUntil(() => transform.position.y <= eggCollisionHeight && rigidbody.velocity.y <= 0);
        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;
        rigidbody.gravityScale = 0;
    }

    void Hatch()
    {
        animator.SetBool(hatchHash, true);
        DeactivateEggMode();
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

        InitializeNewEgg(egg);

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

    void InitializeNewEgg(GameObject egg)
    {
        egg.SetActive(true);

        float angle;
        if (isPlayer)
        {
            angle = playerController.MoveAngle;
            CameraController.SetFollowTarget(egg.transform);
        }
        else
            angle = UnityEngine.Random.Range(movement.MinAngle, movement.MaxAngle);

        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float upperAngle = (angle + 0.5f * Mathf.PI) / 2;
        Vector2 upperDirection = new Vector2(Mathf.Cos(upperAngle), Mathf.Sin(upperAngle));

        egg.transform.position = spitPoint.position;
        egg.transform.rotation = Quaternion.Euler(0, 0, angle - Mathf.PI);

        Rigidbody2D eggRigidbody = egg.GetComponent<Rigidbody2D>();
        eggRigidbody.velocity = upperDirection * spitStrength;
        eggRigidbody.angularVelocity = UnityEngine.Random.Range(180, 720);

        float collisionHeight = (transform.position.ToVector2() + direction.normalized * spitDistance).y;
        egg.GetComponent<CreatureController>().ActivateInstance(collisionHeight);
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

    void DeactivateEggMode()
    {
        regularCollider.enabled = true;
        eggCollider.enabled = false;

        gameObject.layer = Constants.creaturesLayer;
        transform.rotation = Quaternion.identity;
        rigidbody.gravityScale = 0;
        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;
    }

    void ActivateEggMode()
    {
        regularCollider.enabled = false;
        eggCollider.enabled = true;

        gameObject.layer = Constants.eggLayer;
        rigidbody.gravityScale = 1;
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
