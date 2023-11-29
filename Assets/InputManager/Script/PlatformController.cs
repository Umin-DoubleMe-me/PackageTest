using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class InteractorCollector
{
	public IPokeInteractor pokeInteractor;
	public IGrabInteractor grabInteractor;
	public IRayInteractor rayInteractor;
	public IDistanceInteractor distanceInteractor;
}


public abstract class APlatfromController : MonoBehaviour, IPlatformController
{
	[Header("RightHand")]
	[SerializeField] private GameObject _rightHandObj;
	[SerializeField] private GameObject _rightpokeInteractor;
	[SerializeField] private GameObject _rightgrabInteractor;
	[SerializeField] private GameObject _rightrayInteractor;
	[SerializeField] private GameObject _rightdistanceInteractor;
	[Header("LeftHand")]
	[SerializeField] private GameObject _leftHandObj;
	[SerializeField] private GameObject _leftpokeInteractor;
	[SerializeField] private GameObject _leftgrabInteractor;
	[SerializeField] private GameObject _leftrayInteractor;
	[SerializeField] private GameObject _leftdistanceInteractor;

	private IPlatformHand _rightHand;
	private IPlatformHand _leftHand;
	private InteractorCollector _rightHandInteractor;
	private InteractorCollector _leftHandInteractor;

	public virtual void Init()
	{

	}

	protected virtual void Start()
	{
		if(!_rightHandObj.TryGetComponent(out  _rightHand))
		{
			Debug.LogError("Can't find RightHand");
			return;
		}
		
		if(!_leftHandObj.TryGetComponent(out _leftHand))
		{
			Debug.LogError("Can't find _leftHand");
			return;
		}

		_rightHandInteractor = new InteractorCollector()
		{
			pokeInteractor = _rightpokeInteractor.GetComponent<IPokeInteractor>(),
			grabInteractor = _rightgrabInteractor.GetComponent<IGrabInteractor>(),
			rayInteractor = _rightrayInteractor.GetComponent<IRayInteractor>(),
			distanceInteractor = _rightdistanceInteractor.GetComponent<IDistanceInteractor>(),
		};

		_leftHandInteractor = new InteractorCollector()
		{
			pokeInteractor = _leftpokeInteractor.GetComponent<IPokeInteractor>(),
			grabInteractor = _leftgrabInteractor.GetComponent<IGrabInteractor>(),
			rayInteractor = _leftrayInteractor.GetComponent<IRayInteractor>(),
			distanceInteractor = _leftdistanceInteractor.GetComponent<IDistanceInteractor>(),
		};

		GenerateEventSystem();
	}

	public abstract Task InitializeManipulator(GameObject targetObj);

	public virtual bool GetJointPose(bool isRightHand, int handId, out Pose pose, bool isLocal)
	{
		if (isRightHand)
			return _rightHand.GetJointPose(true, handId, out pose, isLocal);
		else
			return _leftHand.GetJointPose(false, handId, out pose, isLocal);
	}

	public virtual bool GetFingerIsPinching(bool isRightHand, int handId)
	{
		if (isRightHand)
			return _rightHand.GetFingerIsPinching(true, handId);
		else
			return _leftHand.GetFingerIsPinching(false, handId);
	}

	public virtual Pose GetHandPose(bool isRightHand)
	{
		if (isRightHand)
			return _rightHand.GetHandPose(true);
		else
			return _leftHand.GetHandPose(false);
	}

	public virtual bool IsHandTracking(bool isRightHand)
	{
		if (isRightHand)
			return _rightHand.IsHandTracking(true);
		else
			return _leftHand.IsHandTracking(false);
	}

	public virtual bool GetGrabInteractor(bool isRightHand, out IGrabInteractor grabInteractor)
	{
		grabInteractor = null;

		if (isRightHand)
		{
			if (_rightHandInteractor.grabInteractor != null)
			{
				grabInteractor = _rightHandInteractor.grabInteractor;
				return true;
			}
			else
				return false;
		}
		else
		{
			if (_leftHandInteractor.grabInteractor != null)
			{
				grabInteractor = _leftHandInteractor.grabInteractor;
				return true;
			}
			else
				return false;
		}
	}

	public virtual bool GetPokeInteractor(bool isRightHand, out IPokeInteractor pokeInteractor)
	{
		pokeInteractor = null;

		if (isRightHand)
		{
			if (_rightHandInteractor.pokeInteractor != null)
			{
				pokeInteractor = _rightHandInteractor.pokeInteractor;
				return true;
			}
			else
				return false;
		}
		else
		{
			if (_leftHandInteractor.pokeInteractor != null)
			{
				pokeInteractor = _leftHandInteractor.pokeInteractor;
				return true;
			}
			else
				return false;
		}
	}

	public virtual bool GetRayInteractor(bool isRightHand, out IRayInteractor rayInteractor)
	{
		rayInteractor = null;

		if (isRightHand)
		{
			if (_rightHandInteractor.rayInteractor != null)
			{
				rayInteractor = _rightHandInteractor.rayInteractor;
				return true;
			}
			else
				return false;
		}
		else
		{
			if (_leftHandInteractor.rayInteractor != null)
			{
				rayInteractor = _leftHandInteractor.rayInteractor;
				return true;
			}
			else
				return false;
		}
	}

	public virtual bool GetDistanceInteractor(bool isRightHand, out IDistanceInteractor distanceInteractor)
	{
		distanceInteractor = null;

		if (isRightHand)
		{
			if (_rightHandInteractor.distanceInteractor != null)
			{
				distanceInteractor = _rightHandInteractor.distanceInteractor;
				return true;
			}
			else
				return false;
		}
		else
		{
			if (_leftHandInteractor.distanceInteractor != null)
			{
				distanceInteractor = _leftHandInteractor.distanceInteractor;
				return true;
			}
			else
				return false;
		}
	}

	protected abstract void GenerateEventSystem();

}
