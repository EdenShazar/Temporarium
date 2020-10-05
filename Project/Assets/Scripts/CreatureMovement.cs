using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureMovement
{
#pragma warning disable CS0649
    [SerializeField] AnimationCurve speedOverLife;
    [SerializeField] [Min(0.01f)] float maxSpeed = 1f;
    [SerializeField] [Min(0.01f)] float franticness = 1f;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float minAngle = -Mathf.PI;
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] float maxAngle = Mathf.PI;
#pragma warning restore CS0649

    float lifespan;
    Transform transform;
    CreatureController creature;

    float previousAge = 0;
    float age = 0;

    float seed;

    public float MinAngle { get => minAngle; }
    public float MaxAngle { get => maxAngle; }

    public void Initialize(float lifespan, Transform transform)
    {
        this.lifespan = lifespan;
        this.transform = transform;

        creature = transform.GetComponent<CreatureController>();

        seed = UnityEngine.Random.Range(0f, 100f);

        NormalizeSpeedCurve();
        EnsureMinMaxDirections();
    }

    public void Update(float age)
    {
        if (age > lifespan || age == 0)
        {
            ResetAge(0);
            return;
        }

        if (age < this.age)
        {
            ResetAge(age);
            return;
        }

        previousAge = this.age;
        this.age = age;

        Move();
    }

    void Move()
    {
        float deltaTime = age - previousAge;
        transform.Translate(GetCurrentDirection() * GetCurrentSpeed() * deltaTime);
    }

    float GetCurrentSpeed()
    {
        return speedOverLife.Evaluate(Mathf.Clamp(age, 0, lifespan));
    }

    Vector2 GetCurrentDirection()
    {
        if (creature.IsPlayer)
            return GetPlayerMoveDirection();
        else
            return GetRandomMoveDirection();
    }

    Vector2 GetRandomMoveDirection()
    {
        float t = Mathf.PerlinNoise(Time.time * franticness + seed, 0);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
#endif

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public Vector2 GetPlayerMoveDirection()
    {
        float angle = PlayerModule.GetMoveAngle(from: transform);
        angle = angle.ClampAngleRad(minAngle, maxAngle);

#if UNITY_EDITOR
        Debug.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
#endif

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void NormalizeSpeedCurve()
    {
        float maxTime = speedOverLife.keys[speedOverLife.length - 1].time;

        float maxValue = Mathf.NegativeInfinity;
        foreach (var key in speedOverLife.keys)
            if (key.value > maxValue)
                maxValue = key.value;

        float timeFactor = lifespan / maxTime;
        float valueFactor = maxSpeed / maxValue;

        List<Keyframe> newKeys = new List<Keyframe>();
        for (int i = 0; i < speedOverLife.length; i++)
        {
            Keyframe newKey = new Keyframe()
            {
                inTangent = speedOverLife.keys[i].inTangent,
                outTangent = speedOverLife.keys[i].outTangent,
                inWeight = speedOverLife.keys[i].inWeight,
                outWeight = speedOverLife.keys[i].outWeight,
                time = speedOverLife.keys[i].time * timeFactor,
                value = speedOverLife.keys[i].value * valueFactor
            };

            newKeys.Add(newKey); 
        }

        speedOverLife.keys = newKeys.ToArray();
    }

    void EnsureMinMaxDirections()
    {
        if (minAngle <= maxAngle)
            return;

        float temp = minAngle;
        minAngle = maxAngle;
        maxAngle = temp;
    }

    void ResetAge(float initialAge)
    {
        age = initialAge;
        previousAge = initialAge;
    }
}
