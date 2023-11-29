using Oculus.Interaction.Input;
using UnityEngine;

public class OculusPlatformHand : Hand, IPlatformHand
{
	[SerializeField] private bool _isRightHand = false;
	[SerializeField] private Transform _handObj;

	[SerializeField] private GameObject _visualHand;
	[SerializeField] private GameObject _visualController;

	private SkinnedMeshRenderer _handRender;

	protected override  void Start()
	{
		base.Start();

		if (_visualHand == null) return;
		if (!_visualHand.TryGetComponent(out _handRender))
		{
			Debug.LogError("There is no HandRenderer");
			return;
		}
	}

	public bool GetFingerIsPinching(bool isRightHand, int handId)
	{
		if (!IsHandSideCorrect(isRightHand)) return false;

		return GetFingerIsPinching((HandFinger)handId);
	}

	public Pose GetHandPose(bool isRightHand)
	{
		if (!IsHandSideCorrect(isRightHand)) return new Pose();

		return new Pose(_handObj.position, _handObj.rotation);
	}

	public bool GetJointPose(bool isRightHand, int handId, out Pose pose, bool isLocal)
	{
		pose = new Pose();
		if (!IsHandSideCorrect(isRightHand)) return false;

		if(isLocal)
			return GetJointPoseLocal((HandJointId)handId, out pose);
		else
			return GetJointPose((HandJointId)handId, out pose);

	}

	public bool IsHandTracking(bool isRightHand)
	{
		if (!IsHandSideCorrect(isRightHand)) return false;

		if (_handRender.enabled && _visualController.activeSelf)
			return true;

		return false;
	}

	private bool IsHandSideCorrect(bool isRightHand)
	{
		if (_isRightHand == isRightHand)
			return true;
		else
		{
			Debug.LogError("Hand Side is wrong");
			return false;
		}
	}
}
