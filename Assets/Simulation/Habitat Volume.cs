using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class HabitatVolume : MonoBehaviour {

    #region Variables
    [SerializeField] Vector3 swimArea;
    [SerializeField] Vector3 centerOffset;

    [SerializeField] float wallThickness;

    [SerializeField] bool showBottom;
    [SerializeField] bool showFront;
    [SerializeField] bool showBack;
    [SerializeField] bool showLeft;
    [SerializeField] bool showRight;

    private BoxCollider boxCollider;
    #endregion

    #region Unity Methods
    private void Awake() {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.center = centerOffset;
        boxCollider.size = swimArea;
    }

    private void Update() {
        boxCollider.center = centerOffset;
        boxCollider.size = swimArea;
    }

    void OnDrawGizmosSelected() {

        //Setup
        Gizmos.color = Color.cyan;
        Vector3 swimCenter = transform.position + centerOffset;

        //Swim area
        Gizmos.DrawWireCube(swimCenter, swimArea);

        Gizmos.color = Color.white;

        //draw bottom
        if (showBottom) {
            float bottomX = swimCenter.x;
            float bottomY = swimCenter.y - (swimArea.y / 2) - wallThickness / 2;
            float bottomZ = swimCenter.z;

            float bottomSizeX = swimArea.x + 2 * wallThickness;
            float bottomSizeY = wallThickness;
            float bottomSizeZ = swimArea.z + 2 * wallThickness;

            Gizmos.DrawCube(new Vector3(bottomX, bottomY, bottomZ), new Vector3(bottomSizeX, bottomSizeY, bottomSizeZ));
        }

        //draw X axis - Left
        if (showLeft) {
            float leftX = swimCenter.x - (swimArea.x / 2) - wallThickness / 2;
            float leftY = swimCenter.y;
            float leftZ = swimCenter.z;

            float leftSizeX = wallThickness;
            float leftSizeY = swimArea.y;
            float leftSizeZ = swimArea.z;

            Gizmos.DrawCube(new Vector3(leftX, leftY, leftZ), new Vector3(leftSizeX, leftSizeY, leftSizeZ));
        }

        //draw X axis - Right
        if (showRight) {
            float rightX = swimCenter.x + (swimArea.x / 2) + wallThickness / 2;
            float rightY = swimCenter.y;
            float rightZ = swimCenter.z;

            float rightSizeX = wallThickness;
            float rightSizeY = swimArea.y;
            float rightSizeZ = swimArea.z;

            Gizmos.DrawCube(new Vector3(rightX, rightY, rightZ), new Vector3(rightSizeX, rightSizeY, rightSizeZ));

        }

        //draw Z axis - Front
        if (showFront) {
            float frontX = swimCenter.x;
            float frontY = swimCenter.y;
            float frontZ = swimCenter.z - (swimArea.z / 2) - wallThickness / 2;

            float frontSizeX = swimArea.x + 2 * wallThickness;
            float frontSizeY = swimArea.y;
            float frontSizeZ = wallThickness;

            Gizmos.DrawCube(new Vector3(frontX, frontY, frontZ), new Vector3(frontSizeX, frontSizeY, frontSizeZ));
        }


        //draw Z axis - Back
        if (showBack) {
            float backX = swimCenter.x;
            float backY = swimCenter.y;
            float backZ = swimCenter.z + (swimArea.z / 2) + wallThickness / 2;

            float backSizeX = swimArea.x + 2 * wallThickness;
            float backSizeY = swimArea.y;
            float backSizeZ = wallThickness;

            Gizmos.DrawCube(new Vector3(backX, backY, backZ), new Vector3(backSizeX, backSizeY, backSizeZ));
        }
    }
    #endregion

    #region Utility Methods
    public void Query(Vector3 worldPos, float margin, out float distanceToBoundary, out Vector3 correctionDir, out bool isInside) {
        // Convert to box local space (relative to collider center)
        Vector3 local = transform.InverseTransformPoint(worldPos) - boxCollider.center;

        Vector3 half = boxCollider.size * 0.5f;

        // Check inside
        isInside =
            Mathf.Abs(local.x) <= half.x &&
            Mathf.Abs(local.y) <= half.y &&
            Mathf.Abs(local.z) <= half.z;

        // Clamp to inside point
        Vector3 clamped = new Vector3(
            Mathf.Clamp(local.x, -half.x, half.x),
            Mathf.Clamp(local.y, -half.y, half.y),
            Mathf.Clamp(local.z, -half.z, half.z)
        );

        Vector3 clampedWorld = transform.TransformPoint(clamped + boxCollider.center);

        if (!isInside) {
            // Outside: push toward closest point in volume
            Vector3 toInside = clampedWorld - worldPos;
            correctionDir = toInside.sqrMagnitude > 0.0001f ? toInside.normalized : Vector3.zero;
            distanceToBoundary = 0f;
            return;
        }

        // Inside: distance to nearest boundary plane
        float dx = half.x - Mathf.Abs(local.x);
        float dy = half.y - Mathf.Abs(local.y);
        float dz = half.z - Mathf.Abs(local.z);

        distanceToBoundary = Mathf.Min(dx, Mathf.Min(dy, dz));

        if (distanceToBoundary <= margin) {
            // Determine which face is closest and steer away from it (back toward center)
            Vector3 away = Vector3.zero;

            if (dx <= dy && dx <= dz) away = new Vector3(-Mathf.Sign(local.x), 0f, 0f);
            else if (dy <= dx && dy <= dz) away = new Vector3(0f, -Mathf.Sign(local.y), 0f);
            else away = new Vector3(0f, 0f, -Mathf.Sign(local.z));

            // Convert local direction to world
            correctionDir = transform.TransformDirection(away).normalized;
        }
        else {
            correctionDir = Vector3.zero;
        }
    }
    #endregion

}
