using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtlr : MonoBehaviour
{
    [SerializeField]
    GameObject wepone;
    [SerializeField]
    GameObject EscUI;
    [SerializeField]
    GameObject UISoundObj;
    [SerializeField]
    AudioClip footstep;
    [SerializeField]
    AudioClip attackSE;

    private AudioSource audioSource;
    private float gravityRate = 2.0f;
    private float moveSpeedRate = 4.0f;
    private CharacterController charaCtlr;
    private SimpleAnimator animator;
    private bool inAttack = false;
    private int state = 0;
    private float animStartTime;
    private bool inputEnable = true;
    private bool cameraEnable = true;
    private bool escUIEnable = true;
    private float speed = 0;
    private Vector3 beforePos;
    private bool inEsc = false;
    private bool footstepEnable = true;
    private float fs_timeDis = 0;
    private UISound uiSound;
    private Vector3 attackStepDir;
    private bool inAttackStep = false;

    public bool InputEnable
    {
        get { return inputEnable;  }
    }
    public bool CameraEnable
    {
        get { return cameraEnable; }
    }
    public bool EscUIEnable
    {
        get { return escUIEnable; }
    }
    public float Speed
    {
        get { return speed; }
    }


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        beforePos = this.transform.position;
        charaCtlr = GetComponent<CharacterController>();
        animator = GetComponent<SimpleAnimator>();
        DisableAttack();
        EscUI.SetActive(inEsc);
        uiSound = UISoundObj.GetComponent<UISound>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel") && escUIEnable)
        {
            uiSound.PlayEscSE();
            inEsc = !inEsc;
            EscUI.SetActive(inEsc);
            /* Back Game */
            if (inEsc)
            {
                /* Disable Input */
                inputEnable = false;
                /* Disable Camera rot */
                cameraEnable = false;
            }
            /* Stop Game */
            else
            {
                /* Disable Input */
                inputEnable = true;
                /* Disable Camera rot */
                cameraEnable = true;
            }
        }

        updateSpeed();

        /* Audio foot step */
        if (!footstepEnable)
        {
            fs_timeDis += Time.deltaTime;
            if (fs_timeDis >= 0.33f)
            {
                footstepEnable = true;
            }
        }
        if (this.Speed >= 0.2f && footstepEnable)
        {
            fs_timeDis = 0;
            footstepEnable = false;
            audioSource.PlayOneShot(footstep);
        }

        Vector3 moveDir = Vector3.zero;

        /* move */
        if (inputEnable)
        {
            moveDir = getStickInput();
            moveDir *= moveSpeedRate;
        }

        /* check attack */
        if (Input.GetMouseButtonDown(0) && inputEnable)
        {
            if (!inAttack)
            {
                animStartTime = Time.fixedTime;
                animator.CrossFade("Attack");
                inAttack = true;
                state = 3;
                StartCoroutine("attack1Enable");
                audioSource.PlayOneShot(attackSE);

                StartCoroutine("inAttackStepCol");
                attackStepDir = this.transform.forward * moveSpeedRate;
            }
            else if(state == 3)
            {
                animStartTime = Time.fixedTime;
                animator.CrossFade("Attack2");
                state = 4;
                StopCoroutine("attack1Enable");
                StartCoroutine("attack2Enable");
                audioSource.PlayOneShot(attackSE);

                StartCoroutine("inAttackStepCol");
                attackStepDir = this.transform.forward * moveSpeedRate;
            }
        }

        if (inAttack && Time.fixedTime - animStartTime > 1.0f)
        {
            inAttack = false;
            DisableAttack();
        }

        if (inAttack)
        {
            /* in attack */
            if (inAttackStep)
            {
                ExtMove.MoveWithRotation(this, attackStepDir);
            }
        }
        else if (moveDir.magnitude > 0.5f)
        {
            moveDir += Vector3.down * gravityRate;
            ExtMove.MoveWithRotation(this, moveDir);
            if (state != 2)
            {
                animator.CrossFade("Move");
                state = 2;
            }
        }
        else if(state != 1)
        {
            state = 1;
            animator.CrossFade("Idle");
        }

    }
    public void EnableAttack()
    {
        //Debug.Log("Enabled");
        wepone.layer = LayerMask.NameToLayer("Default");
    }
    public void DisableAttack()
    {
        //Debug.Log("Disabled");
        wepone.layer = LayerMask.NameToLayer("Invisible");
    }


    private IEnumerator attack1Enable()
    {
        yield return new WaitForSeconds(15.0f / 60.0f);
        EnableAttack();
        yield return new WaitForSeconds(25.0f / 60.0f);
        DisableAttack();
    }
    private IEnumerator attack2Enable()
    {
        yield return new WaitForSeconds(20.0f / 60.0f);
        EnableAttack();
        yield return new WaitForSeconds(10.0f / 60.0f);
        DisableAttack();
    }
    private IEnumerator inAttackStepCol()
    {
        inAttackStep = true;
        yield return new WaitForSeconds(10.0f / 60.0f);
        inAttackStep = false;
    }

    private void updateSpeed()
    {
        Vector3 diff = this.transform.position - beforePos;
        speed = diff.magnitude * 5.0f;
        beforePos = this.transform.position;
    }

    private Vector3 getStickInput()
    {
        float input_h, input_v;
        Vector3 inputDirection_f = Vector3.zero;
        Vector3 inputDirection_r = Vector3.zero;

        /* NORMAL INPUT */
        input_h = Input.GetAxis("Horizontal");
        input_v = Input.GetAxis("Vertical");

        if (Mathf.Abs(input_h) < 0.2f) input_h = 0.0f;
        if (Mathf.Abs(input_v) < 0.2f) input_v = 0.0f;
        //Debug.Log("(h,v) = ("+input_h+","+input_v+")");
        inputDirection_f = Camera.main.transform.forward;
        inputDirection_f.y = 0;
        inputDirection_f = inputDirection_f.normalized;
        inputDirection_r = ExtMathCalc.crossVector(Vector3.up, inputDirection_f);
        inputDirection_f *= input_v;
        inputDirection_r *= input_h;

        return inputDirection_f + inputDirection_r;
    }

    public void ReachGoal()
    {
        /* Disable Input */
        inputEnable = false;
        /* Disable Camera rot */
        cameraEnable = false;
        /* Disable Esc UI */
        escUIEnable = false;
    }
}
