using DoubleMe.Modules.Manipulator;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

[Serializable]
public class XRIHandComponents
{
	public static Pose DefaultPose = new Pose(Vector3.zero, Quaternion.identity);
	public Pose DummyPoseOffset;
	public GameObject HandVisualObj;
	public XRIHand Hand;
	public XRHandTrackingEvents XRHandTracking;
	[SerializeField] private Vector3 _handDireactionOffset;

	private Transform _virtualRoot;
	public bool IsTrack => XRHandTracking.handIsTracked;
	public Pose CurrentPose
	{
		get
		{
			if (!IsTrack)
			{
				if (Camera.main != null)
				{
					Matrix4x4 matLocal = Matrix4x4.TRS(DummyPoseOffset.position, DummyPoseOffset.rotation, Vector3.one);
					Matrix4x4 parentWorld = Matrix4x4.TRS(Camera.main.transform.position, Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0), Vector3.one);
					Matrix4x4 invMine = _virtualRoot.worldToLocalMatrix;
					Matrix4x4 matResult = invMine * parentWorld * matLocal;
					return new Pose(matResult.GetColumn(3), Quaternion.LookRotation(matResult.GetColumn(2), matResult.GetColumn(1)));
				}
			}

			return new Pose(HandVisualObj.transform.position, HandVisualObj.transform.rotation * Quaternion.Euler(_handDireactionOffset));
		}
	}
	public void InitializeDummyHandRoot(Transform root) => _virtualRoot = root;
}


public class XRIPlatformHand : MonoBehaviour, IPlatformHand
{
	[SerializeField] private XRIHandComponents _XRILeftHandComponents;
	[SerializeField] private XRIHandComponents _XRIRightHandComponents;
	private List<XRHandJointID> _tipIndexList = new List<XRHandJointID>();

	public XRIHandComponents LeftHandComponents => _XRILeftHandComponents;
	public XRIHandComponents RightHandComponents => _XRIRightHandComponents;

	public Pose LeftHandPose => _XRILeftHandComponents != null ? _XRILeftHandComponents.CurrentPose : XRIHandComponents.DefaultPose;

	public Pose RightHandPose => _XRIRightHandComponents != null ? _XRIRightHandComponents.CurrentPose : XRIHandComponents.DefaultPose;

	public Pose LeftPointPose
	{
		get
		{
			Pose pose = XRIHandComponents.DefaultPose;

			if (_XRILeftHandComponents != null)
				_XRILeftHandComponents.Hand.GetHandJointPose(out pose, XRHandJointID.IndexTip);

			return pose;
		}
	}

	public Pose RightPointPose
	{
		get
		{
			Pose pose = XRIHandComponents.DefaultPose;

			if (_XRIRightHandComponents != null)
				_XRIRightHandComponents.Hand.GetHandJointPose(out pose, XRHandJointID.IndexTip);

			return pose;
		}
	}

	public bool IsLeftPinch => _XRILeftHandComponents != null ? _XRILeftHandComponents.Hand.IsPinch : false;
	public bool IsRightPinch => _XRIRightHandComponents != null ? _XRIRightHandComponents.Hand.IsPinch : false;

	public bool IsLeftTracking => _XRILeftHandComponents.IsTrack;
	public bool IsRightTracking => _XRIRightHandComponents.IsTrack;

	protected virtual void Start()
	{
		_tipIndexList = new List<XRHandJointID>
		{
			XRHandJointID.ThumbTip,
			XRHandJointID.IndexTip,
			XRHandJointID.MiddleTip,
			XRHandJointID.RingTip,
			XRHandJointID.LittleTip,
		};

		_XRILeftHandComponents.InitializeDummyHandRoot(this.transform.root);
		_XRIRightHandComponents.InitializeDummyHandRoot(this.transform.root);
	}

	public virtual Pose GetFinger(bool isRightHand, int fingerIdx, bool isLocal)
	{
		XRIHandComponents handComp = isRightHand ? _XRIRightHandComponents : _XRILeftHandComponents;
		Pose fingerPose;

		if (handComp != null)
		{
			if (isLocal)
				handComp.Hand.GetHandJointLocalPose(out fingerPose, (XRHandJointID)fingerIdx);
			else
				handComp.Hand.GetHandJointPose(out fingerPose, (XRHandJointID)fingerIdx);
		}
		else
			fingerPose = new Pose(Vector3.zero, Quaternion.identity);

		return fingerPose;
	}

	public virtual Pose GetFingerTrackingLose(bool isRightHand, int fingerIdx, bool isLocal)
	{
		Pose palmCurPose = isRightHand ? RightHandPose : LeftHandPose;
		Pose originLocalPose = isRightHand ? RightHandComponents.Hand.DicOriginFingersLcoalPose[(XRHandJointID)fingerIdx] : LeftHandComponents.Hand.DicOriginFingersLcoalPose[(XRHandJointID)fingerIdx];

		Matrix4x4 matLocal = Matrix4x4.TRS(originLocalPose.position, originLocalPose.rotation, Vector3.one);
		Matrix4x4 parentWorld = Matrix4x4.TRS(palmCurPose.position, palmCurPose.rotation, Vector3.one);
		Matrix4x4 matResult = parentWorld * matLocal;
		
		return new Pose(matResult.GetColumn(3), Quaternion.LookRotation(matResult.GetColumn(2), matResult.GetColumn(1)));
	}


	public virtual void GetIKFingers(out Vector3[] position, out Quaternion[] rotation)
	{
		position = new Vector3[10];
		rotation = new Quaternion[10];

		for (int i = 0; i < 5; i++)
		{
			int fingerTip = (int)(_tipIndexList[i]);

			Pose pose = IsLeftTracking ? GetFinger(false, fingerTip, false) : GetFingerTrackingLose(false, fingerTip, false);
			position[i] = pose.position;
			rotation[i] = pose.rotation;

			pose = IsRightTracking ? GetFinger(true, fingerTip, false) : GetFingerTrackingLose(true, fingerTip, false);
			position[i + FingerEnum.IKPOINT] = pose.position;
			rotation[i + FingerEnum.IKPOINT] = pose.rotation;
		}
	}

	public virtual void SetOriginFingerLocalPose(List<Pose> localPoseData)
	{
		List<Pose> leftOri = localPoseData.GetRange(0, 5);
		List<Pose> rightOri = localPoseData.GetRange(5, 5);

		for(int index = 0; index < leftOri.Count; index++)
		{
			LeftHandComponents.Hand.DicOriginFingersLcoalPose[_tipIndexList[index]] = leftOri[index];
			RightHandComponents.Hand.DicOriginFingersLcoalPose[_tipIndexList[index]] = rightOri[index];
		}
	}

}
