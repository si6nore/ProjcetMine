using UnityEngine;

public class HelpMenuUI : MonoBehaviour
{
    [Header("도움말 패널")]
    public GameObject helpPanel;

    private bool _isOpen = false;

    // FPSController에서 ESC 처리를 넘겨받아 호출
    public void Toggle()
    {
        _isOpen = !_isOpen;
        helpPanel.SetActive(_isOpen);

        // 도움말 열리면 커서 해제, 닫히면 다시 잠금
        Cursor.lockState = _isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isOpen;
    }

    public bool IsOpen => _isOpen;
}
