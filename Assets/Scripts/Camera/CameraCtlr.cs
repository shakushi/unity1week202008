using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtlr : MonoBehaviour
{
    public GameObject Player;
    public GameObject PlayerRoot;
    [SerializeField]
    public float cameraScale = 1.0f; /* <1.0>通常・人型 <1.5>:大型 */

    private float rotateSpeed_h = 6.0f;
    private float rotateSpeed_v = 3.0f;

    private PlayerCtlr pctlr;
    private Vector3 playerPosBefore;
    private Vector3 playerRootPosBefore;
    private float defCameraRootY;
    private float cameraDis;
    private float maxCameraY = 3.7f;
    private float minCameraY = -0.5f;
    private Vector3 preMousePos;
    private Vector3 wallHitPosition;

    // Start is called before the first frame update
    void Start()
    {
        pctlr = Player.GetComponent<PlayerCtlr>();
        playerPosBefore = Player.transform.position;
        playerRootPosBefore = PlayerRoot.transform.position;
        defCameraRootY = playerRootPosBefore.y + (cameraScale - 0.5f);
        preMousePos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerCurPos = Player.transform.position;
        Vector3 playerCurRootPos = PlayerRoot.transform.position;

        /* 将来的にカメラスケールを動的変化させるためUpdateで更新 */
        cameraDis = cameraScale * 3.0f;
        //Debug.Log("speed : " + pctlr.Speed);
        if (pctlr.Speed >= 0.2f)
        {
            //Playerの動きに追従
            followPlayer(playerCurPos, playerCurRootPos);
        }
        else
        {
            //カメラとPlayerの距離を直す
            modifyPos(playerCurRootPos);
        }

        /* Update Before Value */
        playerPosBefore = playerCurPos;
        playerRootPosBefore = playerCurRootPos;

        if (pctlr.CameraEnable)
        {
            rotateByInput();
        }

    }

    void followPlayer(Vector3 playerCurPos, Vector3 playerCurRootPos)
    {
        /* 移動中は重心を参照しない　カメラ揺れを防ぐため */
        Vector3 moveDiff = playerCurPos - playerPosBefore;
        Vector3 cameraDiff = this.transform.position - playerCurPos;
        //Debug.Log(cameraDiff);

        //Debug.Log("moveDiff = ("+moveDirection.x+","+moveDirection.y+","+moveDirection.z+")");
        //transform.Translate(moveDiff, Space.World);
        TransWithWallCheck(playerCurRootPos, moveDiff);
    }

    void modifyPos(Vector3 playerCurRootPos)
    {
        /* 移動していない時は重心を基準にカメラ移動(細かいアニメーションに追従するため) */
        Vector3 targetPos = playerCurRootPos;
        targetPos.y = defCameraRootY;

        Vector3 diffVector = targetPos - this.transform.position;
        float dis = diffVector.magnitude;
        if (dis > (cameraDis + 0.1f))
        {
            //transform.Translate(diffVector * Time.deltaTime * 0.6f, Space.World);
            TransWithWallCheck(playerCurRootPos, diffVector * Time.deltaTime * 0.6f);
        }
        else if (dis < (cameraDis - 0.1f))
        {
            //transform.Translate(-diffVector * Time.deltaTime * 0.6f, Space.World);
            TransWithWallCheck(playerCurRootPos, -diffVector * Time.deltaTime * 0.6f);
        }
        Quaternion lookRotation = Quaternion.LookRotation(diffVector);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 0.05f);
    }

    void rotateByInput()
    {
        float h, v;
        /* Input */
        Vector3 nowMousePos = Input.mousePosition;
        Vector3 mouseDiff = nowMousePos - preMousePos;
        mouseDiff = mouseDiff.normalized;
        h = mouseDiff.x;
        v = mouseDiff.y;
        preMousePos = nowMousePos;
        if (Mathf.Abs(h) < 0.2) h = 0.0f;
        if (Mathf.Abs(v) < 0.2) v = 0.0f;
        //Debug.Log("(h,v) = ("+h+","+v+")");
        float angle_h = h * rotateSpeed_h;
        float angle_v = v * rotateSpeed_v;

        float posy = this.transform.position.y;
        Vector3 playerPos = PlayerRoot.transform.position;
        /* 横回転 */
        transform.RotateAround(playerPos, Vector3.up, angle_h);
        /* 縦回転 */
        if (posy > maxCameraY) /* too high */
        {
            //Debug.Log("camera high adjust. posy = " + posy);
            transform.RotateAround(playerPos, ExtMathCalc.crossVector(Camera.main.transform.forward, Vector3.up), 0.1f);
        }
        else if (posy < minCameraY) /* too low */
        {
            //Debug.Log("camera low adjust. posy = " + posy);
            transform.RotateAround(playerPos, ExtMathCalc.crossVector(Camera.main.transform.forward, Vector3.up), -0.1f);
        }
        else /* normal */
        {
            transform.RotateAround(playerPos, ExtMathCalc.crossVector(Camera.main.transform.forward, Vector3.up), angle_v);
        }
    }

    private void TransWithWallCheck(Vector3 targetPosition, Vector3 desiredPosition)
    {
        if(WallCheck(targetPosition, desiredPosition))
        {
            //transform.Translate(wallHitPosition, Space.World); [TODO]fix
            transform.Translate(desiredPosition, Space.World);
        }
        else
        {
            transform.Translate(desiredPosition, Space.World);
        }
    }

    private bool WallCheck(Vector3 targetPosition, Vector3 desiredPosition)
    {
        RaycastHit wallHit;
        LayerMask mask = LayerMask.GetMask("Wall");
        if (Physics.Raycast(targetPosition, desiredPosition - targetPosition, out wallHit, Vector3.Distance(targetPosition, desiredPosition), mask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("RayCast Hit");
            wallHitPosition = wallHit.point;
            return true;
        }
        else
        {
            return false;
        }
    }
}
