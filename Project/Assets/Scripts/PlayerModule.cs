using UnityEngine;

public class PlayerModule
{
    public static CreatureController CurrentPlayer { get; private set; }
    public static bool IsCurrentPlayerHoldingGem { get; private set; }

    public bool enabled;
    public readonly Transform transform;

    public PlayerModule(Transform transform)
    {
        enabled = false;
        this.transform = transform;
    }

    public static float GetMoveAngle(Transform from)
    {
        Vector3 mousePosition = GameManager.camera.ScreenToWorldPoint(Input.mousePosition);
        return (mousePosition - from.position).ToVector2().GetAngleRad();
    }
    
    public static bool ClearCurrentPlayer(CreatureController clearer)
    {
        if (!clearer.IsPlayer)
            return false;

        CurrentPlayer = null;
        IsCurrentPlayerHoldingGem = false;
        clearer.ConvertToNonPlayer();
        GameManager.NotifyDeactivatedPlayer();
        CameraController.ActivateGemCamera();

        return true;
    }

    public static bool TryInitializeCurrentPlayer(CreatureController newPlayer, bool withGem)
    {
        if (CurrentPlayer != null)
            return false;

        CurrentPlayer = newPlayer;
        IsCurrentPlayerHoldingGem = withGem;
        newPlayer.ConvertToPlayer();
        GameManager.NotifyActivatedPlayer();

        return true;
    }

    public static bool PassOnPlayership(CreatureController newPlayer, CreatureController appointer, bool withGem)
    {
        if (!appointer.IsPlayer)
            return false;

        CurrentPlayer = newPlayer;
        IsCurrentPlayerHoldingGem = withGem;


        if (withGem)
        {
            RecordLostGem(loser: appointer.transform);
            newPlayer.ConvertToPlayer();
            RecordTakenGem(taker: newPlayer.transform);
            appointer.ConvertToNonPlayer();
        }
        else
        { 
            newPlayer.ConvertToPlayer();
            appointer.ConvertToNonPlayer();
        }

        return true;
    }

    public static void RecordTakenGem(Transform taker)
    {
        if (CurrentPlayer == null)
        {
            IsCurrentPlayerHoldingGem = false;
            return;
        }

        if (CurrentPlayer.transform == taker)
            IsCurrentPlayerHoldingGem = true;
    }

    public static void RecordLostGem(Transform loser)
    {
        if (CurrentPlayer == null)
        {
            IsCurrentPlayerHoldingGem = false;
            return;
        }

        if (CurrentPlayer.transform == loser)
            IsCurrentPlayerHoldingGem = false;
    }
}


