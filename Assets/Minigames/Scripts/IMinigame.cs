using UnityEngine;

public interface IMinigame {

    #region Variables
    MinigameType Type { get; }
    bool IsComplete { get; }
    MinigameResult Result { get; }
    #endregion

    #region Methods
    void Begin(in MinigameContext context);
    void HandleInput(in MinigameInput input);
    void Tick(float deltaTime);
    void End(MinigameEndReason reason);
    #endregion
}

#region Structs
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

public struct MinigameResult {
    public MinigameEndReason reason;
    public float land01;
    public float slack01;
}

public struct MinigameContext {
    public MinigameType type;
    public MinigameDifficulty difficulty;
    public bool extendedFight;

    public SpinningRodSettings spinning;
    public CastingRodSettings casting;
}
#endregion

#region Enums
public enum MinigameEndReason {
    None,
    Success,
    TimeOut,
    SlackMaxed,
    Backlash,     // reserved for casting rod game
    Cancelled
}

public enum MinigameType {
    SpinningRod,
    CastingRod
}

public enum MinigameDifficulty {
    Easy,
    Medium,
    Hard
}
#endregion
