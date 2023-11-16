using DoubleMe.Modules.Manipulator;
using UnityEngine.XR.Interaction.Toolkit;

public class XRDistanceCustomInteractor : XRRayInteractor , IXRInteractorType
{
	private ObjectManipulatorMode _thisInteractorMode = ObjectManipulatorMode.distance;

	public ObjectManipulatorMode GetInteractorType()
	{
		return _thisInteractorMode;
	}

	public void Init(XRIPlatformManipulator xRIPlatformManipulator)
	{
	}
}
