using UnityEngine;
using UnityEngine.UI;

public class GenerateXRI : MonoBehaviour
{
	[SerializeField]
	public XRIManager XriManager;

	[SerializeField]
	public Button GenObjButton;

	public void Start()
	{
		XriManager.InitializeManager(null);
		GenObjButton.onClick.AddListener(GenerateObject);
	}

	public void GenerateObject()
	{
		GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		newObj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.1f;
		newObj.transform.rotation = Quaternion.identity;
		newObj.transform.localScale = Vector3.one * 0.1f;
		XriManager.PlatformManipulator.InitializeManipulator(newObj);
	}
}
