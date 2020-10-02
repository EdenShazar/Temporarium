using UnityEngine;

public static class FloatExtensionMethods
{
    /// <summary>
    /// Returns the equivalent angle between -180 and 180 degrees.
    /// </summary>
    public static float WrapAngleDeg(this float value)
    {
        while (value <= -180) value += 360;
        while (value > 180) value -= 360;
        return value;
    }

    /// <summary>
    /// Returns the equivalent angle between -PI and PI radians.
    /// </summary>
    public static float WrapAngleRad(this float value)
    {
        float twoPi = 2 * Mathf.PI;
        while (value <= -Mathf.PI) value += twoPi;
        while (value > Mathf.PI) value -= twoPi;
        return value;
    }
}
