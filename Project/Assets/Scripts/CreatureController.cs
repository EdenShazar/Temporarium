using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class CreatureController : MonoBehaviour
{
#pragma warning disable CS0649
    [SerializeField] [Min(1)] float lifespan = 5;
    [SerializeField] [Range(0.1f, 0.9f)] float oldAgeFactor = 0.5f;
    [SerializeField] [Min(1)] float eggDeathDelay = 5;
    [SerializeField] [Min(1)] float minHatchDelay = 2;
    [SerializeField] [Min(2)] float maxHatchDelay = 5;
    [SerializeField] [Min(1)] float spitStrength = 2;
    [SerializeField] [Min(1)] float spitDistance = 3;
    [SerializeField] Sprite[] orderedDeathSprites;
    [SerializeField] Sprite[] brickSprites;
    [SerializeField] Sprite gemSprite;
    [SerializeField] CreatureMovement movementModule;
#pragma warning restore CS0649

    Animator animator;
    new Rigidbody2D rigidbody;
    SpriteRenderer spriteRenderer;
    Transform spitPoint;
    Transform gemDropPoint;
    Collider2D regularCollider;
    Collider2D eggCollider;
    SpriteRenderer carriableSpriteRenderer;
    Light2D gemLight;

    PlayerModule playerModule;

    readonly Timer oldAgeTimer = new Timer();
    readonly Timer deathTimer = new Timer();
    readonly Timer finalDeathTimer = new Timer();
    readonly Timer hatchTimer = new Timer();

    int deathType;
    float eggCollisionHeight;
    Sprite brickSprite;
    
    bool isHoldingGem;

    bool IsPlayer { get => playerModule.enabled; }
    public bool IsSpottable { get; private set; }

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        spitPoint = transform.GetChild(0);
        gemDropPoint = transform.GetChild(1);
        eggCollider = transform.GetChild(2).GetComponent<Collider2D>();
        carriableSpriteRenderer = transform.GetChild(3).GetComponent<SpriteRenderer>();
        gemLight = transform.GetChild(3).GetChild(0).GetComponent<Light2D>();

        playerModule = new PlayerModule(transform: transform);

        Func<float> getInputDirection;
        if (IsPlayer)
            getInputDirection = playerModule.GetMoveAngle;
        else
            getInputDirection = null;
        movementModule.Initialize(lifespan, getInputDirection, transform);

        UnityEngine.Random.InitState(seed: DateTime.Now.Millisecond);
        brickSprite = brickSprites[UnityEngine.Random.Range(0, brickSprites.Length - 1)];

        // Deactivate until needed
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        float visibilityBuffer = IsPlayer ? 0 : 0.2f;
        if (!IsVisible(visibilityBuffer))
            DisableInstance();

        movementModule.Update(GetAge());
    }

    void OnMouseDown()
    {
        if (GameManager.IsSearchingForPlayer)
            ConvertToPlayer();
    }

    public void ActivateInstance(float eggCollisionHeight = Mathf.NegativeInfinity)
    {
        if (eggCollisionHeight == Mathf.NegativeInfinity)
            this.eggCollisionHeight = transform.position.y;
        else
            this.eggCollisionHeight = eggCollisionHeight;

        gameObject.SetActive(true);
        GameManager.NotifyEnabledCreatureInstance();

        InitializeTimers();
        SubscribeToEvents();
        InitializeAnimator();
        ActivateEggMode();

        CarryItem();

        WaitToHatch();
    }

    public void ConvertToPlayer()
    {
        if (GameManager.player != null)
            return;

        playerModule.enabled = true;

        if (isHoldingGem)
            CarryGem();
        else
            CarryNothing();

        if (hatchTimer.Enabled)
        {
            hatchTimer.Stop();
            WaitToHatch();
        }

        movementModule.NotifyConvertedToPlayer(playerModule.GetMoveAngle);
        CameraController.SetFollowTarget(transform);

        GameManager.NotifyActivatedPlayer(newPlayer: this);

        Debug.Log("Converted to player!");
    }

    public void ConvertToNonPlayer()
    {
        CarryBrick();

        playerModule.enabled = false;
        movementModule.NotifyConvertedToNonPlayer();

        GameManager.NotifyDeactivatedPlayer();

        Debug.Log("Converted back from player!");
    }

    void WaitToHatch()
    {
        if (IsPlayer)
            StartCoroutine(WaitForPlayerinputToHatch());
        else
            hatchTimer.Start();

        StartCoroutine(StopAtEggCollisionHeight());
    }

    IEnumerator WaitForPlayerinputToHatch()
    {
        finalDeathTimer.Start();

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || !gameObject.activeSelf);

        if (gameObject.activeSelf)
        {
            Hatch();
            finalDeathTimer.Stop();
        }
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

        GameObject egg = GameManager.GetFreeCreatureInstance();

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
        finalDeathTimer.OnTick += DisablePlayerOnFinalDeath;
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
        CreatureController eggCreatureController = egg.GetComponent<CreatureController>();

        float angle;
        if (IsPlayer)
        {
            angle = movementModule.GetPlayerMoveDirection().GetAngleRad();
            CameraController.SetFollowTarget(egg.transform);

            if (isHoldingGem)
                eggCreatureController.isHoldingGem = true;

            ConvertToNonPlayer();
            eggCreatureController.ConvertToPlayer();
        }
        else
            angle = UnityEngine.Random.Range(movementModule.MinAngle, movementModule.MaxAngle);

        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float upperAngle = (angle + 0.5f * Mathf.PI) / 2;
        Vector2 upperDirection = new Vector2(Mathf.Cos(upperAngle), Mathf.Sin(upperAngle));

        egg.transform.position = spitPoint.position;
        egg.transform.rotation = Quaternion.Euler(0, 0, angle - Mathf.PI);

        Rigidbody2D eggRigidbody = egg.GetComponent<Rigidbody2D>();
        eggRigidbody.velocity = upperDirection * spitStrength;
        eggRigidbody.angularVelocity = UnityEngine.Random.Range(180, 720);

        float collisionHeight = (transform.position.ToVector2() + direction.normalized * spitDistance).y;
        eggCreatureController.ActivateInstance(collisionHeight);
    }

    #endregion

    #region Disabling methods

    void DisableInstance()
    {
        StopTimers();
        UnsubscribeFromEvents();
        gameObject.SetActive(false);

        if (!IsPlayer)
            GameManager.NotifyDisabledCreatureInstance();
    }

    public void DisablePlayerOnFinalDeath()
    {
        StopTimers();
        UnsubscribeFromEvents();
        TurnIntoDeadBody();
        gameObject.SetActive(false);
        
        if (isHoldingGem)
            GameManager.unheldGem.DropAtPosition(gemDropPoint.position);
        GameManager.NotifyGemOwner(isPlayer: false);

        GameManager.NotifyDeactivatedPlayer();
    }

    public void Eaten()
    {
        StopTimers();
        UnsubscribeFromEvents();
        gameObject.SetActive(false);

        GameManager.NotifyDeactivatedPlayer();
    }

    void UnsubscribeFromEvents()
    {
        hatchTimer.OnTick -= Hatch;
        StateEventManager.OnFinishedHatching -= BeginYoungAge;
        oldAgeTimer.OnTick -= BeginOldAge;
        deathTimer.OnTick -= BeginDeath;
        StateEventManager.OnLayEgg -= LayEgg;
        StateEventManager.OnDeath -= Die;
        finalDeathTimer.OnTick -= DisablePlayerOnFinalDeath;
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

    void TurnIntoDeadBody()
    {
        GameObject body = GameManager.GetFreeBodyInstance();

        if (body == null)
            return;

        body.SetActive(true);
        body.transform.position = transform.position;

        SpriteRenderer spriteRenderer = body.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = orderedDeathSprites[deathType];
        spriteRenderer.color = GetComponent<SpriteRenderer>().color;
    }

    void DeactivateEggMode()
    {
        regularCollider.enabled = true;
        eggCollider.enabled = false;

        gameObject.layer = IsPlayer ? Constants.playerLayer : Constants.creaturesLayer;
        transform.rotation = Quaternion.identity;
        rigidbody.gravityScale = 0;
        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;
        rigidbody.freezeRotation = true;

        IsSpottable = isHoldingGem;
    }

    void ActivateEggMode()
    {
        regularCollider.enabled = false;
        eggCollider.enabled = true;

        gameObject.layer = Constants.eggLayer;
        rigidbody.gravityScale = 1;
        rigidbody.freezeRotation = false;

        IsSpottable = false;
    }

    public bool IsVisible(float buffer = 0)
    {
        Vector2 point = GameManager.camera.WorldToViewportPoint(spriteRenderer.bounds.min);
        if (point.x > 1 + buffer || point.y > 1 + buffer)
            return false;

        point = GameManager.camera.WorldToViewportPoint(spriteRenderer.bounds.max);
        if (point.x < -buffer || point.y < -buffer)
            return false;

        return true;
    }

    void CarryItem()
    {
        if (!IsPlayer)
            CarryBrick();
        else if (isHoldingGem)
            CarryGem();
        else
            CarryNothing();
    }

    void CarryBrick()
    {
        carriableSpriteRenderer.enabled = true;
        carriableSpriteRenderer.sprite = brickSprite;
        gemLight.enabled = false;
        isHoldingGem = false;
    }

    public void CarryGem()
    {
        carriableSpriteRenderer.enabled = true;
        carriableSpriteRenderer.sprite = gemSprite;
        GameManager.GemHolder = transform;
        gemLight.enabled = true;
        isHoldingGem = true;
        GameManager.NotifyGemOwner(isPlayer: true);
    }

    void CarryNothing()
    {
        carriableSpriteRenderer.enabled = false;
        gemLight.enabled = false;
        isHoldingGem = false;
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 minVector = new Vector3(Mathf.Cos(movementModule.MinAngle), Mathf.Sin(movementModule.MinAngle), 0);
        Gizmos.DrawLine(transform.position, transform.position + minVector);

        Gizmos.color = Color.magenta;
        Vector3 maxVector = new Vector3(Mathf.Cos(movementModule.MaxAngle), Mathf.Sin(movementModule.MaxAngle), 0);
        Gizmos.DrawLine(transform.position, transform.position + maxVector);
    }

    #endregion
}
