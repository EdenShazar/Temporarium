using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class Timer
{
    public float Period { get; private set; } = 1f / 30f;
    public float ElapsedTime { get; private set; }
    
    float startTime;


    bool enabled = false;

    public Action OnTick;

    /// <param name="tickPeriod">Time interval in seconds between calls to <see cref="OnTick"/></param>
    public void SetPeriod(float tickPeriod)
    {
        if (tickPeriod <= 0)
            throw new Exception("tickInterval must be positive");

        Period = tickPeriod;
    }

    public void Start()
    {
        if (Period == 0)
            throw new Exception("Initialize using SetInterval(float tickInterval) before calling Start()");

        if (!enabled)
            GameManager.Instance.StartCoroutine(Count());
        else
            ResetTime();
    }

    public void Stop()
    {
        enabled = false;
    }

    IEnumerator Count()
    {
        enabled = true;
        ResetTime();

        while (enabled)
        {
            ElapsedTime = Time.time - startTime;
            int ticks = Mathf.FloorToInt(ElapsedTime / Period);

            if (ElapsedTime > Period)
            {
                OnTick?.Invoke();
                // Reset time is not precise since it might overshoot the last tick, but it's good enough
                ResetTime();
            }

            yield return null;
        }
    }

    void ResetTime()
    {
        startTime = Time.time;
    }
}
