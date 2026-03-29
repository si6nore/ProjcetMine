using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("마우스 설정")]
    public float sensitivity = 2f;
    public float minPitch = -10f;
    public float maxPitch = 20f;

    [Header("레퍼런스")]
    public Transform cameraHolder;
    public Transform[] armBones;
    public Animator animator;

    [Header("반동 설정")]
    public CameraShake cameraShake;
    public float recoilDuration = 0.05f;
    public float recoilMagnitude = 0.05f;

    // 내부 상태
    private CharacterController _cc;
    private WeaponSystem _weapon;
    private float _velocityY = 0f;
    private float _pitch = 0f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _weapon = GetComponent<WeaponSystem>();

        if (cameraHolder == null) Debug.LogError("FPSController: CameraHolder가 연결되지 않았습니다!");
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleGravityJump();
        HandleAnimation();
        HandleCursor();
        HandleWeapon();
    }

    void HandleWeapon()
    {
        if (_weapon == null) return;

        if (Input.GetMouseButton(0))
        {
            bool fired = _weapon.TryFire(cameraHolder);

            // ✅ 발사 성공했을 때만 반동 카메라 흔들림
            if (fired && cameraShake != null)
                StartCoroutine(cameraShake.Shake(recoilDuration, recoilMagnitude));
        }

        if (Input.GetKeyDown(KeyCode.R))
            _weapon.TryReload();
    }

    void LateUpdate()
    {
        if (armBones == null || armBones.Length < 2) return;

        if (armBones[0] != null)
            armBones[0].localRotation *= Quaternion.Euler(_pitch, 0f, 0f);

        if (armBones[1] != null)
            armBones[1].localRotation *= Quaternion.Euler(-_pitch, 0f, 0f);
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        transform.Rotate(Vector3.up, mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;

        if (move.magnitude > 1f) move.Normalize();

        _cc.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleGravityJump()
    {
        if (_cc.isGrounded)
        {
            _velocityY = -2f;

            if (Input.GetButtonDown("Jump"))
                _velocityY = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }
        else
        {
            _velocityY += gravity * Time.deltaTime;
        }

        _cc.Move(new Vector3(0f, _velocityY * Time.deltaTime, 0f));
    }

    void HandleAnimation()
    {
        if (animator == null) return;

        bool isMoving =
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        animator.SetBool("IsRunning", isMoving);
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            LockCursor(true);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    public void TriggerShoot()
    {
        if (animator != null)
            animator.SetTrigger("Shoot");
    }
}
