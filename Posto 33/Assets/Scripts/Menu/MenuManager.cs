
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour{

    [SerializeField] private string nomeDoLevelJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelopcaoes;
    [SerializeField] private Button opcoesMenu;
    [SerializeField] private Button abrirOpcoesMenu;

    public void Jogar(){
        Debug.Log("AA");
        SceneManager.LoadScene("Jogo");
    }

    public void AbrirOpcaoes(){
        if (painelopcaoes && painelMenuInicial) {
            painelMenuInicial.SetActive(false);
            painelopcaoes.SetActive(true);
            EventSystem.current.SetSelectedGameObject(opcoesMenu.gameObject);

        }

    }

    public void fecharOpcoes(){
        if (painelopcaoes && painelMenuInicial) {
            painelopcaoes.SetActive(false);
            painelMenuInicial.SetActive(true);
            EventSystem.current.SetSelectedGameObject(abrirOpcoesMenu.gameObject);

        }

    }

    public void Sairjogo(){
        Debug.Log("Sair do jogo");
        Application.Quit();
    }

}
