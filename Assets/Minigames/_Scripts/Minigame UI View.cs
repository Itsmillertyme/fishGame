using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameUIView : MonoBehaviour {
    #region Variables
    [Header("Panels")]
    [SerializeField] GameObject spinningPanel;
    [SerializeField] GameObject castingPanel;

    [Header("Spinning UI")]
    [SerializeField] RectTransform spinningTargetArcPivot;
    [SerializeField] Image spinningTargetArcGraphic;

    [SerializeField] RectTransform spinningCursorPivot;
    [SerializeField] RectTransform spinningCursorGraphic;

    [SerializeField] Image spinningLandBar;
    [SerializeField] Image spinningSlackBar;
    [SerializeField] Image spinningTimerBar;

    [SerializeField] TextMeshProUGUI spinningTimerText;

    [Header("Casting UI")]

    [SerializeField] Image castingRodImage;
    [SerializeField] Sprite castingRodStraightSprite;
    [SerializeField] Sprite castingRodBentSprite;

    [SerializeField] TextMeshProUGUI castingSurgeText;

    [SerializeField] RectTransform castingRodPulseTarget;
    [SerializeField] float castingSurgePulseScale = 1.08f;
    [SerializeField] float castingSurgePulseSpeed = 10f;
    [SerializeField] float castingSurgeEndFlashWindow01 = 0.35f;

    [SerializeField] Image castingLandBar;
    [SerializeField] Image castingHeatBar;
    [SerializeField] Image castingTimerBar;

    [SerializeField] TextMeshProUGUI castingTimerText;

    #endregion

    #region Unity Methods
    private void Awake() {
        ShowSpinning(false);
        ShowCasting(false);
    }
    #endregion

    #region Utility Methods
    public void ShowSpinning(bool show) {
        if (spinningPanel != null) spinningPanel.SetActive(show);
    }

    public void ShowCasting(bool show) {
        if (castingPanel != null) castingPanel.SetActive(show);
    }

    public void RenderSpinning(SpinningRodMinigame.State state) {
        //Landing Zone Arc
        if (spinningTargetArcGraphic != null) {
            spinningTargetArcGraphic.fillAmount = state.arcFill01;

            //center filled arc
            float halfArcDeg = (state.arcFill01 * 360f) * 0.5f;
            float startDeg = state.targetCenterDeg - halfArcDeg;

            spinningTargetArcPivot.localEulerAngles = new Vector3(0f, 0f, -startDeg);
        }

        // Cursor
        if (spinningCursorPivot != null) {
            //rotate
            spinningCursorPivot.localEulerAngles = new Vector3(0f, 0f, -state.cursorDeg + 45f);
        }
        UpdateArcColor(state.inZone);

        // Meters
        if (spinningLandBar != null) spinningLandBar.fillAmount = Mathf.Clamp01(state.land01);
        if (spinningSlackBar != null) spinningSlackBar.fillAmount = Mathf.Clamp01(state.slack01);
        if (spinningTimerBar != null) spinningTimerBar.fillAmount = Mathf.Clamp01(state.timeRemaining01);

        if (spinningTimerText != null) {
            spinningTimerText.text = state.timeRemainingSeconds.ToString("0.0");
        }
    }

    public void RenderCasting(CastingRodMinigame.State state) {
        // Meters
        if (castingLandBar != null) castingLandBar.fillAmount = Mathf.Clamp01(state.land01);
        if (castingHeatBar != null) castingHeatBar.fillAmount = Mathf.Clamp01(state.heat01);
        if (castingTimerBar != null) castingTimerBar.fillAmount = Mathf.Clamp01(state.timeRemaining01);

        if (castingTimerText != null) {
            castingTimerText.text = state.timeRemainingSeconds.ToString("0.00");
        }

        // Surge indicator text
        if (castingSurgeText != null) {
            castingSurgeText.gameObject.SetActive(state.isSurging);
        }

        // Rod sprite swap (straight vs bent)
        if (castingRodImage != null) {
            if (state.isSurging) {
                if (castingRodBentSprite != null) castingRodImage.sprite = castingRodBentSprite;
            }
            else {
                if (castingRodStraightSprite != null) castingRodImage.sprite = castingRodStraightSprite;
            }
        }

        // Surge pulse + end flash (optional)
        RectTransform pulseTarget = castingRodPulseTarget;
        if (pulseTarget == null && castingRodImage != null) pulseTarget = castingRodImage.rectTransform;

        if (pulseTarget != null) {
            if (state.isSurging) {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.Max(0.01f, castingSurgePulseSpeed));
                float scale = Mathf.Lerp(1f, Mathf.Max(1f, castingSurgePulseScale), pulse);
                pulseTarget.localScale = new Vector3(scale, scale, 1f);

                // Flash toward end of surge (white -> red)
                if (castingRodImage != null) {
                    Color baseColor = Color.white;
                    Color dangerColor = new Color(1f, 0.25f, 0.25f);

                    float endT = 0f;

                    float window = Mathf.Clamp01(castingSurgeEndFlashWindow01);
                    float remaining = Mathf.Clamp01(state.surgeRemaining01);
                    float progressToEnd = 1f - remaining;

                    float startFlashAt = 1f - window;
                    endT = progressToEnd <= startFlashAt ? 0f : Mathf.InverseLerp(startFlashAt, 1f, progressToEnd);
                    castingRodImage.color = Color.Lerp(baseColor, dangerColor, endT);
                }
            }
            else {
                pulseTarget.localScale = Vector3.one;
                if (castingRodImage != null) castingRodImage.color = Color.white;
            }
        }
    }

    public void UpdateArcColor(bool isInZone) {
        if (isInZone) {
            spinningTargetArcGraphic.color = new Color(51f / 255f, 145f / 255f, 250f / 255f);
        }
        else {
            spinningTargetArcGraphic.color = new Color(255f / 255f, 115f / 255f, 35f / 255f);
        }
    }
    #endregion
}
