using UnityEngine;

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

    private void LateUpdate() {
        if (showOverlay) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    #endregion

    #region Utility Methods

    private void BuildStyles() {
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

    private void DrawHeader() {
        GUILayout.Label("Run Debug Overlay", headerStyle);
        GUILayout.Label("Toggle: " + toggleKey, bodyStyle);
        GUILayout.Space(8f);
    }

    private void DrawControls() {
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

    private void DrawRunState() {
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

    private void DrawWaterBodyState(WaterBodyRuntimeState waterBodyState, int index) {
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

    private void DrawChallengeState(WaterBodyRuntimeState waterBodyState, ChallengeInstance challengeInstance, int index) {
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
            challengeInstance.AddProgress(1);
            waterBodyState.RefreshCompletedChallengeCount();
        }

        if (GUILayout.Button("Complete", buttonStyle, GUILayout.Width(100f))) {
            challengeInstance.SetProgress(challengeInstance.Definition.TargetCount);
            waterBodyState.RefreshCompletedChallengeCount();
        }

        if (GUILayout.Button("Claim", buttonStyle, GUILayout.Width(100f))) {
            challengeInstance.MarkClaimed();
            waterBodyState.RefreshCompletedChallengeCount();
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void StartTestRun() {
        if (gameSessionController == null) {
            Debug.LogError("RunDebugOverlay: No GameSessionController reference found.");
            return;
        }

        gameSessionController.StartNewRun();
    }

    private void UnlockAllWaterBodies() {
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

    private void CompleteFirstChallengeInEachWaterBody() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null || waterBodyState.ActiveChallenges == null || waterBodyState.ActiveChallenges.Count == 0) {
                continue;
            }

            ChallengeInstance firstChallenge = waterBodyState.ActiveChallenges[0];

            if (firstChallenge == null || firstChallenge.Definition == null) {
                continue;
            }

            firstChallenge.SetProgress(firstChallenge.Definition.TargetCount);
            waterBodyState.RefreshCompletedChallengeCount();
        }
    }

    private void MarkCurrentVisited() {
        if (gameSessionController == null) {
            return;
        }

        WaterBodyRuntimeState currentWaterBody = gameSessionController.GetCurrentWaterBody();

        if (currentWaterBody == null) {
            return;
        }

        currentWaterBody.MarkVisited();
    }

    private void RefreshCompletedCounts() {
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

    private void SimulateCatch() {
        if (runBootstrapTester == null) {
            Debug.LogWarning("RunDebugOverlay: RunBootstrapTester reference is missing.");
            return;
        }

        runBootstrapTester.SimulateCatch();
    }
    #endregion
}