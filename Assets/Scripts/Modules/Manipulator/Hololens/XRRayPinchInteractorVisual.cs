using UnityEngine;
using UnityEngine.InputSystem;

public class XRRayPinchInteractorVisual : MonoBehaviour
{
	XRIHand hand;
	[SerializeField] private InputActionProperty _triggerAction;

	[Header("Ray")]
	[SerializeField]
	private GameObject _lookTargetObj;
	[SerializeField]
	private SkinnedMeshRenderer _pinchSkinnedMesh;
	[SerializeField]
	Vector2 _alphaRange = new Vector2(.1f, .4f);

	[Header("Cursor")]
	[SerializeField]
	private Renderer _cursorRenderer;
	[SerializeField]
	private Color _outlineColor = Color.black;

	private int _shaderRadialGradientScale = Shader.PropertyToID("_RadialGradientScale");
	private int _shaderRadialGradientIntensity = Shader.PropertyToID("_RadialGradientIntensity");
	private int _shaderRadialGradientBackgroundOpacity = Shader.PropertyToID("_RadialGradientBackgroundOpacity");
	private int _shaderOutlineColor = Shader.PropertyToID("_OutlineColor");

	private float _pinchStrength
	{
		get
		{
			if (hand != null)
				return hand.PinchValue;
			else if (_triggerAction != null)
				return _triggerAction.action.ReadValue<float>();
			else return 0f;
		}
		set => _pinchStrength = value;
	}

	private bool _isPinch
	{
		get
		{
			if (hand != null)
				return hand.IsPinch;
			else if (_triggerAction != null)
				return _triggerAction.action.ReadValue<float>() == 1f;
			else return false;
		}
		set => _isPinch = value;
	}

	private void Awake()
	{
		hand = GetComponentInParent<XRIHand>();
		if (hand == null && _triggerAction == null) Debug.LogError("[XRRayPinchInteractorVisual] : There is no Hand");
	}

	private void Update()
	{
		UpdatePinchVisual();
		UpdateCursorVisual();
	}

	private void UpdatePinchVisual()
	{
		if (!_pinchSkinnedMesh.enabled) _pinchSkinnedMesh.enabled = true;

		if (_lookTargetObj.activeSelf)
			transform.LookAt(_lookTargetObj.transform);
		else
			transform.localRotation = Quaternion.identity;

		_pinchSkinnedMesh.material.color = _isPinch ? Color.white : new Color(1f, 1f, 1f, Mathf.Lerp(_alphaRange.x, _alphaRange.y, _pinchStrength));
		_pinchSkinnedMesh.SetBlendShapeWeight(0, _pinchStrength * 100f);
		_pinchSkinnedMesh.SetBlendShapeWeight(1, _pinchStrength * 100f);
	}

	private void UpdateCursorVisual()
	{
		var radialScale = 1f - _pinchStrength;
		radialScale = Mathf.Max(radialScale, .11f);
		_cursorRenderer.material.SetFloat(_shaderRadialGradientScale, radialScale);
		_cursorRenderer.material.SetFloat(_shaderRadialGradientIntensity, _pinchStrength);
		_cursorRenderer.material.SetFloat(_shaderRadialGradientBackgroundOpacity, Mathf.Lerp(0.3f, 0.7f, _pinchStrength));
		_cursorRenderer.material.SetColor(_shaderOutlineColor, _outlineColor);
	}
}