using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public enum CreatureState { Alive, Egg, Extinct }

    [Min(0.1f)] [SerializeField] float lifespan = 10;

    CreatureState state;
    readonly Timer timer = new Timer();

    void Start()
    {
        SetState(CreatureState.Alive);
    }

    void Update()
    {
        if (state == CreatureState.Egg && Input.GetKeyDown(KeyCode.Alpha1))
            SetState(CreatureState.Alive);
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
