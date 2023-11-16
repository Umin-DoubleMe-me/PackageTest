using DoubleMe.Manager;
using DoubleMe.Modules.Manipulator;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRRayCustomInteractor : XRRayInteractor , IXRInteractorType, ITwinWorldRayInteractor
{
	public bool ShowLine = false;

	private XRIPlatformManipulator _xrPlatformManipulator;
	private ObjectManipulatorMode _thisInteractorMode = ObjectManipulatorMode.ray;
	private LineRenderer _interactorLineRenderer;

	private const string _objectLayerName = "Default";
	private LayerMask _objectLayerMask;


	[SerializeField] private GameObject _reticleParentObj;
	[SerializeField] private GameObject _rayStabilizedAttach;
	[SerializeField] private Material _rayTargetLine;

	public ObjectManipulatorMode GetInteractorType()
	{
		return _thisInteractorMode;
	}

	public void Init(XRIPlatformManipulator xRIPlatformManipulator)
	{
		_xrPlatformManipulator = xRIPlatformManipulator;

		_xrPlatformManipulator.WhenLeftRayActiveChange += ActiveRayVisual;
		_xrPlatformManipulator.WhenRightRayActiveChange += ActiveRayVisual;
		_objectLayerMask = LayerMask.GetMask(_objectLayerName);

		interactionLayers &= ~_objectLayerMask;

		uiHoverEntered.AddListener(UIReticleVisiableOn);
		hoverEntered.AddListener(ReticleVisiable);

		if (_interactorLineRenderer == null)
		{
			if (TryGetComponent(out _interactorLineRenderer))
			{
				if (!ShowLine)
				{
					_interactorLineRenderer.materials = new Material[0];
				}
			}
		}
	}

	private void ActiveRayVisual(bool isActive)
	{
		_rayStabilizedAttach.SetActive(isActive);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		uiHoverEntered.RemoveListener(UIReticleVisiableOn);
		hoverEntered.RemoveListener(ReticleVisiable);
		_xrPlatformManipulator.WhenLeftRayActiveChange -= ActiveRayVisual;
		_xrPlatformManipulator.WhenRightRayActiveChange -= ActiveRayVisual;

	}

	private void ChangeLayerMask(bool isFeatureActive)
	{
		if (isFeatureActive)
			interactionLayers |= _objectLayerMask;
		else
			interactionLayers &= ~_objectLayerMask;
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		_rayStabilizedAttach.gameObject.SetActive(true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		_rayStabilizedAttach.gameObject.SetActive(false);
	}

	private void ReticleVisiable(HoverEnterEventArgs hoverEnterEventArgs)
	{
		_reticleParentObj.SetActive(true);

		var interactable = hoverEnterEventArgs.interactableObject;
		var grabTransformer = interactable.transform.GetComponent<XRCustomGrabTransformer>();

		if (!grabTransformer.IsRayModeOn())
			_reticleParentObj.SetActive(false);
	}

	private void UIReticleVisiableOn(UIHoverEventArgs hoverEventArgs)
	{
		_reticleParentObj.SetActive(true);
	}

	public void RayVisualOn()
	{
		_interactorLineRenderer.material = _rayTargetLine;
	}

	public void RayVisualOff()
	{
		_interactorLineRenderer.materials = new Material[0];
	}

}
