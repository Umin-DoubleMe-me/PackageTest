using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class XRIHand : MonoBehaviour
{
	[SerializeField] private InputActionProperty _pinchAction;
	[SerializeField] private XRDirectInteractor _selectInteractor;
	[SerializeField] private GameObject _targetPalm;

	[SerializeField] private float _palmCheckMinXValue;
	[SerializeField] private float _palmCheckMaxXValue;

	[SerializeField] private float _palmCheckMinYValue;
	[SerializeField] private float _palmCheckMaxYValue;

	[SerializeField] private float _palmCheckMinZValue;
	[SerializeField] private float _palmCheckMaxZValue;

	//엄지손가락부터 새끼손가락까지
	[HideInInspector] public Dictionary<XRHandJointID, Pose> DicOriginFingersLcoalPose = new Dictionary<XRHandJointID, Pose>();

	private XRHandSkeletonDriver _xRHandSkeleton;
	private Dictionary<XRHandJointID, Transform> _jointDictionary = new Dictionary<XRHandJointID, Transform>();

	public Action<bool> WhenPinch;
	public float PinchValue;

	private bool _isPinch = false;
	public bool IsPinch
	{
		get 
		{ 
			return _isPinch; 
		} 
		
		private set
		{
			if (_isPinch != value)
				WhenPinch?.Invoke(value);
			_isPinch = value;
		}
	}

	public Action<bool> PalmUpAction;
	private bool _isPalmUp = false;
	public bool IsPalmUp
	{
		get
		{
			return _isPinch;
		}

		private set
		{
			if (_isPalmUp != value)
				PalmUpAction?.Invoke(value);
			_isPalmUp = value;
		}
	}
	
	public void OnEnable()
	{
		_pinchAction.EnableDirectAction();
	}

	public void OnDisable()
	{
		_pinchAction.DisableDirectAction();
	}

	public void Start()
	{
		_xRHandSkeleton = GetComponentInChildren<XRHandSkeletonDriver>();
	}

	public void Update()
	{
		PinchChecking();
		CheckPalmUp();
	}

	private void PinchChecking()
	{
		PinchValue = _pinchAction.action.ReadValue<float>();

		if (PinchValue != 0)
			IsPinch = true;
		else
			IsPinch = false;
	}

	private void CheckPalmUp()
	{
		var targetHandUp = _targetPalm.transform.up;
		var targetHandLocalRot = Camera.main.transform.InverseTransformDirection(targetHandUp);

		var xrange = targetHandLocalRot.x > _palmCheckMinXValue && targetHandLocalRot.x < _palmCheckMaxXValue;
		var zrange = targetHandLocalRot.z > _palmCheckMinZValue && targetHandLocalRot.z < _palmCheckMaxZValue;
		var yrange = targetHandUp.y > _palmCheckMinYValue && targetHandUp.y < _palmCheckMaxYValue;

		if(xrange && yrange && zrange && !_selectInteractor.hasSelection)
		{
			IsPalmUp = true;
		}
		else
		{
			IsPalmUp = false;
		}
	}

	Transform palm;

	public void GetHandJointPose(out Pose pose, XRHandJointID xRHandJointID)
	{
		Transform resultTransform = FindJoint(xRHandJointID);
		pose = new Pose(resultTransform.position, resultTransform.rotation);
	}

	public void GetHandJointLocalPose(out Pose pose, XRHandJointID xRHandJointID)
	{
		Transform resultTransform = FindJoint(xRHandJointID);
		pose = new Pose(resultTransform.localPosition, resultTransform.localRotation);
	}

	private Transform FindJoint(XRHandJointID targetJoin)
	{
		Transform result;
		if (!_jointDictionary.TryGetValue(targetJoin, out result))
		{
			result = _xRHandSkeleton.jointTransformReferences.Find((index) => index.xrHandJointID == targetJoin).jointTransform;
			_jointDictionary[targetJoin] = result;
		}

		return result;
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;

		if(palm != null)
			Gizmos.DrawCube(palm.transform.position, Vector3.one * 0.03f);
	}
}