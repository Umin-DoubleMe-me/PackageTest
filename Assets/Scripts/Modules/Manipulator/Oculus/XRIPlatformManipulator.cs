using DoubleMe.Framework.Helper;
using DoubleMe.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace DoubleMe.Modules.Manipulator
{
	public class XRIPlatformManipulator : MonoBehaviour
	{
		public List<GameObject> PlatformObjectList { get; private set; }

		public Action<bool> WhenLeftRayActiveChange;
		public Action<bool> WhenRightRayActiveChange;

		[SerializeField] private XRInteractionGroup _interactionGroupLeft = null;
		[SerializeField] private XRInteractionGroup _interactionGroupRight = null;

		private IXRInteractor _leftRayInteractor;
		private IXRInteractor _leftDistanceInteractor;
		private IXRInteractor _leftGrabInteractor;

		private IXRInteractor _rightRayInteractor;
		private IXRInteractor _rightDistanceInteractor;
		private IXRInteractor _rightGrabInteractor;

		private ManipulatorMode _currentMode;
		private List<ManipulatorMode> _listPreMode;
		private AsyncLock _changeModeLock = new AsyncLock();

		private bool _isAvailableRay = true;
		private bool _isAvailableDistanceGrab = false;
		private bool _isAvailableHandGrab = true;

		private bool _isLeftRayCanActive = true;
		private bool _isRightRayCanActive = true;

		public virtual bool IsAvailableRay => _isAvailableRay;
		public virtual bool IsAvailableDistanceGrab => _isAvailableDistanceGrab;
		public virtual bool IsAvailableHandGrab => _isAvailableHandGrab;

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
			PlatformObjectList = new List<GameObject>();

			IXRInteractorType[] xRInteractorTypes = GetComponentsInChildren<IXRInteractorType>();
			foreach (var interactor in xRInteractorTypes)
				interactor.Init(this);
			
			ChangeMode(ManipulatorMode.SpaceNomalMode);
		}

		/// <summary>
		/// 해당 Manipulator와 상호작용 할 수 있도록 오브젝트 셋팅 로직
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual async Task InitializeManipulator(GameObject item)
		{
			if (item == null) return;

			try
			{
				if (TryGenerateGrabbable(item, out XRCustomInteractable createdGrabbale, out XRCustomGrabTransformer grabbable))
				{
					GenerateInteractable(item, createdGrabbale);
					ObjectManipulatorEventSetting(item, createdGrabbale);

					grabbable.Init();
					PlatformObjectList.Add(item);
					await ChangeSpaceItemMode(item, _currentMode);
				}
				await Task.Yield();
			}
			catch (Exception ex)
			{
				Debug.LogError($"[DoubleMe] [Exception] {ex.ToString()}");
			}
		}

		#region Generate Grabbable, Interactable
		protected virtual bool TryGenerateGrabbable(GameObject itemObject, out XRCustomInteractable interactable, out XRCustomGrabTransformer grabbable)
		{
			grabbable = Utils.GetOrAddComponent<XRCustomGrabTransformer>(itemObject);
			grabbable.allowTwoHandedScaling = true;
			grabbable.clampScaling = false;
			interactable = Utils.GetOrAddComponent<XRCustomInteractable>(itemObject);
			interactable.AddMultipleGrabTransformer(grabbable);
			interactable.AddSingleGrabTransformer(grabbable);
			interactable.useDynamicAttach = true;

			if (interactable != null) return true;
			else return false;
		}

		/// <summary>
		/// 오브젝트 입력에 필요한 컴포넌트 넣는 작업
		/// </summary>
		/// <param name="createdGrabbale"></param>
		/// <param name="targetObject"></param>
		/// <returns></returns>
		protected virtual void GenerateInteractable(GameObject targetObject, XRCustomInteractable createdGrabbale)
		{
			//셋팅 전 오브젝트 비활성화
			targetObject.SetActive(false);

			createdGrabbale.selectMode = InteractableSelectMode.Multiple;
			GenerateRigidBody(createdGrabbale);

			//셋팅 종료. 오브젝트 활성화
			targetObject.SetActive(true);
		}

		protected virtual void ObjectManipulatorEventSetting(GameObject item, XRCustomInteractable interactable)
		{
			if (interactable == null) return;

			ObjectManipulator result = GenerateObjectManipulator(item);
			XRCustomGrabTransformer transformer = result.GetComponent<XRCustomGrabTransformer>();

			interactable.firstHoverEntered.AddListener((events) => { GrabInteractableEventSetting(events, result); });
			interactable.hoverExited.AddListener((events) => { GrabInteractableEventSetting(events, result); });
			interactable.selectEntered.AddListener((events) => { GrabInteractableEventSetting(events, result); });
			interactable.selectExited.AddListener((events) => { GrabInteractableEventSetting(events, result); });
		}

		protected virtual void GrabInteractableEventSetting(BaseInteractionEventArgs events, ObjectManipulator oculusObjectManipulator)
		{
			if (oculusObjectManipulator != null)
				oculusObjectManipulator.OnWhenPointEventRaised(ConvertPointEventType(events));
		}

		private Rigidbody GenerateRigidBody(XRCustomInteractable grabbable)
		{
			Rigidbody rigidBody = Utils.GetOrAddComponent<Rigidbody>(grabbable.gameObject);
			rigidBody.isKinematic = true;
			rigidBody.useGravity = false;

			return rigidBody;
		}

		private ObjectManipulator GenerateObjectManipulator(GameObject item)
		{
			ObjectManipulator result = Utils.GetOrAddComponent<ObjectManipulator>(item);
			IXRGrabbable objectGrabbable = Utils.GetOrAddComponent<XRIObjectGrabbable>(item);
			result.Initialize(objectGrabbable);

			return result;
		}

		private PointEventType ConvertPointEventType(BaseInteractionEventArgs type)
		{
			switch (type)
			{
				case HoverEnterEventArgs: return PointEventType.Hover;
				case HoverExitEventArgs: return PointEventType.UnHover;
				case SelectEnterEventArgs: return PointEventType.Select;
				case SelectExitEventArgs: return PointEventType.UnSelect;
				default: Debug.LogError("[DoubleMe] Wrong Event Type"); return PointEventType.Cancel;
			}
		}

		#endregion

		#region Control Manipulator

		/// <summary>
		/// 설정했던 모드 종료(데이터 리스트에서 삭제)후 최근 모드 실행
		/// </summary>
		/// <param name="MinuMode"></param>
		public virtual async void ChangeModeReset(ManipulatorMode MinuMode)
		{
			using (await _changeModeLock.Lock())
			{
				if (MinuMode != ManipulatorMode.Default)
				{
					int indexNumber = _listPreMode.LastIndexOf(MinuMode);
					if (indexNumber > 0) _listPreMode.RemoveAt(indexNumber);
				}
			}
			//최근 모드 실행이라 데이터 리스트에 담을 필요 없음.
			ChangeMode(_listPreMode[_listPreMode.Count - 1], false);
		}


		/// <summary>
		/// 새로운 모드 실행 <br></br><br></br>
		/// mode - 실행하고자 하는 모드<br></br>
		/// modeCount - 해당 모드를 데이터 리스트에 담아야 하는가
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="modeCount"></param>
		public virtual async void ChangeMode(ManipulatorMode mode, bool modeCount = true)
		{
			using (await _changeModeLock.Lock())
			{
				if (modeCount) _listPreMode.Add(mode);
				_currentMode = mode;

				await ChangeModeRightHand(mode);
				await ChangeModeLeftHand(mode);

				SettingSpaceItems(mode);
			}
		}

		public virtual async Task ChangeModeRightHand(ManipulatorMode? mode)
		{
			if (_rightRayInteractor == null)
				_rightRayInteractor = GetInteractor<XRRayCustomInteractor>(true);
			if (_rightDistanceInteractor == null)
				_rightDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(true);

			await ChangeModeInteractors(_rightRayInteractor, _rightDistanceInteractor, _rightGrabInteractor, mode == null ? _currentMode : mode.Value);
		}

		public virtual async Task ChangeModeLeftHand(ManipulatorMode? mode)
		{
			if (_leftRayInteractor == null)
				_leftRayInteractor = GetInteractor<XRRayCustomInteractor>(false);
			if (_leftDistanceInteractor == null)
				_leftDistanceInteractor = GetInteractor<XRDistanceCustomInteractor>(false);

			await ChangeModeInteractors(_leftRayInteractor, _leftDistanceInteractor, _leftGrabInteractor, mode == null ? _currentMode : mode.Value);
		}

		public virtual async Task ChangeSpaceItemMode(GameObject item, ManipulatorMode mode)
		{
			if (item == null) return;
			if (!item.TryGetComponent(out XRCustomGrabTransformer xRGrabTransformer) || !item.TryGetComponent(out XRCustomInteractable xRGrabInteractable))
			{
				Debug.LogWarning($"Object {item.name} does not have XR Grab Component");
				return;
			}

			switch (mode)
			{
				case ManipulatorMode.SpaceNomalMode:
					xRGrabInteractable.enabled = true;
					xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.grab);
					break;

				case ManipulatorMode.SpaceUIMixMode:
					xRGrabInteractable.enabled = true;
					xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.ray, ObjectManipulatorMode.grab);
					break;

				case ManipulatorMode.SpaceUIMixMode_NotObjectRay:
					xRGrabInteractable.enabled = true;
					xRGrabTransformer.SetCurMode(ObjectManipulatorMode.distance, ObjectManipulatorMode.grab);
					break;
				case ManipulatorMode.SpaceUIMixMode_NotObjectDis:
					xRGrabInteractable.enabled = true;
					xRGrabTransformer.SetCurMode(ObjectManipulatorMode.ray, ObjectManipulatorMode.grab);
					break;

				case ManipulatorMode.SpaceUIMixMode_NotObjectAnything:
					xRGrabInteractable.enabled = false;
					break;

				case ManipulatorMode.OnlyUIMode:
					xRGrabInteractable.enabled = true;
					xRGrabTransformer.SetCurMode(ObjectManipulatorMode.ray);
					break;

				case ManipulatorMode.OnlyUIMode_NotObject:
					xRGrabInteractable.enabled = false;
					break;

				case ManipulatorMode.None:
				case ManipulatorMode.SpaceDrawLineMode:
					xRGrabInteractable.enabled = false;
					break;
			}
		}

		protected virtual async Task ChangeModeInteractors(IXRInteractor ray, IXRInteractor distance, IXRInteractor grab, ManipulatorMode mode)
		{
			switch (mode)
			{
				case ManipulatorMode.SpaceNomalMode: // In Space, Others (Not Ray , But Distance) 
					ToggleInteractor(ray, false);
					ToggleInteractor(distance, IsAvailableDistanceGrab);
					ToggleInteractor(grab, IsAvailableHandGrab);
					break;

				case ManipulatorMode.SpaceUIMixMode:
					ToggleInteractor(ray, IsAvailableRay);
					ToggleInteractor(distance, IsAvailableDistanceGrab);
					ToggleInteractor(grab, false);
					break;

				// In Space, UI / Others (Use Ray and Distance)
				case ManipulatorMode.SpaceUIMixMode_NotObjectRay:
				case ManipulatorMode.SpaceUIMixMode_NotObjectDis:
				case ManipulatorMode.SpaceUIMixMode_NotObjectAnything:
				case ManipulatorMode.SpaceDrawLineMode:
					ToggleInteractor(ray, IsAvailableRay);
					ToggleInteractor(distance, IsAvailableDistanceGrab);
					ToggleInteractor(grab, IsAvailableHandGrab);
					break;

				// Only Ray (Lobby Scene) (Not Distance , But Ray)
				case ManipulatorMode.OnlyUIMode:
					ToggleInteractor(ray, IsAvailableRay);
					ToggleInteractor(distance, false);
					ToggleInteractor(grab, IsAvailableHandGrab);
					break;

				case ManipulatorMode.OnlyUIMode_NotObject:
					ToggleInteractor(ray, IsAvailableRay);
					ToggleInteractor(distance, false);
					ToggleInteractor(grab, IsAvailableHandGrab);
					break;

				case ManipulatorMode.None: // Nothing (no distance, no ray)
					ToggleInteractor(ray, false);
					ToggleInteractor(distance, false);
					ToggleInteractor(grab, false);
					break;
			}
		}

		protected virtual void SettingSpaceItems(ManipulatorMode mode)
		{
			for(int index = 0; index < PlatformObjectList.Count; index++)
			{
				bool isObjectActive = PlatformObjectList[index] != null && PlatformObjectList[index].activeSelf;

				if (isObjectActive)
					ChangeSpaceItemMode(PlatformObjectList[index], mode);
			}
		}

		private IXRInteractor GetInteractor<T>(bool isRight) where T : IXRInteractor
		{
			IXRInteractor interactor = null;

			if (isRight)
			{
				interactor = _interactionGroupRight.startingGroupMembers.Find(o => o is T) as IXRInteractor;
			}
			else
			{
				interactor = _interactionGroupLeft.startingGroupMembers.Find(o => o is T) as IXRInteractor;
			}
			return interactor;
		}

		private void ToggleInteractor(IXRInteractor interactor, bool toggle)
		{
			if (interactor == null) return;
			MonoBehaviour targetInteractor = interactor as MonoBehaviour;

			if (targetInteractor.gameObject.activeSelf != toggle)
				targetInteractor.gameObject.SetActive(toggle);
		}

		#endregion
	}
}