#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;


[CreateAssetMenu(fileName = "PackageController", menuName = "Scriptable Object/PackageController", order = int.MaxValue)]
public class PackageControllerObj : ScriptableObject, IPackageController
{
	public platform Platform;
	public List<string> DependenciesPackages;

	public string DefineString; // TODO : 직접 쓰지말고 Reference로 외부에 AssemblyDefinitionAsset의 Define Constraints 데이터 가져올것.

	private bool _isWorking = false;

	public async void PackageInit()
	{
		if (_isWorking) return;
		_isWorking = true;

		Debug.Log($"{name} : Init");

		ListRequest listRequest = Client.List();
		while (!listRequest.IsCompleted) { };

		if (listRequest.Status != StatusCode.Success)
		{
			Debug.LogError($"{this.GetType()} : Init Package 에러");
			_isWorking = false;
			return;
		}

		List<string> leftPackages = DependenciesPackages.ToList();

		foreach (PackageInfo installedPackage in listRequest.Result)
		{
			if (leftPackages.Count == 0)
				break;

			foreach (string dependencyPackage in leftPackages)
			{
				if (installedPackage.name == dependencyPackage)
				{
					leftPackages.Remove(installedPackage.name);
					break;
				}
			}
		}

		foreach (string package in leftPackages)
		{
			AddRequest addRequest = Client.Add(package);

			while (!addRequest.IsCompleted)
			{
				await Task.Yield();
			}

			if (addRequest.Status == StatusCode.Success)
				Debug.Log("Package installed successfully!");
			else if (addRequest.Status >= StatusCode.Failure)
			{
				Debug.LogError("Package installation failed!");
				_isWorking = false;
				return;
			}
		}
		DefineAdd();

		_isWorking = false;

		Debug.Log($"{name} : Init Done");

	}

	public async void PackageDeactive()
	{
		if (!_isWorking) return;
		_isWorking = true;

		Debug.Log($"{name} : Deactive");

		DefineRemove();

		ListRequest listRequest = Client.List();
		while (!listRequest.IsCompleted) { };

		if (listRequest.Status != StatusCode.Success)
		{
			Debug.LogError($"{this.GetType()} : Deactive Package 에러");
			_isWorking = false;
			return;
		}

		List<string> leftPackages = DependenciesPackages.ToList();

		foreach (PackageInfo installedPackage in listRequest.Result)
		{
			if (leftPackages.Count == 0)
				break;

			foreach (string dependencyPackage in leftPackages)
			{
				if (installedPackage.name == dependencyPackage)
				{
					leftPackages.Remove(installedPackage.name);
					RemoveRequest removeRequest = Client.Remove(installedPackage.name);

					while (!removeRequest.IsCompleted)
					{
						await Task.Yield();
					}

					if (removeRequest.Status == StatusCode.Success)
					{
						Debug.Log("Package removed successfully!");
						break;
					}
					else if (removeRequest.Status >= StatusCode.Failure)
					{
						Debug.LogError("Package removal failed!");
						_isWorking = false;
						return;
					}
				}
			}
		}

		_isWorking = false;

		Debug.Log($"{name} : Deactive Done");

	}

	private void DefineAdd()
	{
		string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		List<string> allDefines = definesString.Split(';').ToList();

		allDefines.Add(DefineString);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
	}

	private void DefineRemove()
	{
		string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
		List<string> allDefines = definesString.Split(';').ToList();

		allDefines.Remove(DefineString);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", allDefines.ToArray()));
	}
}
#endif