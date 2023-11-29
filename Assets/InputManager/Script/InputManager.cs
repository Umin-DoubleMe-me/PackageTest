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


#region Assembly �˻��ؼ� ��������
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

			// Ư�� �������̽��� ���ǵ� �޼ҵ带 ȣ��
			MethodInfo method = type.GetMethod(methodName); // MyMethod�� ȣ���Ϸ��� �޼ҵ��

			// ���� �޼ҵ尡 �����Ѵٸ� ����
			method?.Invoke(instance, null);
		}
	}

	// Ư�� �������̽� Ÿ���� ã�� �� ����� �޼���
	private static IEnumerable<Type> FindImplementationsOfType<T>()
	{
		Type interfaceType = typeof(T);

		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		List<Type> implementations = new List<Type>();

		foreach (var assembly in assemblies)
		{
			// ����� ���� ��� Ÿ�Ե��� ������
			Type[] types = assembly.GetTypes();

			foreach (var type in types)
			{
				// Ư�� �������̽��� ������ Ŭ�������� Ȯ��
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