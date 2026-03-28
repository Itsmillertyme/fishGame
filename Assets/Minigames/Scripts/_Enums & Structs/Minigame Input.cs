using UnityEngine;

public struct MinigameInput {
    public Vector2 pointerScreenPos;
    public Vector2 dragDelta;

    // Screen region (based on pointer x)
    public float pointerX01;
    public bool isLeftSide;
    public bool isRightSide;

    public bool primaryDown;
    public bool primaryHeld;
    public bool primaryUp;

    public bool cancelDown;
}