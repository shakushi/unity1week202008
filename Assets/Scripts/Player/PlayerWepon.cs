using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWepon : MonoBehaviour
{
    [SerializeField]
    public AudioClip attackedSE;
    [SerializeField]
    public GameObject Player;

    private AudioSource audioSource;
    private bool SEstop = false;
    private PlayerCtlr playerCtlr;

    void Awake()
    {
        playerCtlr = Player.GetComponent<PlayerCtlr>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "ball" && !SEstop)
        {
            playerCtlr.DisableAttack();
            SEstop = true;
            StartCoroutine("seEnable");
        }
    }

    private IEnumerator seEnable()
    {
        audioSource.PlayOneShot(attackedSE);
        yield return new WaitForSeconds(20.0f / 60.0f);
        SEstop = false;
    }
}
