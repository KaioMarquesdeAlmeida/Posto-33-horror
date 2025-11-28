using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour{

    [SerializeField] private string nomeDoLevelJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelopcaoes;

    public void teste() { 
        Debug.Log("AA");

    }
    public void Jogar(){
        Debug.Log("AA");
        SceneManager.LoadScene("Jogo");
    }

    public void AbrirOpcaoes(){
        if (painelopcaoes && painelMenuInicial) {
            painelMenuInicial.SetActive(false);
            painelopcaoes.SetActive(true);
        }
        
    }

    public void fecharOpcoes(){
        if (painelopcaoes && painelMenuInicial) {
            painelopcaoes.SetActive(false);
            painelMenuInicial.SetActive(true);
        }
        
    }

    public void Sairjogo(){
        Debug.Log("Sair do jogo");
        Application.Quit();
    }

}
