using Cinemachine;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    static CinemachineVirtualCamera playerCamera;
    static CinemachineVirtualCamera searchCamera;

    const int activeCameraPriority = 20;
    const int inactiveCameraPriority = 10;
    
    void Awake()
    {
        CinemachineVirtualCamera[] cameras = FindObjectsOfType<CinemachineVirtualCamera>();
        playerCamera = cameras.Single(cam => cam.name == "Player camera");
        searchCamera = cameras.Single(cam => cam.name == "Search camera");
    }

    public static void SetFollowTarget(Transform target)
    {
        playerCamera.Follow = target;
    }

    public static void SearchForPlayer()
    {
        playerCamera.Priority = inactiveCameraPriority;
        searchCamera.Priority = activeCameraPriority;
    }

    public static void StopSearchForPlayer()
    {
        playerCamera.Priority = activeCameraPriority;
        searchCamera.Priority = inactiveCameraPriority;
    }
}
