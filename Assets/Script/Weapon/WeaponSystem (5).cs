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

    [Header("총구 위치 (선택)")]
    public Transform muzzlePoint;

    [Header("총알 이펙트")]
    public float bulletLineDuration = 0.05f;

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

        // 발사음 - PlayOneShot 방식 (루프 없음)
        _fireAudio = gameObject.AddComponent<AudioSource>();
        _fireAudio.loop = false;
        _fireAudio.playOnAwake = false;

        // 장전음
        _reloadAudio = gameObject.AddComponent<AudioSource>();
        _reloadAudio.loop = false;
        _reloadAudio.playOnAwake = false;
    }

    public bool TryFire(Transform firePoint)
    {
        if (_isReloading) return false;
        if (_currentAmmo <= 0) { TryReload(); return false; }
        if (Time.time < _nextFireTime) return false;

        ExecuteFire(new Ray(firePoint.position, firePoint.forward));
        return true;
    }

    public void TryFire(Transform firePoint, Vector3 targetPoint)
    {
        if (_isReloading) return;
        if (_currentAmmo <= 0) { TryReload(); return; }
        if (Time.time < _nextFireTime) return;

        Vector3 dir = (targetPoint - firePoint.position).normalized;
        ExecuteFire(new Ray(firePoint.position, dir));
    }

    // FPSController에서 마우스 뗄 때 호출 (루프 방식 제거로 실질적으론 불필요하지만 호환성 유지)
    public void StopFireSound()
    {
        // PlayOneShot 방식이라 자연스럽게 끊김, 별도 처리 불필요
    }

    void ExecuteFire(Ray ray)
    {
        _currentAmmo--;
        _nextFireTime = Time.time + (1f / fireRate);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger("Shoot");

        // 발사할 때마다 한 번만 재생
        if (fireSound != null)
            _fireAudio.PlayOneShot(fireSound);

        Vector3 lineStart = muzzlePoint != null ? muzzlePoint.position : ray.origin;

        RaycastHit[] hits = Physics.RaycastAll(ray, 200f);

        HitBox hitBox = null;
        RaycastHit closestHit = default;
        float closestDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            HitBox hb = hit.collider.GetComponent<HitBox>();
            if (hb != null && hit.distance < closestDist)
            {
                hitBox = hb;
                closestDist = hit.distance;
                closestHit = hit;
            }
        }

        if (hitBox != null)
        {
            hitBox.OnHit(damage);
            StartCoroutine(DrawBulletLine(lineStart, closestHit.point));
        }
        else
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            if (hits.Length > 0)
            {
                Health h = hits[0].collider.GetComponentInParent<Health>();
                if (h != null) h.TakeDamage(damage);
                StartCoroutine(DrawBulletLine(lineStart, hits[0].point));
            }
            else
            {
                StartCoroutine(DrawBulletLine(lineStart, ray.origin + ray.direction * 200f));
            }
        }

        UpdateUI();
    }

    IEnumerator DrawBulletLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("BulletLine");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = new Color(1f, 0.5f, 0f);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.005f;
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
