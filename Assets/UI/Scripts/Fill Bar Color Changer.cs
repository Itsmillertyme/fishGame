using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBarColorChanger : MonoBehaviour {
    #region Variables
    [Tooltip("Colors to interpolate between based on fill amount. Empty color first")]
    [SerializeField] List<Color> colors;

    Image filledImage;
    #endregion

    #region Unity Methods    
    private void Awake() {
        filledImage = GetComponent<Image>();
        ApplyColor();
    }

    private void Update() {
        ApplyColor();
    }
    #endregion

    #region Utility Methods
    public void ApplyColor() {
        //Sanity
        if (colors == null || colors.Count == 0) return;
        if (filledImage.type != Image.Type.Filled) return;

        //Single Color
        if (colors.Count == 1) {
            filledImage.color = colors[0];
            return;
        }

        //Helpers
        int segments = colors.Count - 1;
        float scaledFilledAmount = filledImage.fillAmount * segments;

        int leftColorIndex = Mathf.FloorToInt(scaledFilledAmount);
        leftColorIndex = Mathf.Clamp(leftColorIndex, 0, segments - 1);

        int rightColorIndex = leftColorIndex + 1;

        float currentSegmentFill = scaledFilledAmount - leftColorIndex;

        filledImage.color = Color.Lerp(colors[leftColorIndex], colors[rightColorIndex], currentSegmentFill);
    }
    #endregion
}
