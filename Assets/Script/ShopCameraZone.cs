using UnityEngine;

public class ShopCameraZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SceneTransitionManager.Instance?.ForceFirstPerson(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SceneTransitionManager.Instance?.ForceFirstPerson(false);
    }
}
