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


	private void OnValidate()
	{
		if (PackageStart)
		{
			PackageStart = false;

			PackageControllerObj resultPackage = null;
			foreach (PackageControllerObj packageObj in PlatformPackage)
			{
				if (TargetPlatform == packageObj.Platform)
					resultPackage = packageObj;
				else
					packageObj.PackageDeactive();
			}

			resultPackage?.PackageInit();
		}
	}

}


#region Assembly �˻��ؼ� ��������
/*
 			IEnumerable<Type> implementations = FindImplementationsOfType<APackageController>();
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




	// Ư�� �������̽� Ÿ���� ã�� �� ����� �޼���
	public static IEnumerable<Type> FindImplementationsOfType<T>()
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