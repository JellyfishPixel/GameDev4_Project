using UnityEngine;

public class ShopCameraZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SceneTransitionManager.Instance.isTransitioning) return;

        SceneTransitionManager.Instance.SetShopState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (SceneTransitionManager.Instance.isTransitioning) return;

        SceneTransitionManager.Instance.SetShopState(false);
    }

}
