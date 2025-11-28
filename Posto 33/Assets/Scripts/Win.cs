using UnityEngine;
using UnityEngine.SceneManagement;


    public class Win : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnTriggerEnter(Collider other)
        {
            // Mostra no console a tag do objeto que entrou no trigger
            Debug.Log(other.gameObject.tag);

            if (other.CompareTag("Player"))
            {
                SceneManager.LoadScene("Vitoria");
            }
        }


}
