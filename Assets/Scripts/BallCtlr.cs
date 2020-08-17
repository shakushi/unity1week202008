using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BallCtlr : MonoBehaviour
{
    [SerializeField]
    public int maxBallNum = 20;
    [SerializeField]
    public TextMeshProUGUI ballNumText;

    public bool CanGenerateBall
    {
        get { return !overNum; }
    }
    public bool Limit
    {
        get { return realLimit; }
    }


    private bool overNum = false;
    private bool realLimit = false;
    private bool inTitle = false;

    // Start is called before the first frame update
    void Start()
    {
        if (ballNumText == null)
        {
            inTitle = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int ObjCount = this.transform.childCount;
        overNum = (ObjCount > maxBallNum);
        realLimit = (ObjCount > maxBallNum + 5);
        if (inTitle)
        {
            return;
        }
        ballNumText.text = ObjCount.ToString("D2");
        if (overNum)
        {
            ballNumText.color = Color.yellow;
        }
        else
        {
            ballNumText.color = Color.white;
        }
    }
}
