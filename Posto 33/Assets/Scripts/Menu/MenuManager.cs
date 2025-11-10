using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour{

    [SerializeField] private string nomeDoLevelJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelopcaoes;

    public void jogar(){
        SceneManager.LoadScene(nomeDoLevelJogo);
    }

    public void AbrirOpcaoes(){
        painelMenuInicial.SetActive(false);
        painelopcaoes.SetActive(true);
    }

    public void fecharOpcoes(){
        painelopcaoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }

    public void Sairjogo(){
        Debug.Log("Sair do jogo");
        Application.Quit();
    }

}
