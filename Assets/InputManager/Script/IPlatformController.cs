
using System.Threading.Tasks;
using UnityEngine;

public interface IPlatformController : IPlatformHand, IPlatformInteractor
{
	void Init();
	Task InitializeManipulator(GameObject targetObj);
}


public interface IPlatformHand
{
	bool GetJointPose(bool isRightHand, int handId, out Pose pose, bool isLocal);
	bool GetFingerIsPinching(bool isRightHand, int handId);
	Pose GetHandPose(bool isRightHand);
	bool IsHandTracking(bool isRightHand);
}

public interface IPlatformInteractor
{
	bool GetGrabInteractor(bool isRightHand, out IGrabInteractor grabInteractor);
	bool GetPokeInteractor(bool isRightHand, out IPokeInteractor pokeInteractor);
	bool GetRayInteractor(bool isRightHand, out IRayInteractor rayInteractor);
	bool GetDistanceInteractor(bool isRightHand, out IDistanceInteractor distanceInteractor);
}

public interface IGrabInteractor
{

}

public interface IPokeInteractor
{

}

public interface IRayInteractor
{

}

public interface IDistanceInteractor
{

}
