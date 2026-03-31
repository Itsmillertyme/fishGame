using UnityEngine;

public class PlayerCosmeticApplier : MonoBehaviour
{
    [SerializeField] private PlayerDataRuntime playerData;
    [SerializeField] private Transform hatAnchor;
    [SerializeField] private Transform shirtAnchor;
    [SerializeField] private Transform bootAnchor;

    private GameObject currentHat;
    private GameObject currentShirt;
    private GameObject currentBoot;

    public void RefreshVisuals()
    {
        ApplyCosmetic(CosmeticType.PlayerHat, ref currentHat, hatAnchor);
        ApplyCosmetic(CosmeticType.PlayerShirt, ref currentShirt, shirtAnchor);
        ApplyCosmetic(CosmeticType.PlayerBoots, ref currentBoot, bootAnchor);
    }

    private void ApplyCosmetic(CosmeticType type, ref GameObject currentInstance, Transform anchor)
    {
        if (anchor == null)
            return;

        if (currentInstance != null)
            Destroy(currentInstance);

        var def = playerData.GetEquippedCosmetic(type);
        if (def == null || def.Prefab == null)
            return;

        currentInstance = Instantiate(def.Prefab, anchor);
        currentInstance.transform.localPosition = Vector3.zero;
        currentInstance.transform.localRotation = Quaternion.identity;
    }
}
