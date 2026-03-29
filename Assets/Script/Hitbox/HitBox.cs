using UnityEngine;

public class HitBox : MonoBehaviour
{
    [Header("데미지 설정")]
    public float damageMultiplier = 1.0f;
    public string bodyPartName;

    
    private Health _health;

    void Start()
    {
        // 부모에서 Health 찾기 (플레이어/적 공통)
        _health = GetComponentInParent<Health>();
    }

    public void OnHit(float baseDamage)
    {
        if (_health == null || _health.IsDead) return;

        float finalDamage = baseDamage * damageMultiplier;

        _health.TakeDamage(finalDamage);

        Debug.Log($"{transform.root.name}의 {bodyPartName} 피격! 데미지: {finalDamage}");
    }
}
