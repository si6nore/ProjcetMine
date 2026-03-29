using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

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

    [Header("적 전용 (사망 연출)")]
    public Vector3 deathRotation = new Vector3(90f, 0f, 0f);
    public float deathRotateSpeed = 5f;

    public float CurrentHealth => _currentHealth;
    public bool IsDead => _isDead;

    void Start()
    {
        _currentHealth = maxHealth;

        if (isPlayer)
        {
            if (hitFlashImage != null)
                hitFlashImage.gameObject.SetActive(false);
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlayer && _isDead)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(deathRotation),
                Time.deltaTime * deathRotateSpeed
            );
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}의 현재 체력: {_currentHealth}");

        if (isPlayer)
        {
            // 화면 붉게 깜빡임
            if (hitFlashImage != null)
            {
                StopCoroutine("FlashRedScreen");
                StartCoroutine(FlashRedScreen());
            }

            // ✅ 수정: StartCoroutine으로 카메라 흔들림
            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.2f, 0.15f));
        }

        if (_currentHealth <= 0) Die();
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

    void Die()
    {
        _isDead = true;

        if (isPlayer)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Invoke("QuitGame", 3f);
        }
        else
        {
            if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;
            if (GetComponent<EnemyAI>()) GetComponent<EnemyAI>().enabled = false;

            Destroy(gameObject, 3f);
        }
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
