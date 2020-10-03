using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public enum CreatureState { Alive, Egg, Extinct }

    [SerializeField] [Min(1)] float lifespan = 10;
    [SerializeField] [Min(0.01f)] float speed = 1f;

    CreatureState state;
    readonly Timer timer = new Timer();

    PlayerController playerController;
    float movementSeed;

    void Start()
    {
        SetState(CreatureState.Alive);

        if (TryGetComponent(out PlayerController playerController))
            this.playerController = playerController;

        movementSeed = Random.Range(0, 100);
    }

    void Update()
    {
        if (state == CreatureState.Egg && Input.GetKeyDown(KeyCode.Alpha1))
            SetState(CreatureState.Alive);

        // Move
        if (playerController != null)
            transform.Translate(playerController.MoveDirection * speed * Time.deltaTime);
        else
        {
            float angle = Mathf.Lerp(-Mathf.PI, Mathf.PI, Mathf.PerlinNoise(Time.time + movementSeed, 0));
            Vector2 moveDirection = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle));
            transform.Translate(moveDirection * speed * Time.deltaTime);
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
                return;

            case CreatureState.Alive:
                Debug.Log(gameObject.name + " birthed itself after " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.SetPeriod(lifespan - timer.ElapsedTime);
                timer.Start();

                timer.OnTick += DieAndBirthSelf;
                timer.OnTick -= EndDynasty;
                return;

            case CreatureState.Extinct:
                Debug.Log(gameObject.name + " ended its dynasty " + System.Math.Round(timer.ElapsedTime, 2) + " seconds!");

                timer.Stop();

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
}
