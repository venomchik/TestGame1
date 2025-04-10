using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathHandler : MonoBehaviour
{
    public Animator playerAnimator;         
    public GameObject deathScreenUI;         
    public Button restartButton;            

    private bool deathHandled = false;

    void Start()
    {
        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartScene);
    }

    void Update()
    {
        if (!deathHandled && playerAnimator != null && playerAnimator.GetBool("Death"))
        {
            ShowDeathScreen();
            deathHandled = true;
        }
    }

    void ShowDeathScreen()
    {
        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
