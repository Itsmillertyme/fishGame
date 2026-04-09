using UnityEngine;
using UnityEngine.InputSystem;

public class RunDebugOverlay : MonoBehaviour {
    #region Variables

    [Header("References")]
    [SerializeField] private GameSessionController gameSessionController;
    [SerializeField] private RunBootstrapTester runBootstrapTester;

    [Header("Display")]
    [SerializeField] private bool showOverlay = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;

    [Header("Layout")]
    [SerializeField] private float panelWidth = 500f;
    [SerializeField] private float panelHeight = 700f;
    [SerializeField] private Vector2 scrollPosition;

    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;

    #endregion

    #region Unity Methods

    private void Awake() {
        if (gameSessionController == null) {
            gameSessionController = GameSessionController.Instance;
        }
    }

    private void OnGUI() {
        if (!showOverlay) {
            return;
        }

        if (headerStyle == null || sectionStyle == null || bodyStyle == null || buttonStyle == null || boxStyle == null) {
            BuildStyles();
        }

        if (gameSessionController == null) {
            gameSessionController = GameSessionController.Instance;
        }

        float panelX = 10f;
        float panelY = 10f;

        GUILayout.BeginArea(new Rect(panelX, panelY, panelWidth, panelHeight), boxStyle);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(panelWidth - 10f), GUILayout.Height(panelHeight - 10f));

        DrawHeader();
        DrawControls();
        DrawRunState();

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void Update() {
        if (WasTogglePressedThisFrame()) {
            showOverlay = !showOverlay;
        }

        if (!showOverlay) {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //private void LateUpdate() {
    //    if (showOverlay) {
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //    }
    //}
    #endregion

    #region Utility Methods

    void BuildStyles() {
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.wordWrap = true;

        sectionStyle = new GUIStyle(GUI.skin.label);
        sectionStyle.fontSize = 14;
        sectionStyle.fontStyle = FontStyle.Bold;
        sectionStyle.wordWrap = true;

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 12;
        bodyStyle.wordWrap = true;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;
        buttonStyle.wordWrap = true;
        buttonStyle.fixedHeight = 28f;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
    }

    void DrawHeader() {
        GUILayout.Label("Run Debug Overlay", headerStyle);
        GUILayout.Label("Toggle: " + toggleKey, bodyStyle);
        GUILayout.Space(8f);
    }

    void DrawControls() {
        GUILayout.Label("Controls", sectionStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Test Run", buttonStyle, GUILayout.Width(150f))) {
            StartTestRun();
        }

        if (GUILayout.Button("Unlock All", buttonStyle, GUILayout.Width(100f))) {
            UnlockAllWaterBodies();
        }

        if (GUILayout.Button("Complete First", buttonStyle, GUILayout.Width(120f))) {
            CompleteFirstChallengeInEachWaterBody();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Mark Current Visited", buttonStyle, GUILayout.Width(150f))) {
            MarkCurrentVisited();
        }

        if (GUILayout.Button("Refresh Counts", buttonStyle, GUILayout.Width(120f))) {
            RefreshCompletedCounts();
        }

        if (GUILayout.Button("Sim Catch", buttonStyle, GUILayout.Width(100f))) {
            SimulateCatch();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
    }

    void DrawRunState() {
        GUILayout.Label("Run State", sectionStyle);

        if (gameSessionController == null) {
            GUILayout.Label("GameSessionController reference is missing.", bodyStyle);
            return;
        }

        if (gameSessionController.CurrentRun == null) {
            GUILayout.Label("No active run.", bodyStyle);
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        GUILayout.Label("Run Id: " + currentRun.RunId, bodyStyle);

        if (currentRun.CurrentWaterBody != null && currentRun.CurrentWaterBody.Definition != null) {
            GUILayout.Label("Current Water Body: " + currentRun.CurrentWaterBody.Definition.DisplayName, bodyStyle);
        }
        else {
            GUILayout.Label("Current Water Body: None", bodyStyle);
        }

        GUILayout.Label("Water Body Count: " + currentRun.WaterBodies.Count, bodyStyle);
        GUILayout.Space(10f);

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            DrawWaterBodyState(currentRun.WaterBodies[i], i);
            GUILayout.Space(10f);
        }
    }

    void DrawWaterBodyState(WaterBodyRuntimeState waterBodyState, int index) {
        if (waterBodyState == null || waterBodyState.Definition == null) {
            GUILayout.Label("Water Body [" + index + "] is null.", bodyStyle);
            return;
        }

        GUILayout.BeginVertical(boxStyle);

        string waterTitle = "Water Body [" + index + "]: " + waterBodyState.Definition.DisplayName;
        GUILayout.Label(waterTitle, sectionStyle);

        GUILayout.Label("Id: " + waterBodyState.Definition.Id, bodyStyle);
        GUILayout.Label("Scene: " + waterBodyState.Definition.SceneName, bodyStyle);
        GUILayout.Label("Tier: " + waterBodyState.Definition.ProgressionTier, bodyStyle);
        GUILayout.Label("Unlocked: " + waterBodyState.IsUnlocked, bodyStyle);
        GUILayout.Label("Visited: " + waterBodyState.HasBeenVisited, bodyStyle);
        GUILayout.Label("Challenges: " + waterBodyState.ActiveChallenges.Count, bodyStyle);
        GUILayout.Label("Completed: " + waterBodyState.CompletedChallengeCount, bodyStyle);

        GUILayout.Space(6f);

        if (GUILayout.Button("Set Current: " + waterBodyState.Definition.DisplayName, buttonStyle)) {
            gameSessionController.SetCurrentWaterBody(waterBodyState.Definition.Id);
        }

        GUILayout.Space(6f);
        GUILayout.Label("Active Challenges", sectionStyle);

        for (int i = 0; i < waterBodyState.ActiveChallenges.Count; i++) {
            DrawChallengeState(waterBodyState, waterBodyState.ActiveChallenges[i], i);
        }

        GUILayout.EndVertical();
    }

    void DrawChallengeState(WaterBodyRuntimeState waterBodyState, ChallengeInstance challengeInstance, int index) {
        if (challengeInstance == null || challengeInstance.Definition == null) {
            GUILayout.Label("Challenge [" + index + "] is null.", bodyStyle);
            return;
        }

        GUILayout.BeginVertical(boxStyle);

        GUILayout.Label("Challenge [" + index + "]: " + challengeInstance.Definition.DisplayName, bodyStyle);
        GUILayout.Label("Id: " + challengeInstance.Definition.Id, bodyStyle);
        GUILayout.Label("Type: " + challengeInstance.Definition.ObjectiveType, bodyStyle);
        GUILayout.Label("Target: " + challengeInstance.Definition.TargetCount, bodyStyle);
        GUILayout.Label("Progress: " + challengeInstance.CurrentProgressValue, bodyStyle);
        GUILayout.Label("Completed: " + challengeInstance.IsCompleted, bodyStyle);
        GUILayout.Label("Claimed: " + challengeInstance.IsClaimed, bodyStyle);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("+1 Progress", buttonStyle, GUILayout.Width(100f))) {
            gameSessionController.AddProgressToChallenge(
                waterBodyState.Definition.Id,
                challengeInstance.Definition.Id,
                1
            );
        }

        if (GUILayout.Button("Complete", buttonStyle, GUILayout.Width(100f))) {
            gameSessionController.SetChallengeProgress(
                waterBodyState.Definition.Id,
                challengeInstance.Definition.Id,
                challengeInstance.Definition.TargetCount
            );
        }

        if (GUILayout.Button("Claim", buttonStyle, GUILayout.Width(100f))) {
            gameSessionController.ClaimChallenge(
                waterBodyState.Definition.Id,
                challengeInstance.Definition.Id
            );
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void StartTestRun() {
        if (runBootstrapTester != null) {
            runBootstrapTester.StartTestRun();
            return;
        }

        if (gameSessionController == null) {
            Debug.LogError("RunDebugOverlay: No GameSessionController reference found.");
            return;
        }

        gameSessionController.StartNewRun();
    }

    void UnlockAllWaterBodies() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null) {
                continue;
            }

            waterBodyState.SetUnlocked(true);
        }
    }

    void CompleteFirstChallengeInEachWaterBody() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            if (waterBodyState.ActiveChallenges == null || waterBodyState.ActiveChallenges.Count == 0) {
                continue;
            }

            ChallengeInstance firstChallenge = waterBodyState.ActiveChallenges[0];

            if (firstChallenge == null || firstChallenge.Definition == null) {
                continue;
            }

            gameSessionController.SetChallengeProgress(
                waterBodyState.Definition.Id,
                firstChallenge.Definition.Id,
                firstChallenge.Definition.TargetCount
            );
        }
    }

    void MarkCurrentVisited() {
        if (gameSessionController == null) {
            return;
        }

        WaterBodyRuntimeState currentWaterBody = gameSessionController.GetCurrentWaterBody();

        if (currentWaterBody == null) {
            return;
        }

        currentWaterBody.MarkVisited();
    }

    void RefreshCompletedCounts() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null) {
                continue;
            }

            waterBodyState.RefreshCompletedChallengeCount();
        }
    }

    void SimulateCatch() {
        if (runBootstrapTester == null) {
            Debug.LogWarning("RunDebugOverlay: RunBootstrapTester reference is missing.");
            return;
        }

        runBootstrapTester.SimulateCatch();
    }

    bool WasTogglePressedThisFrame() {
        if (Keyboard.current == null) {
            return false;
        }

        switch (toggleKey) {
            case KeyCode.F1:
                return Keyboard.current.f1Key.wasPressedThisFrame;
            case KeyCode.F2:
                return Keyboard.current.f2Key.wasPressedThisFrame;
            case KeyCode.F3:
                return Keyboard.current.f3Key.wasPressedThisFrame;
            case KeyCode.F4:
                return Keyboard.current.f4Key.wasPressedThisFrame;
            case KeyCode.F5:
                return Keyboard.current.f5Key.wasPressedThisFrame;
            case KeyCode.F6:
                return Keyboard.current.f6Key.wasPressedThisFrame;
            case KeyCode.F7:
                return Keyboard.current.f7Key.wasPressedThisFrame;
            case KeyCode.F8:
                return Keyboard.current.f8Key.wasPressedThisFrame;
            case KeyCode.F9:
                return Keyboard.current.f9Key.wasPressedThisFrame;
            case KeyCode.F10:
                return Keyboard.current.f10Key.wasPressedThisFrame;
            case KeyCode.F11:
                return Keyboard.current.f11Key.wasPressedThisFrame;
            case KeyCode.F12:
                return Keyboard.current.f12Key.wasPressedThisFrame;
        }

        return false;
    }
    #endregion
}