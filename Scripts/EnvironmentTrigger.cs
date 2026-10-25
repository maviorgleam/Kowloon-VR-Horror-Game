using UnityEngine;

public class EnvironmentTrigger : MonoBehaviour
{
    [Header("触发效果设置")]
    public AudioClip ambientSound;    
    public Light targetLight;         
    public float lightDimAmount = 0.4f; 
    public bool triggerOnce = true;   

    private AudioSource audioSource;
    private bool hasTriggered;
    private float originalLightIntensity;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        hasTriggered = false;
        
        if (targetLight != null)
        {
            originalLightIntensity = targetLight.intensity;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!triggerOnce || !hasTriggered))
        {
            hasTriggered = true;
            
            if (ambientSound != null)
            {
                audioSource.clip = ambientSound;
                audioSource.Play();
            }
            
            if (targetLight != null)
            {
                targetLight.intensity = originalLightIntensity * (1 - lightDimAmount);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !triggerOnce)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            if (targetLight != null)
            {
                targetLight.intensity = originalLightIntensity;
            }
        }
    }
}
