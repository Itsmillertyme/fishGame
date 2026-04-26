using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class CharacterControllerThirdPerson : MonoBehaviour {
    #region Variables
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float moveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float sprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float acceleration = 10.0f;

    [Tooltip("Is movement currently locked in place")]
    public bool MovementLocked { get; set; }

    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float jumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float jumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float fallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool grounded = true;

    [Tooltip("Useful for rough ground")]
    public float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;

    private float speed;
    private float animationBlend;
    private float targetRotation;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    private int animIdSpeed;
    private int animIdGrounded;
    private int animIdJump;
    private int animIdIsFishing;
    private int animIdFreeFall;
    private int animIdMotionSpeed;
    private int animIdIsShowingFish;

    private PlayerInput playerInput;
    private Animator animator;
    private CharacterController controller;
    private CharacterControllerInputs inputs;
    private GameObject mainCamera;

    private const float threshold = 0.01f;
    private bool hasAnimator;
    #endregion

    #region Unity Methods
    private void Awake() {
        if (mainCamera == null) {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start() {
        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        inputs = GetComponent<CharacterControllerInputs>();
        playerInput = GetComponent<PlayerInput>();

        AssignAnimationIds();

        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    private void Update() {
        hasAnimator = TryGetComponent(out animator);

        HandleFishing();
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void OnDrawGizmosSelected() {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = grounded ? transparentGreen : transparentRed;

        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
            groundedRadius
        );
    }

    private void OnFootstep(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            if (footstepAudioClips != null && footstepAudioClips.Length > 0) {
                int index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(
                    footstepAudioClips[index],
                    transform.TransformPoint(controller.center),
                    footstepAudioVolume
                );
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            if (landingAudioClip != null) {
                AudioSource.PlayClipAtPoint(
                    landingAudioClip,
                    transform.TransformPoint(controller.center),
                    footstepAudioVolume
                );
            }
        }
    }
    #endregion

    #region Utility Methods
    private void AssignAnimationIds() {
        animIdSpeed = Animator.StringToHash("Speed");
        animIdGrounded = Animator.StringToHash("Grounded");
        animIdJump = Animator.StringToHash("Jump");
        animIdIsFishing = Animator.StringToHash("IsFishing");
        animIdFreeFall = Animator.StringToHash("FreeFall");
        animIdMotionSpeed = Animator.StringToHash("MotionSpeed");
        animIdIsShowingFish = Animator.StringToHash("IsShowingFish");
    }

    private void HandleFishing() {
        if (inputs == null) return;

        bool isFishing = inputs.cast;

        if (hasAnimator) {
            animator.SetBool(animIdIsFishing, isFishing);
        }
    }

    private void GroundedCheck() {
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - groundedOffset,
            transform.position.z
        );

        grounded = Physics.CheckSphere(
            spherePosition,
            groundedRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        if (hasAnimator) {
            animator.SetBool(animIdGrounded, grounded);
        }
    }

    private void Move() {
        if (inputs == null) return;
        if (mainCamera == null) return;

        if (MovementLocked) {
            if (hasAnimator) {
                animator.SetFloat(animIdSpeed, 0f);
                animator.SetFloat(animIdMotionSpeed, 0f);
            }
            return;
        }

        float targetSpeed = inputs.sprint ? sprintSpeed : moveSpeed;
        if (inputs.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = inputs.analogMovement ? inputs.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * acceleration);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else {
            speed = targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * acceleration);
        if (animationBlend < 0.01f) animationBlend = 0f;

        Vector3 inputDirection = new Vector3(inputs.move.x, 0.0f, inputs.move.y).normalized;

        if (inputs.move != Vector2.zero) {

            inputs.cast = false;

            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        controller.Move(
            targetDirection.normalized * (speed * Time.deltaTime) +
            new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime
        );

        if (hasAnimator) {
            animator.SetFloat(animIdSpeed, animationBlend);
            animator.SetFloat(animIdMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity() {
        if (inputs == null) return;

        if (grounded) {
            fallTimeoutDelta = fallTimeout;

            if (hasAnimator) {
                animator.SetBool(animIdJump, false);
                animator.SetBool(animIdFreeFall, false);
            }

            if (verticalVelocity < 0.0f) {
                verticalVelocity = -2f;
            }

            if (inputs.jump && jumpTimeoutDelta <= 0.0f) {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (hasAnimator) {
                    inputs.cast = false;
                    animator.SetBool(animIdJump, true);
                }
            }

            if (jumpTimeoutDelta >= 0.0f) {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f) {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else {
                if (hasAnimator) {
                    animator.SetBool(animIdFreeFall, true);
                }
            }

            inputs.jump = false;
        }

        if (verticalVelocity < terminalVelocity) {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    #endregion
}