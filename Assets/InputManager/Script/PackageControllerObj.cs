#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "PackageController", menuName = "Scriptable Object/PackageController", order = int.MaxValue)]
public class PackageControllerObj : ScriptableObject
{
	public platform Platform;
	public List<string> DependenciesPackages;

	public string DefineString; // TODO : 직접 쓰지말고 Reference로 외부에 AssemblyDefinitionAsset의 Define Constraints 데이터 가져올것.
	public GameObject PackageInputObj;

	private bool _isWorking = false;
	
	public void PackageInit()
	{
		if (_isWorking) return;
		_isWorking = true;

		Debug.Log($"{name} : Init");

		DefineAdd();

		_isWorking = false;

		Debug.Log($"{name} : Init Done");

	}

	public void PackageDeactive()
	{
		if (_isWorking) return;
		_isWorking = true;

		Debug.Log($"{name} : Deactive");

		DefineRemove();

		_isWorking = false;

		Debug.Log($"{name} : Deactive Done");

	}

	public GameObject GenerateInputObj(Transform parent)
	{
		GameObject resultObj = Instantiate(PackageInputObj, parent, true);

		return resultObj;
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