using DoubleMe.Modules.Manipulator;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGrabCustomInteractor : XRDirectInteractor , IXRInteractorType
{
	[SerializeField] private bool _isLeftHand = false;
	private ObjectManipulatorMode _thisInteractorMode = ObjectManipulatorMode.grab;
	private XRIPlatformManipulator _xrPlatformManipulator;
	
	public ObjectManipulatorMode GetInteractorType()
	{
		return _thisInteractorMode;
	}

	public void Init(XRIPlatformManipulator xrPlatformManipulator)
	{
		_xrPlatformManipulator = xrPlatformManipulator;
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		selectEntered.AddListener(LimitOnPokeGestureOn);
		selectExited.AddListener(ReleasePokeGesture);
	}
	protected override void OnDisable()
	{
		base.OnDisable();

		selectEntered.RemoveListener(LimitOnPokeGestureOn);
		selectExited.RemoveListener(ReleasePokeGesture);
	}

	private void LimitOnPokeGestureOn(SelectEnterEventArgs hoverEnterEventArgs)
	{
		if (_isLeftHand)
			_xrPlatformManipulator.IsLeftRayCanActive = false;
		else
			_xrPlatformManipulator.IsRightRayCanActive = false;
	}

	private void ReleasePokeGesture(SelectExitEventArgs hoverExitEventArgs)
	{
		if (_isLeftHand)
			_xrPlatformManipulator.IsLeftRayCanActive = true;
		else
			_xrPlatformManipulator.IsRightRayCanActive = true;
	}
}
