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

    float nonPlayerMovementSeed;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        isPlayer = TryGetComponent(out PlayerController playerController);
        if (isPlayer)
            this.playerController = playerController;
            
        nonPlayerMovementSeed = Random.Range(0, 100);
        
        SetState(CreatureState.Alive);
    }

    void Update()
    {
        if (state == CreatureState.Egg && isPlayer && Input.GetKeyDown(KeyCode.Alpha1))
            SetState(CreatureState.Alive);

        if (state == CreatureState.Alive)
            Move();

        if (minDirection > maxDirection)
        {
            float temp = minDirection;
            minDirection = maxDirection;
            maxDirection = temp;
        }
    }

    void SetState(CreatureState newState)
    {
        state = newState;

        switch (state)
        {
            case CreatureState.Egg:
                Debug.Log(gameObject.name + " died after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");
                
                timer.SetPeriod(lifespan);
                timer.Start();

                timer.OnTick += EndDynasty;
                timer.OnTick -= DieAndBirthSelf;

                spriteRenderer.sprite = eggSprite;

                return;

            case CreatureState.Alive:
                Debug.Log(gameObject.name + " birthed itself after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.SetPeriod(lifespan - timer.ElapsedTime);
                timer.Start();

                timer.OnTick += DieAndBirthSelf;
                timer.OnTick -= EndDynasty;

                spriteRenderer.sprite = livingSprite;

                return;

            case CreatureState.Extinct:
                Debug.Log(gameObject.name + " ended its dynasty " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.Stop();

                timer.OnTick -= DieAndBirthSelf;
                timer.OnTick -= EndDynasty;

                spriteRenderer.sprite = eggSprite;

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

    void Move()
    {
        if (playerController != null)
            transform.Translate(GetPlayerMoveDirection() * movementSpeed * Time.deltaTime);
        else
            transform.Translate(GetRandomMoveDirection() * movementSpeed * Time.deltaTime);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 minVector = new Vector3(Mathf.Cos(minDirection), Mathf.Sin(minDirection), 0);
        Gizmos.DrawLine(transform.position, transform.position + minVector);

        Gizmos.color = Color.magenta;
        Vector3 maxVector = new Vector3(Mathf.Cos(maxDirection), Mathf.Sin(maxDirection), 0);
        Gizmos.DrawLine(transform.position, transform.position + maxVector);
    }
}
