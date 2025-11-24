using System;
using UnityEngine;

namespace Quantum
{
    public class InHouseColliderEvent : MonoBehaviour
    {
        public Action<Boolean> CanRainEvent;
        private Boolean canRain = true;
        public GameObject obj;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        private void OnTriggerExit(Collider other)
        {
            
            if (other.CompareTag("Player"))
            {
                canRain = !canRain;
                CanRainEvent?.Invoke(canRain);
                obj.SetActive(canRain);

            }
            Debug.Log(canRain);


        }

    }
}
