//using UnityEngine;
//using UnityEngine.UI;

//public class SpinningMinigameController : MonoBehaviour {
//    #region Variables
//    [Header("UI Elements")]
//    [SerializeField] Image timerBar;
//    [SerializeField] Image landingBar;
//    [SerializeField] Image slackBar;
//    [SerializeField] RectTransform landingArcRect;
//    [SerializeField] RectTransform cursorRect;

//    bool isGameOver;
//    float gameDuration; //in Seconds
//    float gameTimer; //in Seconds

//    float landingArcSize; //in Degrees
//    float landingArcAngle; //in Degrees
//    float spinSpeed; //in Degrees per Second
//    #endregion

//    #region Unity Methods    
//    private void Awake() {
//        isGameOver = true;
//    }
//    private void Update() {
//        if (isGameOver) return;
//        if (gameTimer >= gameDuration) {
//            EndGame(true);
//            return;
//        }

//        //update timer
//        gameTimer += Time.deltaTime;
//        timerBar.fillAmount = 1f - (gameTimer / gameDuration);

//        //update landing bar

//        //update slack bar

//        //update landing arc spin

//        //update cursor spin

//    }
//    #endregion

//    #region Utility Methods
//    void InitializeGame() {
//        isGameOver = false;
//        gameDuration = 5f;
//        landingArcSize = 60f;
//        landingArcAngle = Random.Range(0f, 360f);
//        spinSpeed = 5f;
//    }
//    void EndGame(bool gameLost) {
//        isGameOver = true;
//        if (gameLost) {
//            Debug.Log("Game Over: You Lost!");
//        }
//        else {
//            Debug.Log("Game Over: You Won!");
//        }
//    }
//    void ShowGame() {

//    }
//    void HideGame() {

//    }
//    #endregion
//}
