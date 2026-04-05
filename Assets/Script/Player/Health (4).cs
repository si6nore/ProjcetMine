using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class Health : MonoBehaviour
{
    [Header("공통 설정")]
    public float maxHealth = 100f;
    private float _currentHealth;
    private bool _isDead = false;
    public bool isPlayer = false;

    [Header("플레이어 전용 (UI/Camera)")]
    public Image hitFlashImage;
    public float flashDuration = 0.2f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f);
    public GameObject gameOverPanel;
    public CameraShake cameraShake;

    [Header("출혈 UI (플레이어 전용)")]
    public TMP_Text bleedingText;
    public Image bleedingFlashImage;

    [Header("출혈 설정")]
    public float bleedDamagePerSecond = 1.5f;

    private bool _isBleeding = false;
    private Coroutine _bleedCoroutine;

    public float CurrentHealth => _currentHealth;
    public bool IsDead => _isDead;
    public bool IsBleeding => _isBleeding;

    void Start()
    {
        _currentHealth = maxHealth;
        DisableRagdoll();

        if (isPlayer)
        {
            if (hitFlashImage != null)
                hitFlashImage.gameObject.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            if (bleedingText != null)
                bleedingText.gameObject.SetActive(false);
            if (bleedingFlashImage != null)
                bleedingFlashImage.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}의 현재 체력: {_currentHealth}");

        if (isPlayer && damage > 0)
        {
            if (hitFlashImage != null)
            {
                StopCoroutine("FlashRedScreen");
                StartCoroutine(FlashRedScreen());
            }

            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.2f, 0.15f));
        }

        if (_currentHealth <= 0) Die();
    }

    private void TakeBleedDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} 출혈 데미지: {damage}, 현재 체력: {_currentHealth}");

        if (isPlayer && bleedingFlashImage != null)
            StartCoroutine(FlashBleedScreen());

        if (_currentHealth <= 0) Die();
    }

    public void StartBleeding()
    {
        if (_isDead || _isBleeding) return;

        _isBleeding = true;

        if (isPlayer && bleedingText != null)
            bleedingText.gameObject.SetActive(true);

        _bleedCoroutine = StartCoroutine(BleedCoroutine());
        Debug.Log($"{gameObject.name} 출혈 시작!");
    }

    public void StopBleeding()
    {
        if (!_isBleeding) return;

        _isBleeding = false;

        if (_bleedCoroutine != null)
        {
            StopCoroutine(_bleedCoroutine);
            _bleedCoroutine = null;
        }

        if (isPlayer)
        {
            if (bleedingText != null)
                bleedingText.gameObject.SetActive(false);
            if (bleedingFlashImage != null)
                bleedingFlashImage.gameObject.SetActive(false);
        }

        Debug.Log($"{gameObject.name} 출혈 멈춤!");
    }

    IEnumerator BleedCoroutine()
    {
        while (_isBleeding && !_isDead)
        {
            yield return new WaitForSeconds(1f);
            if (_isDead) break;
            TakeBleedDamage(bleedDamagePerSecond);
        }
    }

    void Die()
    {
        _isDead = true;
        StopBleeding();

        if (isPlayer)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Invoke("QuitGame", 3f);
        }
        else
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null) anim.enabled = false;

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            EnemyAI ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;

            EnableRagdoll();
            Destroy(gameObject, 5f);
        }
    }

    void DisableRagdoll()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb.GetComponent<HitBox>() != null) continue;

            rb.isKinematic = true;

            foreach (Collider col in rb.GetComponents<Collider>())
                col.enabled = false;
        }
    }

    void EnableRagdoll()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb.GetComponent<HitBox>() != null) continue;

            // ✅ isKinematic 해제 먼저 → 그 다음 속도 0 설정
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            foreach (Collider col in rb.GetComponents<Collider>())
                col.enabled = true;
        }
    }

    IEnumerator FlashBleedScreen()
    {
        bleedingFlashImage.gameObject.SetActive(true);
        float elapsed = 0f;
        float duration = 0.3f;
        Color bleedColor = new Color(0.6f, 0f, 0f, 0.35f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bleedingFlashImage.color = Color.Lerp(bleedColor, new Color(0.6f, 0f, 0f, 0f), elapsed / duration);
            yield return null;
        }

        bleedingFlashImage.gameObject.SetActive(false);
    }

    IEnumerator FlashRedScreen()
    {
        hitFlashImage.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            hitFlashImage.color = Color.Lerp(flashColor, new Color(1f, 0f, 0f, 0f), elapsed / flashDuration);
            yield return null;
        }
        hitFlashImage.gameObject.SetActive(false);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
