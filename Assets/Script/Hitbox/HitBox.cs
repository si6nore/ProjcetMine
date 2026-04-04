using UnityEngine;

public class HitBox : MonoBehaviour
{
    [Header("데미지 설정")]
    public float damageMultiplier = 1.0f;
    public string bodyPartName;

    [Header("출혈 설정")]
    public bool canCauseBleeding = true;  // 머리는 Inspector에서 false로 설정
    [Range(0f, 1f)]
    public float bleedChance = 0.3f;      // 30% 확률 (Inspector에서 부위별 조정 가능)

    private Health _health;

    void Start()
    {
        _health = GetComponentInParent<Health>();
    }

    public void OnHit(float baseDamage)
    {
        if (_health == null || _health.IsDead) return;

        float finalDamage = baseDamage * damageMultiplier;
        _health.TakeDamage(finalDamage);

        // 출혈 판정 (머리 제외, 이미 출혈 중이면 중복 적용 안 함)
        if (canCauseBleeding && !_health.IsBleeding)
        {
            if (Random.value <= bleedChance)
                _health.StartBleeding();
        }

        Debug.Log($"{transform.root.name}의 {bodyPartName} 피격! 데미지: {finalDamage}");
    }
}
