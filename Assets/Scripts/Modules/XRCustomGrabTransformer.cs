using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

/// 상호작용 순서
/// Hover Enter -> Grab -> SelectEnter -> Process .... -> SelectExit -> Hover Exit


public class XRCustomGrabTransformer : XRGeneralGrabTransformer
{
	public Action OnMove = null;

	public int CurMode; //2진수

	public Transform CurInteractorObj;
	public XRIHand CurInteractorHand;
	public ObjectManipulatorMode CurInteractorMode;

	private XRCustomInteractable _interactable;
	private bool _requestOwnership = false;
	private bool? _getresult = false;
	private bool _isInitialize = false;

	public Action<Vector3> OnBeginTransform;
	public Action<Vector3> OnUpdateTransform;
	public Action OnEndTransform;

	public void Init()
	{
		_interactable = GetComponent<XRCustomInteractable>();
		CurMode = (int)ObjectManipulatorMode.none;
		scaleMultiplier = 1;

		_isInitialize = true;
		AddEvent();
	}

	public void SetCurMode(params ObjectManipulatorMode[] mode)
	{
		int result = 0;
		foreach (var data in mode)
		{
			result |= (int)data;
		}
		CurMode = result;
	}

	public void AddMode(ObjectManipulatorMode mode)
	{
		CurMode |= (int)mode;
	}

	public void MinusMode(ObjectManipulatorMode mode)
	{
		CurMode &= ~(int)mode;
	}

	public void OnEnable()
	{
		if (_isInitialize)
			AddEvent();
	}

	public void OnDisable()
	{
		RemoveEvent();
	}

	private void AddEvent()
	{
		if (_interactable != null)
		{
			_interactable.selectEntered.AddListener(SetCurInteractorMode);
			_interactable.selectExited.AddListener(SetCurinteractorModeRelease);
		}
	}

	private void RemoveEvent()
	{
		if (_interactable != null)
		{
			_interactable.selectEntered.RemoveListener(SetCurInteractorMode);
			_interactable.selectExited.RemoveListener(SetCurinteractorModeRelease);
		}
	}

	private void SetCurInteractorMode(SelectEnterEventArgs eventArgs)
	{
		if (eventArgs.interactorObject == null) return;
		if (eventArgs.interactorObject.transform.GetComponent<IXRInteractorType>() == null) return;

		CurInteractorObj = eventArgs.interactorObject.transform;
		CurInteractorHand = CurInteractorObj.GetComponentInParent<XRIHand>();
		CurInteractorMode = CurInteractorObj.GetComponent<IXRInteractorType>().GetInteractorType();
	}

	private void SetCurinteractorModeRelease(SelectExitEventArgs eventArgs)
	{
		if (eventArgs.interactorObject == null) return;
		if (eventArgs.interactorObject.transform.GetComponent<IXRInteractorType>() == null) return;

		if (CurInteractorObj == eventArgs.interactorObject.transform)
		{
			CurInteractorObj = null;
			CurInteractorHand = null;
			CurInteractorMode = ObjectManipulatorMode.none;
		}
	}
	public override void OnGrab(XRGrabInteractable grabInteractable)
	{
		base.OnGrab(grabInteractable);

		_interactable.selectExited.AddListener(CheckGrabEnd);
		OnBeginTransform?.Invoke(GetGrabPoint());
		return;
	}
	public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
		if (CheckInteractActive())
		{
			OnMove?.Invoke();
			OnUpdateTransform?.Invoke(GetGrabPoint());

			base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);
		}
	}

	public bool IsRayModeOn()
	{
		if ((CurMode & (int)ObjectManipulatorMode.ray) != 0)
			return true;
		else
		{
			return false;
		}
	}

	private bool CheckInteractActive()
	{
		if ((CurMode & (int)CurInteractorMode) != 0)
		{
			if (!IsRayModeOn())
				return true;
			else
				return false;
		}
		else
			return false;
	}

	private void CheckGrabEnd(SelectExitEventArgs eventArgs)
	{
		_requestOwnership = false;
		_getresult = null;

		OnEndTransform?.Invoke();
		_interactable.selectExited.RemoveListener(CheckGrabEnd);
	}

	private Vector3 GetGrabPoint()
	{
		if (CurInteractorObj == null)
			return default;

		if (CurInteractorHand.IsPinch)
		{
			CurInteractorHand.GetHandJointPose(out Pose pose, UnityEngine.XR.Hands.XRHandJointID.IndexTip);
			return pose.position;
		}

		return default;
	}
}