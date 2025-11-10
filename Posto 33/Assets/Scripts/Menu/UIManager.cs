
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour{
    private PlayerMovement controls;
    private void Start()
    {
    }
    private void Update()
    {

    }

    public void ToggleMenuPanel()
    {
        if (!string.IsNullOrEmpty("Menu"))
        {
            SceneManager.LoadScene("Menu");
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido!");
        }
    }
    public void ToggleDeathPanel()
    {
        if (!string.IsNullOrEmpty("Morte"))
        {
            SceneManager.LoadScene("Morte");
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido!");
        }
    }
    public void ToggleWinPanel()
    {
        if (!string.IsNullOrEmpty("Vitoria"))
        {
            SceneManager.LoadScene("Vitoria");
        }
        else
        {
            Debug.LogWarning("Nome da cena não definido!");
        }
    }
}
