using DoubleMe.Modules.Manipulator;
using DoubleMe.Modules.Platform;
using System;
using System.Collections.Generic;
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
	[SerializeField] private List<GameObject> _platformList;
	
	public IPlatform Platform { get; private set; }

	public XRIPlatformManipulator PlatformManipulator { get; private set; }
	public IPlatformHand PlatformHand { get; private set; }

	public bool IsInitialized {get; private set; } = false;
	public Action<bool> OnInitialize;

	public void InitializeManager(Action<bool> result = null)
	{
		Platform = GeneratePlatform(result);
		GeneratePlatFormTypeManipulator();
	}

	private void OnDisable()
	{
		if (Platform != null)
		{
			Platform.OnResult -= OnPlatformInitializeResult;
			Platform = null;
			OnInitialize = null;
		}
	}

	private IPlatform GeneratePlatform(Action<bool> result = null)
	{
		IPlatform resultPlatform = null;

		switch (PlatFormType)
		{
			case PlatFormType.Oculus:
				resultPlatform = new OculusPlatform();
				break;
			case PlatFormType.Pico:
				break;
			case PlatFormType.Apple:
				break;
			case PlatFormType.Hololens:
				break;
		}

		if(resultPlatform == null)
		{
			Debug.LogError("[XRIManager] : There is no Platform");
			return null;
		}

		if (result != null)
			OnInitialize += result;

		resultPlatform.OnResult += OnPlatformInitializeResult;
		resultPlatform.Initialize();
		return resultPlatform;
	}

	public PlatformUserData GetPlatformUserData()
	{
		if (!IsInitialized || Platform == null) return null;
		return Platform.UserData;
	}

	private void OnPlatformInitializeResult(bool success)
	{
		IsInitialized = success;
		OnInitialize?.Invoke(IsInitialized);
	}

	private void GeneratePlatFormTypeManipulator()
	{
		int targetIndex = _platformList.FindIndex(platform => platform.name.Contains(PlatFormType.ToString()));
		GameObject platformObj = Instantiate(_platformList[targetIndex]);
		platformObj.transform.SetParent(transform);

		switch (PlatFormType)
		{
			case PlatFormType.Oculus:
			case PlatFormType.Hololens:
				PlatformManipulator = platformObj.GetComponentInChildren<XRIPlatformManipulator>();
				PlatformHand = platformObj.GetComponentInChildren<IPlatformHand>();
				break;
			case PlatFormType.Apple:
				break;
			case PlatFormType.Pico:
				break;
		}

		if(PlatformManipulator == null || PlatformHand == null)
		{
			Debug.LogError("[XRIManager] : There is no PlatformManipulator");
			return;
		}

		PlatformManipulator.InitData();
	}
}
