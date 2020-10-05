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
        GemCamera = cameras.Single(cam => cam.name == "Search camera");
    }

    public static void SetFollowTarget(Transform target)
    {
        PlayerCamera.Follow = target;
    }

    public static void ActivateGemCamera()
    {
        PlayerCamera.Priority = inactiveCameraPriority;
        GemCamera.Priority = activeCameraPriority;

        GemCamera.Follow = GameManager.GemHolder;
    }

    public static void ActivatePlayerCamera()
    {
        PlayerCamera.Priority = activeCameraPriority;
        GemCamera.Priority = inactiveCameraPriority;
    }
}
