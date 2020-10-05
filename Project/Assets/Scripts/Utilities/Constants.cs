using UnityEngine;

public static class Constants
{
    public static readonly int maxCreatureInstances = 15;
    public static readonly int maxBodyInstances = 100;

    public static readonly int eggLayer = LayerMask.NameToLayer("Egg");
    public static readonly int creaturesLayer = LayerMask.NameToLayer("Creatures");
}
