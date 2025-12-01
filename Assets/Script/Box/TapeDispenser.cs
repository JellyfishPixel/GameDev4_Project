using UnityEngine;

public class TapeDispenser : MonoBehaviour, IInteractable
{
    public Material tapeMaterial;

    [Header("Tape Config")]
    public TapeColor tapeColor = TapeColor.Red;

    public void Interact(PlayerInteractionSystem interactor)
    {
        var eco = EconomyManager.Instance;
        if (eco != null && !eco.HasTapeUse(tapeColor))
        {
            Debug.Log("[TapeDispenser] No tape left.");
            AddSalesPopupUI.ShowMessage("No tape left.\nPlease buy more tape rolls at the shop.");
            return;
        }

        var tape = FindFirstObjectByType<TapeDragScaler>();

        if (!tape)
        {
            Debug.LogWarning("[TapeDispenser] ไม่พบ TapeDragScaler");
            return;
        }

        tape.SelectDispenser(this);
        Debug.Log($"[TapeDispenser] Selected: {name}");
    }

    public Material GetMaterial()
    {
        return tapeMaterial;
    }
}
