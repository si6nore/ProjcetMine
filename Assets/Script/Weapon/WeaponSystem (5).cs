using UnityEngine;
using TMPro;
using System.Collections;

public class WeaponSystem : MonoBehaviour
{
    [Header("탄약 설정")]
    public int magazineSize = 30;
    public int totalAmmo = 120;

    [Header("사격 설정")]
    public float fireRate = 2f;
    public float damage = 20f;

    [Header("재장전 설정")]
    public float reloadTime = 2.0f;

    [Header("총구 위치 (필수)")]
    public Transform muzzlePoint;

    [Header("총알 이펙트")]
    public float bulletLineDuration = 0.05f;
    public Material bulletLineMaterial;

    [Header("머즐 플래시 이펙트")]
    public GameObject muzzleFlashVFX;
    public float muzzleFlashDuration = 0.1f;

    [Header("사운드")]
    public AudioClip fireSound;
    public AudioClip reloadSound;

    private AudioSource _fireAudio;
    private AudioSource _reloadAudio;

    [Header("UI (선택)")]
    public TMP_Text ammoText;
    public TMP_Text reloadText;

    int _currentAmmo;
    bool _isReloading = false;
    float _nextFireTime = 0f;

    void Start()
    {
        _currentAmmo = magazineSize;
        UpdateUI();

        if (muzzlePoint == null)
            Debug.LogError($"{gameObject.name}: muzzlePoint가 연결되지 않았습니다!");

        _fireAudio = gameObject.AddComponent<AudioSource>();
        _fireAudio.loop = false;
        _fireAudio.playOnAwake = false;

        _reloadAudio = gameObject.AddComponent<AudioSource>();
        _reloadAudio.loop = false;
        _reloadAudio.playOnAwake = false;
    }

    // 플레이어용 - 카메라 방향을 받아서 머즐포인트에서 레이 발사
    public bool TryFire(Transform cameraTransform)
    {
        if (_isReloading) return false;
        if (_currentAmmo <= 0) { TryReload(); return false; }
        if (Time.time < _nextFireTime) return false;

        if (muzzlePoint == null) return false;

        // ✅ 레이 시작: 머즐포인트 위치, 방향: 카메라 forward
        Ray ray = new Ray(muzzlePoint.position, cameraTransform.forward);
        ExecuteFire(ray);
        return true;
    }

    // AI용 - 머즐포인트에서 목표 지점으로 발사
    public void TryFire(Transform firePoint, Vector3 targetPoint)
    {
        if (_isReloading) return;
        if (_currentAmmo <= 0) { TryReload(); return; }
        if (Time.time < _nextFireTime) return;

        if (muzzlePoint == null) return;

        Vector3 dir = (targetPoint - muzzlePoint.position).normalized;
        Ray ray = new Ray(muzzlePoint.position, dir);
        ExecuteFire(ray);
    }

    public void StopFireSound() { }

    void ExecuteFire(Ray ray)
    {
        _currentAmmo--;
        _nextFireTime = Time.time + (1f / fireRate);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Shoot");

        if (fireSound != null)
            _fireAudio.PlayOneShot(fireSound);

        // 머즐 플래시
        if (muzzleFlashVFX != null)
        {
            GameObject fx = Instantiate(muzzleFlashVFX, muzzlePoint.position, muzzlePoint.rotation);
            Destroy(fx, muzzleFlashDuration);
        }

        // 레이캐스트
        RaycastHit[] hits = Physics.RaycastAll(ray, 200f);

        bool hitSomething = false;
        RaycastHit closestHit = default;
        float closestDist = float.MaxValue;
        HitBox hitBox = null;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.root == transform.root) continue;
            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closestHit = hit;
                hitSomething = true;
                hitBox = hit.collider.GetComponent<HitBox>();
            }
        }

        // ✅ 시작점, 끝점 전부 머즐포인트 기준
        Vector3 lineEnd = hitSomething
            ? closestHit.point
            : muzzlePoint.position + ray.direction * 200f;

        StartCoroutine(DrawBulletLine(muzzlePoint.position, lineEnd));

        if (hitSomething)
        {
            if (hitBox != null)
                hitBox.OnHit(damage);
            else
            {
                Health h = closestHit.collider.GetComponentInParent<Health>();
                if (h != null) h.TakeDamage(damage);
            }
        }

        UpdateUI();
    }

    IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("BulletLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        if (bulletLineMaterial != null)
            lr.material = bulletLineMaterial;
        else
            lr.material = new Material(Shader.Find("Hidden/Internal-Colored"));

        lr.startColor = Color.yellow;
        lr.endColor = new Color(1f, 0.5f, 0f);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.005f;
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(bulletLineDuration);
        Destroy(lineObj);
    }

    public void TryReload()
    {
        if (_isReloading) return;
        if (totalAmmo <= 0 || _currentAmmo == magazineSize) return;
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        _isReloading = true;

        if (reloadSound != null)
            _reloadAudio.PlayOneShot(reloadSound);

        if (reloadText != null)
            reloadText.text = "RELOADING...";

        yield return new WaitForSeconds(reloadTime);

        int fill = Mathf.Min(magazineSize - _currentAmmo, totalAmmo);
        _currentAmmo += fill;
        totalAmmo -= fill;

        _isReloading = false;

        if (reloadText != null)
            reloadText.text = "";

        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoText != null)
            ammoText.text = $"{_currentAmmo} / {totalAmmo}";
    }

    public int CurrentAmmo => _currentAmmo;
    public int TotalAmmo => totalAmmo;
    public bool IsReloading => _isReloading;
}
