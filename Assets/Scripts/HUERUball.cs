using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUERUball : MonoBehaviour
{
    [SerializeField]
    public int maxChildNum = 2;
    [SerializeField]
    public float duplicTime = 6.0f;
    [SerializeField]
    public AudioClip boundSE;
    [SerializeField]
    public AudioClip duplicateSE;


    private BallCtlr ballCtlr;
    private GameObject lightObj;
    private GameObject nextball;
    private GameObject myself;
    private Rigidbody rd;
    private Light mylight;
    static float maxPower = 40.0f;
    static float minPower = 20.0f;
    private float startTime;
    private float ramdTime;
    private int count = 0;
    private bool onCol = false;
    private AudioSource audioSource;
    private bool boundSEEnable = true;
    private float boundSETime = 0;

    void Awake()
    {
        lightObj = transform.GetChild(0).gameObject;
        mylight = lightObj.GetComponent<Light>();
        myself = this.gameObject;
        rd = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ballCtlr = transform.parent.gameObject.GetComponent<BallCtlr>();
        Vector3 dir = randamVector();
        rd.AddForce(dir);
        startTime = Time.fixedTime;
        ramdTime = Random.Range(-1.5f, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        float nowtime = Time.fixedTime;
        if (rd.velocity.magnitude < 5 && !onCol)
        {
            StartCoroutine("AddForceCol");
            onCol = true;
        }
        if (Time.fixedTime - startTime > duplicTime + ramdTime)
        {
            count++;
            startTime = Time.fixedTime;
            audioSource.PlayOneShot(duplicateSE);
            duplicateSelf();
            rd.AddForce(randamVector());
            if (count == maxChildNum)
            {
                mylight.color = (Color.yellow - (Color.white * 0.2f));
            }
        }

        if (count > maxChildNum)
        {
            deleteSelf();
        }

        if (!boundSEEnable)
        {
            boundSETime += Time.deltaTime;
            if (boundSETime >= 0.33f)
            {
                boundSEEnable = true;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PlayerWepon")
        {
            if (!ballCtlr.Limit)
            {
                duplicateSelf(1); /* force_flag ON */
            }
            else
            {
                deleteSelf();
            }
        }
        else if(collision.gameObject.tag == "ball")
        {
            if (boundSEEnable)
            {
                audioSource.PlayOneShot(boundSE);
                boundSEEnable = false;
                boundSETime = 0;
            }
        }
    }

    private Vector3 randamVector()
    {
        float x = Random.value > 0.5f ? Random.Range(minPower, maxPower) : -Random.Range(minPower, maxPower);
        float y = Random.value > 0.5f ? Random.Range(minPower, maxPower) : -Random.Range(minPower, maxPower);
        float z = Random.value > 0.5f ? Random.Range(minPower, maxPower) : -Random.Range(minPower, maxPower);
        return new Vector3(x, y, z);
    }

    private void duplicateSelf(int? force_flag = 0)
    {
        if (ballCtlr.Limit) { return; }
        if (!ballCtlr.CanGenerateBall && force_flag == 0)
        //if (!ballCtlr.CanGenerateBall)
        {
            return;
        }        
        nextball = (GameObject)Resources.Load("HUERUball");
        if (nextball.transform.localScale.x < 1.5f && Random.value > 0.5f)
        {
            nextball.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        }
        else if (nextball.transform.localScale.x > 0.3f && Random.value > 0.2f)
        {
            nextball.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        }
        GameObject next = Instantiate(nextball, this.transform.position, Quaternion.identity);
        next.transform.parent = this.transform.parent;
    }

    private void deleteSelf()
    {
        // Stops all running coroutines
        StopAllCoroutines();
        Destroy(myself);
    }

    private IEnumerator AddForceCol()
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 4.0f));
        rd.AddForce(randamVector());
        onCol = false;
    }
}
