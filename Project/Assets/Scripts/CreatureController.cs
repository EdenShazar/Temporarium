//using UnityEngine;

//[RequireComponent(typeof(SpriteRenderer))]
//[RequireComponent(typeof(Animator))]
//public class CreatureController : MonoBehaviour
//{
//    public enum CreatureState { Alive, Egg }//, Dead }

//    #region Inspector fields

//    [SerializeField] [Min(1)] float lifespan = 10;
//    [SerializeField] [Min(0.01f)] float movementSpeed = 1f;
//    [SerializeField] [Min(0.01f)] float franticness = 1f;
//    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float minDirection = -Mathf.PI;
//    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float maxDirection = Mathf.PI;
//    [SerializeField] Sprite[] orderedDeathSprites;

//    #endregion

//    #region Reference fields

//    Animator animator;
//    PlayerController playerController;
//    new Collider2D collider;
//    ScaleManager sm;
//    Transform spitPoint;

//    #endregion

//    CreatureState state;
//    readonly Timer lifespanTimer = new Timer();
//    readonly Timer extinctionTimer = new Timer();

//    bool isPlayer;

//    #region Non-player fields

//    float nonPlayerMovementSeed;
//    readonly Timer naturalHatchTimer = new Timer();

//    #endregion

//    #region Animator hashes

//    readonly int hatch = Animator.StringToHash("Hatch");
//    readonly int die = Animator.StringToHash("Die");
//    readonly int deathType = Animator.StringToHash("Death type");

//    #endregion

//    void Start()
//    {
//        Initialize();
//    }

//    void Update()
//    {
//        UpdateScale();

//        switch (state)
//        {
//            case CreatureState.Egg:
//                EggBehavior();
//                break;
//            case CreatureState.Alive:
//                AliveBehavior();
//                break;
//        }
//    }
//    void OnDestroy()
//    {
//        StateEventManager.OnLayEgg -= SpitEgg;
//        StateEventManager.OnDeath -= TurnIntoDeadBody;
//        lifespanTimer.OnTick -= DieAndBirthSelf;
//        naturalHatchTimer.OnTick -= Hatch;
//        //extinctionTimer.OnTick += TurnIntoDeadBody;
//    }

//    void EggBehavior()
//    {
//        if (isPlayer && Input.GetMouseButtonDown(0))
//            SetState(CreatureState.Alive);

//        // Non-players use timer to hatch
//    }

//    void AliveBehavior()
//    {
//        Move();
//    }

//    void UpdateScale()
//    {
//        float targetScale = sm.GetTargetScale(transform.position);
//        transform.localScale = new Vector3(targetScale, targetScale, 1f);
//    }

//    void SetState(CreatureState newState)
//    {
//        state = newState;

//        switch (state)
//        {
//            case CreatureState.Egg:
//                SwitchToEgg();
//                return;

//            case CreatureState.Alive:
//                SwitchToAlive();
//                return;

//            //case CreatureState.Dead:
//            //    SwitchToDead();
//            //    return;
//        }
//    }

//    void DieAndBirthSelf()
//    {
//        animator.SetBool(die, true);
//        //SetState(CreatureState.Dead);
//    }

//    //void EndDynasty()
//    //{
//    //    SetState(CreatureState.Dead);
//    //}

//    void Hatch()
//    {
//        SetState(CreatureState.Alive);
//    }

//    void Move()
//    {
//        transform.Translate(GetMoveDirection() * movementSpeed * Time.deltaTime);
//    }

//    void SpitEgg()
//    {
//        GameObject newEgg = isPlayer ? GameManager.GetFreePlayerInstance(forced: true) : GameManager.GetFreeCreatureInstance();

//        if (newEgg == null)
//            return;

//        newEgg.SetActive(true);
//        newEgg.transform.position = spitPoint.position;
//        newEgg.transform.rotation = Quaternion.identity;

//        newEgg.GetComponent<CreatureController>().ReinitializeInstance();

//        // TODO: Add trajectory
//    }

//    void TurnIntoDeadBody()
//    {
//        ResetValues();
//        GameManager.FreeInstance(gameObject);

//        // Replace with body if and instance is available
//        GameObject body = GameManager.GetFreeBodyInstance();
//        if (body != null)
//        {
//            body.transform.position = transform.position;
//            body.GetComponent<SpriteRenderer>().sprite = orderedDeathSprites[(int)animator.GetFloat(deathType)];
//        }
//    }

//    #region State switch methods

//    void SwitchToEgg()
//    {
//        if (!isPlayer)
//        {
//            naturalHatchTimer.Start();

//            // Ensure other timers are not interfering
//            lifespanTimer.Stop();
//            extinctionTimer.Stop();

//            return;
//        }

//        extinctionTimer.Start();

//        // Ensure other timers are not interfering
//        lifespanTimer.Stop();
//        naturalHatchTimer.Stop();
//    }

//    void SwitchToAlive()
//    {
//        Debug.Log(gameObject.name + " birthed itself after " + System.Math.Round(isPlayer ? extinctionTimer.ElapsedTime : naturalHatchTimer.ElapsedTime, 2) + " seconds!");

//        animator.SetBool(hatch, true);

//        lifespanTimer.Start();

//        // Ensure other timers are not interfering
//        naturalHatchTimer.Stop();
//        extinctionTimer.Stop();
//    }

//    //void SwitchToDead()
//    //{
//    //    Debug.Log(gameObject.name + " died after " + System.Math.Round(extinctionTimer.ElapsedTime, 2) + " seconds!");

//    //    animator.SetBool(die, true);
//    //}

//    #endregion

//    #region Initialization methods

//    public void Initialize(bool active = false)
//    {
//        // References
//        animator = GetComponent<Animator>();
//        collider = GetComponent<Collider2D>();
//        sm = FindObjectOfType<ScaleManager>();
//        spitPoint = GetComponentInChildren<Transform>();

//        isPlayer = TryGetComponent(out PlayerController playerController);

//        // Player data
//        if (isPlayer)
//            this.playerController = playerController;

//        // Non-player data
//        nonPlayerMovementSeed = Random.Range(0, 100);

//        // Initialization
//        InitializeAnimator();
//        EnsureMinMaxDirections();
//        SetState(active ? CreatureState.Alive : CreatureState.Egg);

//        // Timers
//        float currentGenerationLifespan = isPlayer ? lifespan : Random.Range(lifespan * 0.5f, lifespan * 3f);
//        lifespanTimer.SetPeriod(currentGenerationLifespan);
//        naturalHatchTimer.SetPeriod(Random.Range(lifespan * 0.2f, lifespan * 1.4f));
//        extinctionTimer.SetPeriod(lifespan);

//        // Event subscription
//        StateEventManager.OnLayEgg += SpitEgg;
//        StateEventManager.OnDeath += TurnIntoDeadBody;
//        lifespanTimer.OnTick += DieAndBirthSelf;
//        naturalHatchTimer.OnTick += Hatch;
//        //extinctionTimer.OnTick += TurnIntoDeadBody;
//    }

//    public void ReinitializeInstance()
//    {
//        InitializeAnimator();

//        float currentGenerationLifespan = isPlayer ? lifespan : Random.Range(lifespan * 0.5f, lifespan * 3f);
//        lifespanTimer.SetPeriod(currentGenerationLifespan);
//        naturalHatchTimer.SetPeriod(Random.Range(lifespan * 0.2f, lifespan * 1.4f));

//        SetState(CreatureState.Egg);
//    }

//    void InitializeAnimator()
//    {
//        animator.SetBool(hatch, false);
//        animator.SetBool(die, false);
//        animator.SetFloat(deathType, Mathf.Floor(Random.Range(0, 3)));
//    }

//    void ResetValues()
//    {
//        lifespanTimer.Stop();
//        naturalHatchTimer.Stop();
//        extinctionTimer.Stop();

//        InitializeAnimator();
//    }

//    #endregion

//    #region Helper methods

//    Vector2 GetMoveDirection()
//    {
//        if (playerController != null)
//            return GetPlayerMoveDirection();
//        else
//            return GetRandomMoveDirection();
//    }

//    Vector2 GetPlayerMoveDirection()
//    {
//        float angle = playerController.MoveAngle;
//        angle = angle.ClampAngleRad(minDirection, maxDirection);

//#if UNITY_EDITOR
//        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
//#endif

//        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
//    }
    
//    Vector2 GetRandomMoveDirection()
//    {
//        float t = Mathf.PerlinNoise(Time.time * franticness + nonPlayerMovementSeed, 0);
//        float angle = Mathf.Lerp(minDirection, maxDirection, t);

//#if UNITY_EDITOR
//        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
//#endif

//        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
//    }

//    #endregion

//    #region Gizmos

//    void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.cyan;
//        Vector3 minVector = new Vector3(Mathf.Cos(minDirection), Mathf.Sin(minDirection), 0);
//        Gizmos.DrawLine(transform.position, transform.position + minVector);

//        Gizmos.color = Color.magenta;
//        Vector3 maxVector = new Vector3(Mathf.Cos(maxDirection), Mathf.Sin(maxDirection), 0);
//        Gizmos.DrawLine(transform.position, transform.position + maxVector);
//    }

//    #endregion
//}
