using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtMove
{
    public static void MoveWithRotation(MonoBehaviour charactor, Vector3 moveDirection, Vector3? target = null)
    {
        Quaternion lookRotation;
        CharacterController controller = charactor.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.Log("failed to get CharactorController");
            return;
        }
        controller.Move(moveDirection * Time.deltaTime);

        if (target == null) /* not in target Cameta */
        {
            lookRotation = Quaternion.LookRotation(moveDirection);
        }
        else /* in target Camera */
        {
            lookRotation = Quaternion.LookRotation((Vector3)target);
        }
        lookRotation.x = 0;
        lookRotation.z = 0;
        charactor.transform.rotation = Quaternion.Lerp(charactor.transform.rotation, lookRotation, 0.2f);
    }

    public static void MoveWithNoRot(MonoBehaviour charaobj, Vector3 moveDirection)
    {
        CharacterController controller = charaobj.GetComponent<CharacterController>();
        controller.Move(moveDirection * Time.deltaTime);
    }

    public static bool IsMoving(Vector3 normalizeMoveDirection)
    {
        /* 移動判定 */
        if ((Mathf.Abs(normalizeMoveDirection.x) + Mathf.Abs(normalizeMoveDirection.z)) < 0.05f) /* 移動量が少なすぎる時は無視する */
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static void MoveWithSpeedyRot(MonoBehaviour charactor, Vector3 moveDirection, Vector3? target = null)
    {
        Quaternion lookRotation;
        CharacterController controller = charactor.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.Log("failed to get CharactorController");
            return;
        }
        controller.Move(moveDirection * Time.deltaTime);

        if (target == null) /* not in target Cameta */
        {
            lookRotation = Quaternion.LookRotation(moveDirection);
        }
        else /* in target Camera */
        {
            lookRotation = Quaternion.LookRotation((Vector3)target);
        }
        lookRotation.x = 0;
        lookRotation.z = 0;
        charactor.transform.rotation = Quaternion.Lerp(charactor.transform.rotation, lookRotation, 0.8f);
    }
}
