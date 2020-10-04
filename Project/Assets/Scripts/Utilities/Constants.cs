using UnityEngine;

public static class Constants
{
    public static readonly int maxPlayerInstances = 3;
    public static readonly int maxCreatureInstances = 30;
    public static readonly int maxBodyInstances = 200;

    public static readonly int eggLayer = LayerMask.NameToLayer("Egg");
    public static readonly int creaturesLayer = LayerMask.NameToLayer("Creatures");
}
