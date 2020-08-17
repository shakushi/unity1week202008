using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMng : MonoBehaviour
{
    private AudioSource source;

    // Start is called before the first frame update
    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GotoTitle()
    {
        SceneManager.LoadScene("Title");
    }
    public void GotoMain1()
    {
        SceneManager.LoadScene("Main1");
    }

    public void StopBGM()
    {
        source.Stop();
    }
}
