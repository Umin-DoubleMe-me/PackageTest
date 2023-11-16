using DoubleMe.Modules.Manipulator;
using DoubleMe.Modules.Platform;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public enum PointEventType
{
	Hover,
	UnHover,
	Select,
	UnSelect,
	Move,
	Cancel,
}
public enum PlatFormType
{
	Oculus = 1,
	Apple = 2,
	Pico = 3,
	Hololens = 4,
}

public class PlatformUserData
{
	public string? UserID = string.Empty;
	public string? UserNickName = string.Empty;
	public string? Nonce = string.Empty;
	public string? ServiceName = string.Empty;
	public string? Email = string.Empty;
	public string? Password = string.Empty;
	public List<PlatformUserData> UserFriends = new List<PlatformUserData>();
}

public interface IPlatform
{
	public PlatformUserData UserData { get; set; }
	public Action<bool> OnResult { get; set; }
	public Action<bool> OnNonce { get; set; }
	void Initialize();
	void GetPlatformLoginUser();
	void GetPlatformFriends();
	void GetPlatformUserNonce();
}

public class XRIManager : MonoBehaviour
{
	[SerializeField] public PlatFormType PlatFormType;
	private bool _isInitialized = false;
	private IPlatform _platform;
	public IPlatform Platform => _platform;
	public bool IsInitialized => _isInitialized;
	public Action<bool> OnInitialize;

	public void InitializeManager(Action<bool> result = null)
	{
		switch (PlatFormType)
		{
			case PlatFormType.Oculus:
				_platform = new OculusPlatform();
				break;
			case PlatFormType.Pico:
				break;
			case PlatFormType.Apple:
				break;
		}

		if (_platform != null)
		{
			if (result != null)
			{
				OnInitialize += result;
			}

			_platform.OnResult += OnPlatformInitializeResult;
			_platform.Initialize();
		}
	}

	public PlatformUserData GetPlatformUserData()
	{
		if (!_isInitialized || _platform == null) return null;
		return _platform.UserData;
	}

	private void OnPlatformInitializeResult(bool success)
	{
		_isInitialized = success;
		OnInitialize?.Invoke(_isInitialized);
	}

	private void OnDisable()
	{
		if (_platform != null)
		{
			_platform.OnResult -= OnPlatformInitializeResult;
			_platform = null;
			OnInitialize = null;
		}
	}

	[SerializeField] private List<GameObject> _platformList;

	public XRIPlatformManipulator PlatformManipulator { get; private set; }
	public IPlatformHand PlatformHand { get; private set; }

	public void Init()
	{
		GeneratePlatFormTypeManipulator();
		PlatformManipulator.InitData();
	}

	private void GeneratePlatFormTypeManipulator()
	{
		//int targetIndex = _platformList.FindIndex(platform => platform.name.Contains(Managers.PlatformManager.PlatFormType.ToString()));
		//GameObject platformObj = Instantiate(_platformList[targetIndex]);
		//platformObj.transform.SetParent(transform);

		//switch (Managers.PlatformManager.PlatFormType)
		//{
		//	case PlatFormType.Default:
		//		break;
		//	case PlatFormType.Unity:
		//		break;
		//	case PlatFormType.Oculus:
		//	case PlatFormType.OculusXR:
		//	case PlatFormType.Hololens:
		//		PlatformManipulator = platformObj.GetComponentInChildren<IPlatformManipulator>();
		//		PlatformHand = platformObj.GetComponentInChildren<IPlatformHand>();
		//		break;
		//}
	}
}
