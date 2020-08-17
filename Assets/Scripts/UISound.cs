using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField]
    public AudioClip s_EnterSE;
    [SerializeField]
    public AudioClip s_EscSE;

    private AudioSource source;

    // Start is called before the first frame update
    void Awake()
    {
        source = GetComponent<AudioSource>();        
    }

    public void PlayEnterSE()
    {
        source.PlayOneShot(s_EnterSE);
    }

    public void PlayEscSE()
    {
        source.PlayOneShot(s_EscSE);
    }
}
