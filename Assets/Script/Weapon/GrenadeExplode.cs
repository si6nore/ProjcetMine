using UnityEngine;

public class GrenadeExplode : MonoBehaviour
{
    [Header("폭발 설정")]
    public float explosionRadius = 5f;
    public float explosionDamage = 80f;
    public float fuseTime = 3f; // 던진 후 폭발까지 시간

    [Header("이펙트")]
    public GameObject explosionVFX; // 폭발 파티클 (선택)

    [Header("사운드")]
    public AudioClip explosionSound;

    void Start()
    {
        Invoke("Explode", fuseTime);
    }

    void Explode()
    {
        // 폭발 사운드
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        // 폭발 이펙트
        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // 범위 내 오브젝트 데미지
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            HitBox hitBox = col.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnHit(explosionDamage);
                continue;
            }

            Health health = col.GetComponentInParent<Health>();
            if (health != null)
                health.TakeDamage(explosionDamage);
        }

        Destroy(gameObject);
    }

    // 에디터에서 폭발 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
