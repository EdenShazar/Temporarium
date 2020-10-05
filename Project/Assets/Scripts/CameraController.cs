using Cinemachine;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CinemachineVirtualCamera PlayerCamera { get; private set; }
    public static CinemachineVirtualCamera GemCamera { get; private set; }

    const int activeCameraPriority = 20;
    const int inactiveCameraPriority = 10;
    
    void Awake()
    {
        CinemachineVirtualCamera[] cameras = FindObjectsOfType<CinemachineVirtualCamera>();
        PlayerCamera = cameras.Single(cam => cam.name == "Player camera");
        GemCamera = cameras.Single(cam => cam.name == "Gem camera");
    }

    void Update()
    {
        if (GameManager.GemHolder != null)
            GemCamera.Follow = GameManager.GemHolder;

        if (PlayerModule.CurrentPlayer != null)
            PlayerCamera.Follow = PlayerModule.CurrentPlayer.transform;
    }


    public static void ActivateGemCamera()
    {
        PlayerCamera.Priority = inactiveCameraPriority;
        GemCamera.Priority = activeCameraPriority;
    }

    public static void ActivatePlayerCamera()
    {
        PlayerCamera.Priority = activeCameraPriority;
        GemCamera.Priority = inactiveCameraPriority;
    }
}
