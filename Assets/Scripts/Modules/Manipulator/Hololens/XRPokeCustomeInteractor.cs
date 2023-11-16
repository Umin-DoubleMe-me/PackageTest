using DoubleMe.Manager;
using DoubleMe.Modules.Manipulator;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRPokeCustomeInteractor : XRPokeInteractor , IXRInteractorType
{
	[SerializeField] private bool _isLeftHand = false;
	private XRIPlatformManipulator _xrPlatformManipulator;
	private ObjectManipulatorMode _thisInteractorMode = ObjectManipulatorMode.poke;
	
	public ObjectManipulatorMode GetInteractorType()
	{
		return _thisInteractorMode;
	}

	public void Init(XRIPlatformManipulator xrPlatformManipulator)
	{
		_xrPlatformManipulator = xrPlatformManipulator;
	}

	//protected override void OnEnable()
	//{
	//	base.OnEnable();

	//	uiHoverEntered.AddListener(LimitOnPokeGestureOn);
	//	uiHoverExited.AddListener(ReleasePokeGesture);
	//}

	//protected override void OnDisable()
	//{
	//	base.OnDisable();

	//	uiHoverEntered.RemoveListener(LimitOnPokeGestureOn);
	//	uiHoverExited.RemoveListener(ReleasePokeGesture);
	//}

	//private void LimitOnPokeGestureOn(UIHoverEventArgs hoverEnterEventArgs)
	//{
	//	if (_isLeftHand)
	//		_platformManipulator.ChangeModeLeftHand(ManipulatorMode.None);
	//	else
	//		_platformManipulator.ChangeModeRightHand(ManipulatorMode.None);
	//}

	//private void ReleasePokeGesture(UIHoverEventArgs hoverExitEventArgs)
	//{
	//	if (_isLeftHand)
	//		_platformManipulator.ChangeModeLeftHand();
	//	else
	//		_platformManipulator.ChangeModeRightHand();
	//}
}
