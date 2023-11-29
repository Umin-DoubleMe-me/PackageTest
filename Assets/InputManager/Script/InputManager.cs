using System.Collections.Generic;
using UnityEngine;

public enum platform
{
	AppleInputController,
	OculusInputController,
	pico
}

public class InputManager : MonoBehaviour
{
	public bool PackageStart = false;

	public platform TargetPlatform;
	public List<PackageControllerObj> PlatformPackage;

	private PackageControllerObj _targetPackageController = null;

	private IPlatformController _targetPlatformController;
	public IPlatformController TargetPlatformController
	{
		get
		{
			if (_targetPlatformController == null)
			{
				Debug.LogError("There is no targetPlatformController");
				return null;
			}
			return _targetPlatformController;
		}
	}

	private void Start()
	{
		GenerateInputObj();
	}

	private void OnValidate()
	{
		PackageSetting();
	}

	private void PackageSetting()
	{
#if UNITY_EDITOR
		if (PackageStart)
		{
			Debug.Log("PackageSetting Start");
			PackageStart = false;

			PackageControllerObj resultPackage = null;
			foreach (PackageControllerObj packageObj in PlatformPackage)
			{
				if (TargetPlatform == packageObj.Platform)
				{
					resultPackage = packageObj;
					break;
				}
			}

			resultPackage?.PackageInit();


			Debug.Log("PackageSetting Done");
		}
#endif
	}

	private void GenerateInputObj()
	{
		foreach (PackageControllerObj packageObj in PlatformPackage)
		{
			if (TargetPlatform == packageObj.Platform)
			{
				_targetPackageController = packageObj;
				break;
			}
		}

		if (_targetPackageController.PackageInputObj == null)
		{
			Debug.LogError("There is no PackageInputObj");
			return;
		}

		GameObject resultInputObj = _targetPackageController.GenerateInputObj(this.transform);
		if(!resultInputObj.TryGetComponent(out _targetPlatformController))
		{
			Debug.LogError("There is no PlatformController");
			return;
		}
	}


}


#region Assembly 검색해서 가져오기
/*


	private void GetPlatformPackage()
	{
		IEnumerable<Type> implementations = FindImplementationsOfType<PackageControllerObj>();
		foreach (Type type in implementations)
		{
			object instance = Activator.CreateInstance(type);
			string methodName = "";

			if (type.Name == TargetPlatform.ToString())
				methodName = "PackageInit";
			else
				methodName = "PackageDeactive";

			// 특정 인터페이스에 정의된 메소드를 호출
			MethodInfo method = type.GetMethod(methodName); // MyMethod는 호출하려는 메소드명

			// 만약 메소드가 존재한다면 실행
			method?.Invoke(instance, null);
		}
	}

	// 특정 인터페이스 타입을 찾을 때 사용할 메서드
	private static IEnumerable<Type> FindImplementationsOfType<T>()
	{
		Type interfaceType = typeof(T);

		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		List<Type> implementations = new List<Type>();

		foreach (var assembly in assemblies)
		{
			// 어셈블리 내의 모든 타입들을 가져옴
			Type[] types = assembly.GetTypes();

			foreach (var type in types)
			{
				// 특정 인터페이스를 구현한 클래스인지 확인
				if (interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
				{
					implementations.Add(type);
				}
			}
		}

		return implementations;
	}


 
 */
#endregion