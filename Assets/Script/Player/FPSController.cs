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

    [Header("UI 연결")]
    public HelpMenuUI helpMenu;
    public InventoryUI inventoryUI;

    [Header("걷기 사운드")]
    public AudioClip walkSound;
    public float walkSoundInterval = 0.4f;

    private AudioSource _walkAudio;
    private float _walkSoundTimer = 0f;

    private CharacterController _cc;
    private WeaponSystem _weapon;
    private float _velocityY = 0f;
    private float _pitch = 0f;

    bool IsAnyUIOpen =>
        (helpMenu != null && helpMenu.IsOpen) ||
        (inventoryUI != null && inventoryUI.IsOpen);

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _weapon = GetComponent<WeaponSystem>();

        if (cameraHolder == null)
            Debug.LogError("FPSController: CameraHolder가 연결되지 않았습니다!");
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _walkAudio = gameObject.AddComponent<AudioSource>();
        _walkAudio.clip = walkSound;
        _walkAudio.loop = false;
        _walkAudio.playOnAwake = false;
    }

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        HandleCursor();
        HandleLook();
        HandleMove();
        HandleGravityJump();
        HandleAnimation();
        HandleWeapon();
        HandleWalkSound();
    }

    void HandleWeapon()
    {
        if (_weapon == null) return;
        if (IsAnyUIOpen) return;

        if (Input.GetMouseButton(0))
        {
            // ✅ cameraHolder를 넘김 → WeaponSystem이 카메라 방향으로 머즐에서 레이 발사
            bool fired = _weapon.TryFire(cameraHolder);
            if (fired && cameraShake != null)
                StartCoroutine(cameraShake.Shake(recoilDuration, recoilMagnitude));
        }

        if (Input.GetMouseButtonUp(0))
            _weapon.StopFireSound();

        if (Input.GetKeyDown(KeyCode.R))
            _weapon.TryReload();
    }

    void HandleWalkSound()
    {
        if (walkSound == null) return;
        if (IsAnyUIOpen || !_cc.isGrounded)
        {
            _walkAudio.Stop();
            _walkSoundTimer = 0f;
            return;
        }

        bool isMoving =
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;

        if (isMoving)
        {
            _walkSoundTimer -= Time.deltaTime;
            if (_walkSoundTimer <= 0f)
            {
                _walkAudio.clip = walkSound;
                _walkAudio.Play();
                _walkSoundTimer = walkSoundInterval;
            }
        }
        else
        {
            _walkAudio.Stop();
            _walkSoundTimer = 0f;
        }
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (helpMenu != null)
                helpMenu.Toggle();
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            if (!IsAnyUIOpen)
                LockCursor(true);
        }
    }

    void HandleLook()
    {
        if (IsAnyUIOpen) return;
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
        if (IsAnyUIOpen) return;

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

            if (!IsAnyUIOpen && Input.GetButtonDown("Jump"))
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

    void LateUpdate()
    {
        if (armBones == null || armBones.Length < 2) return;

        if (armBones[0] != null)
            armBones[0].localRotation *= Quaternion.Euler(_pitch, 0f, 0f);

        if (armBones[1] != null)
            armBones[1].localRotation *= Quaternion.Euler(-_pitch, 0f, 0f);
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
