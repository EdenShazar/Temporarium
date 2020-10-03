﻿using UnityEngine;

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

public static class Vector3ExtensionMethods
{
    /// <summary>
    /// Return the xy components of the vector as a <see cref="Vector2"/>.
    /// </summary>
    public static Vector2 ToVector2(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }
}

public static class Vector2ExtensionMethods
{
    /// <summary>
    /// Return the vector as a <see cref="Vector3"/> in the xy plane.
    /// </summary>
    public static Vector3 ToVector3(this Vector2 vec)
    {
        return new Vector3(vec.x, vec.y, 0);
    }

    /// <summary>
    /// Return angle relative to the positive x axis in radians, between -PI and PI.
    /// </summary>
    public static float GetAngleRad(this Vector2 vec)
    {
        return Mathf.Atan2(vec.y, vec.x);
    }

    /// <summary>
    /// Return angle relative to the positive x axis in degrees, between -PI and PI.
    /// </summary>
    public static float GetAngleDeg(this Vector2 vec)
    {
        return vec.GetAngleRad() * Mathf.Rad2Deg;
    }
}

