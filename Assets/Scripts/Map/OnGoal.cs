using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnGoal : MonoBehaviour
{
    private GameObject timerText;
    private TextMeshProUGUI tmptext;
    private GameObject sceneMngObj;
    private SceneMng sceneMng;
    private float time = 0;
    private bool timerActive = true;
    private AudioSource source;
    private GameObject UISoundObj;
    private UISound uiSound;

    // Start is called before the first frame update
    void Awake()
    {
        timerText = GameObject.Find("Time");
        sceneMngObj = GameObject.Find("SceneMng");
        UISoundObj = GameObject.Find("UISound");

        sceneMng = sceneMngObj.GetComponent<SceneMng>();
        tmptext = timerText.GetComponent<TextMeshProUGUI>();
        source = GetComponent<AudioSource>();
        uiSound = UISoundObj.GetComponent<UISound>();
    }

    void Update()
    {
        if (timerActive)
        {
            time += Time.deltaTime;
            tmptext.text = time.ToString("00.00");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Enter Goal");

            /* stop player */
            PlayerCtlr ctlr = other.gameObject.GetComponent<PlayerCtlr>();
            ctlr.ReachGoal();

            /* stop timer */
            timerActive = false;
            tmptext.color = Color.green;

            /* stop BGM */
            sceneMng.StopBGM();

            /* start SE */
            source.Play();

            /* enable UI component */
            GameObject text_r = (GameObject)Resources.Load("GameClearMessage");
            Instantiate(text_r, GameObject.Find("Canvas").transform);
            GameObject button_r = (GameObject)Resources.Load("GoTitleButton");
            GameObject butObj = Instantiate(button_r, GameObject.Find("Canvas").transform);
            Button button = butObj.GetComponent<Button>();
            button.onClick.AddListener(sceneMng.GotoTitle);
            button.onClick.AddListener(uiSound.PlayEnterSE);
        }
    }
}