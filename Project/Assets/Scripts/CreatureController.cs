using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CreatureController : MonoBehaviour
{
    public enum CreatureState { Alive, Egg, Extinct }

    [SerializeField] [Min(1)] float lifespan = 10;
    [SerializeField] [Min(0.01f)] float movementSpeed = 1f;
    [SerializeField] [Min(0.01f)] float franticness = 1f;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float minDirection = -Mathf.PI;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float maxDirection = Mathf.PI;
    [SerializeField] Sprite livingSprite;
    [SerializeField] Sprite eggSprite;

    CreatureState state;
    readonly Timer timer = new Timer();

    SpriteRenderer spriteRenderer;

    PlayerController playerController;
    bool isPlayer;

    #region Non-player fields

    float nonPlayerMovementSeed;

    #endregion

    ScaleManager sm; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sm = FindObjectOfType<ScaleManager>();

        isPlayer = TryGetComponent(out PlayerController playerController);
        if (isPlayer)
            this.playerController = playerController;
            
        nonPlayerMovementSeed = Random.Range(0, 100);
        
        SetState(CreatureState.Alive);
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
            case CreatureState.Extinct:
                ExtinctBehavior();
                break;
        }

        if (minDirection > maxDirection)
        {
            float temp = minDirection;
            minDirection = maxDirection;
            maxDirection = temp;
        }
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

    void ExtinctBehavior()
    {

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

                if (!isPlayer)
                {
                    timer.SetPeriod(Random.Range(lifespan * 0.2f, lifespan * 0.4f));
                    timer.Start();

                    timer.OnTick -= DieAndBirthSelf;
                    timer.OnTick += Hatch;
                    
                    spriteRenderer.sprite = eggSprite;

                    return;
                }
                
                timer.SetPeriod(lifespan);
                timer.Start();

                spriteRenderer.sprite = eggSprite;

                timer.OnTick += EndDynasty;
                timer.OnTick -= DieAndBirthSelf;

                return;

            case CreatureState.Alive:
                Debug.Log(gameObject.name + " birthed itself after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                if (!isPlayer)
                {
                    timer.SetPeriod(Random.Range(lifespan * 0.7f, lifespan * 1.3f));
                    timer.Start();

                    timer.OnTick -= Hatch;
                }
                else
                {
                    timer.SetPeriod(lifespan - timer.ElapsedTime);
                    timer.Start();
                }

                spriteRenderer.sprite = livingSprite;

                timer.OnTick += DieAndBirthSelf;
                timer.OnTick -= EndDynasty;

                return;

            case CreatureState.Extinct:
                Debug.Log(gameObject.name + " ended its dynasty after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.Stop();

                spriteRenderer.sprite = eggSprite;

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
        if (playerController != null)
            transform.Translate(GetPlayerMoveDirection() * movementSpeed * Time.deltaTime);
        else
            transform.Translate(GetRandomMoveDirection() * movementSpeed * Time.deltaTime);
    }

    #region Helper functions

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
