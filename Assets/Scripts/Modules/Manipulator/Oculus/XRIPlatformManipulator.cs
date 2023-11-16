using DoubleMe.Framework.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;

namespace DoubleMe.Modules.Manipulator
{
	public interface ILipSyncManipulator
	{
		public float[] Blendshapes { get; }
	}

	public class XRIPlatformManipulator : MonoBehaviour
	{
		[SerializeField] private XRInteractionGroup _interactionGroupLeft = null;
		[SerializeField] private XRInteractionGroup _interactionGroupRight = null;

		private IXRInteractor _leftRayInteractor;
		private IXRInteractor _leftDistanceInteractor;
		private IXRInteractor _leftGrabInteractor;

		private IXRInteractor _rightRayInteractor;
		private IXRInteractor _rightDistanceInteractor;
		private IXRInteractor _rightGrabInteractor;

		private ILipSyncManipulator _lipSyncContext = null;

		private ManipulatorMode _currentMode;
		private List<ManipulatorMode> _listPreMode;
		private AsyncLock _changeModeLock = new AsyncLock();

		private const string ObjectLayerName = "Default";
		private const string UILayerName = "UI";
		private int _objectLayerNum = -1;
		private int _uiLayerNum = -1;

		private bool _isAvailableDistanceGrab = false;
		public bool IsAvailableDistanceGrab => _isAvailableDistanceGrab;

		private bool _isAvailableHandGrab = true;
		public bool IsAvailableHandGrab => _isAvailableHandGrab;

		public virtual float[] Blendshapes => _lipSyncContext?.Blendshapes;

		private bool _isLeftRayCanActive = true;
		private bool _isRightRayCanActive = true;
		public Action<bool> WhenLeftRayActiveChange;
		public Action<bool> WhenRightRayActiveChange;

		public bool IsLeftRayCanActive
		{
			get => _isLeftRayCanActive;
			set
			{
				if(value != _isLeftRayCanActive)
					WhenLeftRayActiveChange?.Invoke(value);
				_isLeftRayCanActive = value;

			}
		}

		public bool IsRightRayCanActive
		{
			get => _isRightRayCanActive;
			set
			{
				if(value != _isRightRayCanActive)
					WhenRightRayActiveChange?.Invoke(value);
				_isRightRayCanActive = value;
			}
		}


		public void InitData()
		{
			_listPreMode = new List<ManipulatorMode>();

			_objectLayerNum = 1 << LayerMask.NameToLayer(ObjectLayerName);
			_uiLayerNum = 1 << LayerMask.NameToLayer(UILayerName);
			_lipSyncContext = GetComponentInChildren<ILipSyncManipulator>();

			//AvailableInteraction();
			IXRInteractorType[] xRInteractorTypes = GetComponentsInChildren<IXRInteractorType>();
			foreach (var interactor in xRInteractorTypes)
				interactor.Init(this);

			//ChangeMode(ManipulatorMode.OnlyUIMode);
		}

		//public async Task InitializeManipulator(Item item)
		//{
		//	try
		//	{
		//		GameObject rootObject = item.Root;
		//		if (rootObject == null) return;

		//		if (TryGenerateGrabbable(rootObject, out XRCustomInteractable createdGrabbale, out XRCustomGrabTransformer grabbable))
		//		{
		//			GenerateInteractable(createdGrabbale, rootObject);
		//			ObjectManipulatorEventSetting(item, item.BoundingBox, createdGrabbale);

		//			grabbable.Init();
		//			await ChangeSpaceItemMode(item, _currentMode);
		//		}
		//		await Task.Yield();
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogError($"[DoubleMe] [Exception] {ex.ToString()}");
		//	}
		//}

		//#region Generate Grabbable, Interactable
		//private bool TryGenerateGrabbable(GameObject itemObject, out XRCustomInteractable interactable, out XRCustomGrabTransformer grabbable)
		//{
		//	grabbable = Utils.GetOrAddComponent<XRCustomGrabTransformer>(itemObject);
		//	grabbable.allowTwoHandedScaling = true;
		//	grabbable.clampScaling = false;
		//	interactable = Utils.GetOrAddComponent<XRCustomInteractable>(itemObject);
		//	interactable.AddMultipleGrabTransformer(grabbable);
		//	interactable.AddSingleGrabTransformer(grabbable);
		//	interactable.useDynamicAttach = true;

		//	if (interactable != null) return true;
		//	else return false;
		//}

		///// <summary>
		///// 오브젝트 입력에 필요한 컴포넌트 넣는 작업
		///// </summary>
		///// <param name="createdGrabbale"></param>
		///// <param name="rootObject"></param>
		///// <returns></returns>
		//private void GenerateInteractable(XRCustomInteractable createdGrabbale, GameObject rootObject)
		//{
		//	//셋팅 전 오브젝트 비활성화
		//	rootObject.SetActive(false);

		//	createdGrabbale.selectMode = InteractableSelectMode.Multiple;
		//	GenerateRigidBody(createdGrabbale);

		//	//셋팅 종료. 오브젝트 활성화
		//	rootObject.SetActive(true);
		//}

		//private Rigidbody GenerateRigidBody(XRCustomInteractable grabbable)
		//{
		//	Rigidbody rigidBody = Utils.GetOrAddComponent<Rigidbody>(grabbable.gameObject);
		//	rigidBody.isKinematic = true;
		//	rigidBody.useGravity = false;

		//	return rigidBody;
		//}

		//private void ObjectManipulatorEventSetting(Item item, GameObject wireObject, XRCustomInteractable interactable)
		//{
		//	if (interactable == null) return;

		//	ObjectManipulator result = GenerateObjectManipulator(item, wireObject);
		//	XRCustomGrabTransformer transformer = result.GetComponent<XRCustomGrabTransformer>();
		//	ObjectSelectFeature objectSelectFeature = Managers.ObjectManipulatorManager.ObjectSelectFeature;
		//	AnchorObjectSelectFeature anchorObjectSelectFeature = Managers.ObjectManipulatorManager.AnchorObjectSelectFeature;

		//	interactable.firstHoverEntered.AddListener((events) => { GrabInteractableEventSetting(events, result, objectSelectFeature, anchorObjectSelectFeature); });
		//	interactable.hoverExited.AddListener((events) => { GrabInteractableEventSetting(events, result, objectSelectFeature, anchorObjectSelectFeature); });
		//	interactable.selectEntered.AddListener((events) => { GrabInteractableEventSetting(events, result, objectSelectFeature, anchorObjectSelectFeature); });
		//	interactable.selectExited.AddListener((events) => { GrabInteractableEventSetting(events, result, objectSelectFeature, anchorObjectSelectFeature); });

		//	result.WhenSetSelectTargetManipulator += (selected) =>
		//	{
		//		if (IsAvailableDistanceGrab)
		//		{
		//			if (selected)
		//			{
		//				transformer.AddMode(ObjectManipulatorMode.ray);
		//				transformer.MinusMode(ObjectManipulatorMode.distance);
		//			}
		//			else
		//			{
		//				transformer.AddMode(ObjectManipulatorMode.distance);
		//				transformer.MinusMode(ObjectManipulatorMode.ray);
		//			}
		//		}

		//		if (IsAvailableHandGrab)
		//		{
		//			if (selected)
		//			{
		//				transformer.AddMode(ObjectManipulatorMode.grab);
		//				transformer.MinusMode(ObjectManipulatorMode.ray);
		//			}
		//			else
		//			{
		//				transformer.AddMode(ObjectManipulatorMode.ray);
		//				transformer.MinusMode(ObjectManipulatorMode.grab);
		//			}
		//		}
		//	};

		//}

		//private ObjectManipulator GenerateObjectManipulator(Item item, GameObject wireObject)
		//{
		//	ObjectManipulator result = Utils.GetOrAddComponent<ObjectManipulator>(item.Root);
		//	IXRGrabbable objectGrabbable = Utils.GetOrAddComponent<XRIObjectGrabbable>(item.Root);
		//	result.Initialize(item, wireObject, objectGrabbable);

		//	return result;
		//}

		//private void GrabInteractableEventSetting(BaseInteractionEventArgs events, ObjectManipulator oculusObjectManipulator, ObjectSelectFeature objectSelectFeature, AnchorObjectSelectFeature anchorObjectSelectFeature)
		//{
		//	bool isSelectFeature = objectSelectFeature.IsSelectedGroupingActivate || anchorObjectSelectFeature.IsSelectedAnchorObjectActivate;
		//	if (oculusObjectManipulator != null)
		//		oculusObjectManipulator.OnWhenPointEventRaised(isSelectFeature, ConvertPointEventType(events));
		//}

		//private PointEventType ConvertPointEventType(BaseInteractionEventArgs type)
		//{
		//	switch (type)
		//	{
		//		case HoverEnterEventArgs: return PointEventType.Hover;
		//		case HoverExitEventArgs: return PointEventType.UnHover;
		//		case SelectEnterEventArgs: return PointEventType.Select;
		//		case SelectExitEventArgs: return PointEventType.UnSelect;
		//		default: Debug.LogError("[DoubleMe] Wrong Event Type"); return PointEventType.Cancel;
		//	}
		//}

		//#endregion

		//#region Control Manipulator

		///// <summary>
		///// 설정했던 모드 종료(데이터 리스트에서 삭제)후 최근 모드 실행
		///// </summary>
		///// <param name="MinuMode"></param>
		//public async void ChangeModeReset(ManipulatorMode MinuMode)
		//{
		//	using (await _changeModeLock.Lock())
		//	{
		//		if (MinuMode != ManipulatorMode.Default)
		//		{
		//			int indexNumber = _listPreMode.LastIndexOf(MinuMode);
		//			if (indexNumber > 0) _listPreMode.RemoveAt(indexNumber);
		//		}
		//	}
		//	//최근 모드 실행이라 데이터 리스트에 담을 필요 없음.
		//	ChangeMode(_listPreMode[_listPreMode.Count - 1], false);
		//}


		///// <summary>
		///// 새로운 모드 실행 <br></br><br></br>
		///// mode - 실행하고자 하는 모드<br></br>
		///// modeCount - 해당 모드를 데이터 리스트에 담아야 하는가
		///// </summary>
		///// <param name="mode"></param>
		///// <param name="modeCount"></param>
		//public async void ChangeMode(ManipulatorMode mode, bool modeCount = true)
		//{
		//	using (await _changeModeLock.Lock())
		//	{
		//		if (_objectLayerNum <= 0 || _uiLayerNum <= 0)
		//		{
		//			Debug.LogError("Can't Find Target LayerMask");
		//			return;
		//		}

		//		if (modeCount) _listPreMode.Add(mode);
		//		_currentMode = mode;

		//		await ChangeModeRightHand(mode);
		//		await ChangeModeLeftHand(mode);

		//		SettingSpaceItems(mode);
		//	}
		//}

		//public async Task ChangeModeRightHand(ManipulatorMode? mode)
		//{
		//	if (_rightRayInteractor == null)
		//		_rightRayInteractor = GetInteractor<XRRayCustomInteractor>(true);
		//	if (_rightDistanceInteractor == null)
		//		_rightDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(true);
		//	//Grab 우선순위가 가장 높음
		//	//if (_rightGrabInteractor == null)
		//	//	_rightGrabInteractor = GetInteractor<XRGrabCustomInteractor>(true);

		//	await ChangeModeInteractors(_rightRayInteractor, _rightDistanceInteractor, _rightGrabInteractor, mode == null ? _currentMode : mode.Value);
		//}

		//public async Task ChangeModeLeftHand(ManipulatorMode? mode)
		//{
		//	if (_leftRayInteractor == null)
		//		_leftRayInteractor = GetInteractor<XRRayCustomInteractor>(false);
		//	if (_leftDistanceInteractor == null)
		//		_leftDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(false);
		//	//Grab 우선순위가 가장 높음
		//	//if (_leftGrabInteractor == null)
		//	//	_leftGrabInteractor = GetInteractor<XRGrabCustomInteractor>(false);

		//	await ChangeModeInteractors(_leftRayInteractor, _leftDistanceInteractor, _leftGrabInteractor, mode == null ? _currentMode : mode.Value);
		//}

		//private async Task ChangeModeInteractors(IXRInteractor ray, IXRInteractor distance, IXRInteractor grab, ManipulatorMode mode)
		//{
		//	switch (mode)
		//	{
		//		case ManipulatorMode.SpaceNomalMode: // In Space, Others (Not Ray , But Distance) 
		//			ToggleInteractor(ray, false);
		//			ToggleInteractor(distance, IsAvailableDistanceGrab);
		//			ToggleInteractor(grab, IsAvailableHandGrab);
		//			break;

		//		case ManipulatorMode.SpaceUIMixMode:
		//			ToggleInteractor(ray, true);
		//			ToggleInteractor(distance, true);
		//			ToggleInteractor(grab, false);
		//			break;

		//		// In Space, UI / Others (Use Ray and Distance)
		//		case ManipulatorMode.SpaceUIMixMode_NotObjectRay:
		//		case ManipulatorMode.SpaceUIMixMode_NotObjectDis:
		//		case ManipulatorMode.SpaceUIMixMode_NotObjectAnything:
		//		case ManipulatorMode.SpaceDrawLineMode:
		//			ToggleInteractor(ray, true);
		//			ToggleInteractor(distance, IsAvailableDistanceGrab);
		//			ToggleInteractor(grab, IsAvailableHandGrab);
		//			break;

		//		// Only Ray (Lobby Scene) (Not Distance , But Ray)
		//		case ManipulatorMode.OnlyUIMode:

		//			//로비라면 초기화
		//			if (IsLobbyScene())
		//				_listPreMode.Clear();

		//			//Debug.Log($"Callit {ray.transform.gameObject.name}");
		//			ToggleInteractor(ray, true);
		//			ToggleInteractor(distance, false);
		//			ToggleInteractor(grab, true);
		//			break;

		//		case ManipulatorMode.OnlyUIMode_NotObject:
		//			ToggleInteractor(ray, true);
		//			ToggleInteractor(distance, false);
		//			ToggleInteractor(grab, true);
		//			break;

		//		case ManipulatorMode.None: // Nothing (no distance, no ray)
		//			ToggleInteractor(ray, false);
		//			ToggleInteractor(distance, false);
		//			ToggleInteractor(grab, false);
		//			break;
		//	}
		//}

		//private bool IsLobbyScene()
		//{
		//	if (Managers.SceneManager == null) 
		//		return false;

		//	else if (Managers.SceneManager.GetCurrentScene() == null) 
		//		return false;

		//	else if (Managers.SceneManager.GetCurrentScene().SceneType == Define.SceneType.Lobby) 
		//		return true;

		//	else 
		//		return false;
		//}

		//private void SettingSpaceItems(ManipulatorMode mode)
		//{
		//	if (Managers.SpawnManager == null) return;

		//	foreach (var item in Managers.SpawnManager.SpaceItems)
		//	{
		//		bool isObjectActive = item.BaseObject != null && item.BaseObject.activeSelf;

		//		if (isObjectActive)
		//			ChangeSpaceItemMode(item, mode);
		//	}
		//}

		//public async Task ChangeSpaceItemMode(Item item, ManipulatorMode mode)
		//{
		//	if (item == null) return;
		//	if (item.Root == null) return;

		//	XRCustomGrabTransformer xRGrabTransformer;
		//	XRCustomInteractable xRGrabInteractable;
		//	if (!item.Root.TryGetComponent(out xRGrabTransformer) || !item.Root.TryGetComponent(out xRGrabInteractable))
		//	{
		//		Debug.LogWarning($"Object {item.BaseObject.name} does not have XR Grab Component");
		//		return;
		//	}

		//	switch (mode)
		//	{
		//		case ManipulatorMode.SpaceNomalMode:
		//			xRGrabInteractable.enabled = true;
		//			xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.grab);
		//			break;

		//		case ManipulatorMode.SpaceUIMixMode:
		//			xRGrabInteractable.enabled = true;
		//			xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.ray, ObjectManipulatorMode.grab);
		//			break;

		//		case ManipulatorMode.SpaceUIMixMode_NotObjectRay:
		//			xRGrabInteractable.enabled = true;
		//			xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.grab);
		//			break;
		//		case ManipulatorMode.SpaceUIMixMode_NotObjectDis:
		//			xRGrabInteractable.enabled = true;
		//			xRGrabTransformer.SetCurMode(ObjectManipulatorMode.ray, ObjectManipulatorMode.grab);
		//			break;

		//		case ManipulatorMode.SpaceUIMixMode_NotObjectAnything:
		//			xRGrabInteractable.enabled = false;
		//			break;

		//		case ManipulatorMode.OnlyUIMode:
		//			xRGrabInteractable.enabled = true;
		//			xRGrabTransformer.SetCurMode(ObjectManipulatorMode.ray);
		//			break;

		//		case ManipulatorMode.OnlyUIMode_NotObject:
		//			xRGrabInteractable.enabled = false;
		//			break;

		//		case ManipulatorMode.None:
		//		case ManipulatorMode.SpaceDrawLineMode:
		//			xRGrabInteractable.enabled = false;
		//			break;
		//	}
		//}

		//private void AvailableInteraction()
		//{
		//	InteractionType interactionType = Managers.Data != null ? (InteractionType)Managers.Data.InteractionType : InteractionType.Both;

		//	_isAvailableDistanceGrab = (interactionType == InteractionType.Both || interactionType == InteractionType.DistanceOnly);
		//	_isAvailableHandGrab = (interactionType == InteractionType.Both || interactionType == InteractionType.GrabOnly);
		//}

		//public void SetDefaultInteractor(InteractionType interactionType)
		//{
		//	IXRInteractor leftDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(false);
		//	IXRInteractor rightDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(true);

		//	IXRInteractor leftHandInteractor = GetInteractor<XRGrabCustomInteractor>(false);
		//	IXRInteractor rightHandInteractor = GetInteractor<XRGrabCustomInteractor>(true);

		//	if (interactionType == InteractionType.DistanceOnly)
		//	{
		//		ToggleInteractor(leftHandInteractor, false);
		//		ToggleInteractor(rightHandInteractor, false);
		//	}
		//	else if (interactionType == InteractionType.GrabOnly)
		//	{
		//		ToggleInteractor(leftDistanceInteractor, false);
		//		ToggleInteractor(rightDistanceInteractor, false);
		//	}
		//	else if (interactionType == InteractionType.Both)
		//	{

		//	}
		//}

		//private IXRInteractor GetInteractor<T>(bool isRight) where T : IXRInteractor
		//{
		//	IXRInteractor interactor = null;

		//	if (isRight)
		//	{
		//		interactor = _interactionGroupRight.startingGroupMembers.Find(o => o is T) as IXRInteractor;
		//	}
		//	else
		//	{
		//		interactor = _interactionGroupLeft.startingGroupMembers.Find(o => o is T) as IXRInteractor;
		//	}
		//	return interactor;
		//}

		//private void ToggleInteractor(IXRInteractor interactor, bool toggle)
		//{
		//	if (interactor == null) return;
		//	MonoBehaviour targetInteractor = interactor as MonoBehaviour;

		//	if(targetInteractor.gameObject.activeSelf != toggle)
		//		targetInteractor.gameObject.SetActive(toggle);
		//}

		//#endregion
	}
}

[Serializable]
public class HandTrackingEventRegister
{
	[SerializeField] private XRHandTrackingEvents _targetHandTrackingEvent;
	[SerializeField] private GameObject _interactorObj;
	[SerializeField] private LineRenderer _rayLine;

	public void EventRegist()
	{
		_rayLine.enabled = false;

		_targetHandTrackingEvent.trackingLost.AddListener(() => _interactorObj.SetActive(false));
		_targetHandTrackingEvent.trackingAcquired.AddListener(() => _interactorObj.SetActive(true));
	}
}
