using UnityEngine;
using UnityEngine.Audio;

namespace Quantum
{
    
    public class GlobalAudioController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public AudioMixer mixer;

        // volume deve ser entre 0.0001f e 1f
        public void SetVolume(float volume)
        {
            Debug.Log(volume);

            // Permite volume de 0.0001f até 2f (onde 1f = 0 dB, 2f ≈ +6 dB)
            volume = Mathf.Clamp(volume, 0.0001f, 2f);

            // Converte para decibéis
            float dB = Mathf.Log10(volume) * 20f;

            mixer.SetFloat("Volume", dB);
        }


    }
}
