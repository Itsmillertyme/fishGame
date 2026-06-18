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
    void Initialize(MinigameInstructionPopup popup, PlayerDataRuntime runtime);
    void End(MinigameEndReason reason);
    #endregion
}


