using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem.EnhancedTouch;

public class ShieldSample : MonoBehaviour
{
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;

    private void Update()
    {
        if (_leftHand != null && _rightHand != null)
        {
            Vector3 centerPos = (_leftHand.position + _rightHand.position) * 0.5f;
            Vector3 centerPosDirection = (_leftHand.up + -_rightHand.up) * 0.05f;
            Vector3 lookAtDirection = centerPos - (centerPos + centerPosDirection);
            Quaternion lookAtRotation = Quaternion.LookRotation(lookAtDirection);
            this.transform.position = centerPos + centerPosDirection;
            this.transform.rotation = lookAtRotation;
        }
    }
}
