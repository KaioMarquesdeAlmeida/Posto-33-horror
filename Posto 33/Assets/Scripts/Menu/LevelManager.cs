using UnityEngine;
using UnityEngine;
public class LevelManager : MonoBehaviour{

    public static LevelManager instance;
    [SerializeField] private UIManager _ui;

    private void Awake(){
        if (LevelManager.instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void GameOver()
    {
        if (_ui != null)
        {
            _ui.ToggleDeathPanel();

        }
    }

}
