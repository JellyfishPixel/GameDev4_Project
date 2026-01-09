using UnityEngine;

public class DoorTeleport : MonoBehaviour
{
    public Transform spawnInside;
    public Transform spawnOutside;

    public bool playerIsInside; 
    Collider col;
    private void Start()
    {
        col = GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SceneTransitionManager.Instance.isTransitioning) return;

        if (playerIsInside)
        {
            SceneTransitionManager.Instance.Teleport(
                spawnOutside,
                CameraMode.ThirdPerson
            );
        }
        else
        {
            SceneTransitionManager.Instance.Teleport(
                spawnInside,
                CameraMode.FirstPerson
            );
        }

        playerIsInside = !playerIsInside;
    }
}
