using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FishingHotspot : MonoBehaviour {

    #region Variables
    [Header("Optional")]
    [SerializeField] string hotspotName = "Hotspot";
    public string HotspotName => hotspotName;
    #endregion

    #region Unity Methods
    void Reset() {
        // Ensure the collider is set up as a trigger.
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        collider.isTrigger = true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.75f);
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null) Gizmos.DrawSphere(collider.center, collider.radius);
    }
#endif
    #endregion
}