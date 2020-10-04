using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class CreatureController : MonoBehaviour
{
    public enum CreatureState { Alive, Egg, Extinct }

    [SerializeField] [Min(1)] float lifespan = 10;
    [SerializeField] [Min(0.01f)] float movementSpeed = 1f;
    [SerializeField] [Min(0.01f)] float franticness = 1f;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float minDirection = -Mathf.PI;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float maxDirection = Mathf.PI;
    [SerializeField] Sprite[] orderedDeathSprites;

    CreatureState state;
    readonly Timer timer = new Timer();

    Animator animator;

    PlayerController playerController;
    bool isPlayer;

    Collider2D collider;

    ScaleManager sm;
    Transform spitPoint;

    #region Non-player fields

    float nonPlayerMovementSeed;

    #endregion

    #region Animator hashes

    readonly int hatch = Animator.StringToHash("Hatch");
    readonly int die = Animator.StringToHash("Die");
    readonly int deathType = Animator.StringToHash("Death type");

    #endregion

    void Start()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
        sm = FindObjectOfType<ScaleManager>();

        isPlayer = TryGetComponent(out PlayerController playerController);
        if (isPlayer)
            this.playerController = playerController;
            
        nonPlayerMovementSeed = Random.Range(0, 100);

        InitializeAnimator();

        spitPoint = GetComponentInChildren<Transform>();

        if (minDirection > maxDirection)
        {
            float temp = minDirection;
            minDirection = maxDirection;
            maxDirection = temp;
        }

        SetState(CreatureState.Alive);

        StateEventManager.OnSpitEgg += SpitEgg;
    }

    void Update()
    {
        UpdateScale();

        switch (state)
        {
            case CreatureState.Egg:
                EggBehavior();
                break;
            case CreatureState.Alive:
                AliveBehavior();
                break;
        }
    }
    void OnDestroy()
    {
        StateEventManager.OnSpitEgg -= SpitEgg;
    }

    void ResetValues()
    {
        InitializeAnimator();
    }

    void Disable()
    {
        GameManager.FreeInstance(gameObject);
        
        GameObject body = GameManager.GetFreeBodyInstance();
        if (body != null)
        {
            body.transform.position = transform.position;
            body.GetComponent<SpriteRenderer>().sprite = orderedDeathSprites[(int)animator.GetFloat(deathType)];
        }

        enabled = false;
        isPlayer = false;
        StateEventManager.OnSpitEgg -= SpitEgg;
    }

    void EggBehavior()
    {
        if (isPlayer && Input.GetMouseButtonDown(0))
            SetState(CreatureState.Alive);
    }

    void AliveBehavior()
    {
        Move();
    }

    void UpdateScale()
    {
        float targetScale = sm.GetTargetScale(transform.position);
        transform.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    void SetState(CreatureState newState)
    {
        state = newState;

        switch (state)
        {
            case CreatureState.Egg:
                Debug.Log(gameObject.name + " died after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                animator.SetBool(die, true);

                if (!isPlayer)
                {
                    timer.SetPeriod(Random.Range(lifespan * 0.2f, lifespan * 1.4f));
                    timer.Start();

                    timer.OnTick -= DieAndBirthSelf;
                    timer.OnTick += Hatch;

                    return;
                }
                
                timer.SetPeriod(lifespan);
                timer.Start();

                timer.OnTick += EndDynasty;
                timer.OnTick -= DieAndBirthSelf;

                return;

            case CreatureState.Alive:
                Debug.Log(gameObject.name + " birthed itself after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                animator.SetBool(hatch, true);

                if (!isPlayer)
                {
                    timer.SetPeriod(Random.Range(lifespan * 0.5f, lifespan * 3f));
                    timer.Start();

                    timer.OnTick -= Hatch;
                }
                else
                {
                    timer.SetPeriod(lifespan - timer.ElapsedTime);
                    timer.Start();
                }

                timer.OnTick += DieAndBirthSelf;
                timer.OnTick -= EndDynasty;

                return;

            case CreatureState.Extinct:
                Debug.Log(gameObject.name + " ended its dynasty after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.Stop();

                if (!isPlayer)
                    return;

                timer.OnTick -= DieAndBirthSelf;
                timer.OnTick -= EndDynasty;

                return;
        }
    }

    void DieAndBirthSelf()
    {
        SetState(CreatureState.Egg);
    }

    void EndDynasty()
    {
        SetState(CreatureState.Extinct);
    }

    void Hatch()
    {
        SetState(CreatureState.Alive);
    }

    void Move()
    {
        transform.Translate(GetMoveDirection() * movementSpeed * Time.deltaTime);
    }

    void SpitEgg()
    {
        GameObject newEgg = isPlayer ? GameManager.GetFreePlayerInstance(forced: true) : GameManager.GetFreeCreatureInstance();

        if (newEgg == null)
            return;

        newEgg.transform.position = spitPoint.position;
        newEgg.transform.rotation = Quaternion.identity;

        // TODO: Add trajectory

        Disable();
    }

    #region Helper functions

    Vector2 GetMoveDirection()
    {
        if (playerController != null)
            return GetPlayerMoveDirection();
        else
            return GetRandomMoveDirection();
    }

    Vector2 GetPlayerMoveDirection()
    {
        float angle = playerController.MoveAngle;
        angle = angle.ClampAngleRad(minDirection, maxDirection);

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
#endif

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    Vector2 GetRandomMoveDirection()
    {
        float t = Mathf.PerlinNoise(Time.time * franticness + nonPlayerMovementSeed, 0);
        float angle = Mathf.Lerp(minDirection, maxDirection, t);

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
#endif

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void InitializeAnimator()
    {
        animator.SetBool(hatch, true);
        animator.SetBool(die, false);
        animator.SetFloat(deathType, Mathf.Floor(Random.Range(0, 3)));
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 minVector = new Vector3(Mathf.Cos(minDirection), Mathf.Sin(minDirection), 0);
        Gizmos.DrawLine(transform.position, transform.position + minVector);

        Gizmos.color = Color.magenta;
        Vector3 maxVector = new Vector3(Mathf.Cos(maxDirection), Mathf.Sin(maxDirection), 0);
        Gizmos.DrawLine(transform.position, transform.position + maxVector);
    }

    #endregion
}
