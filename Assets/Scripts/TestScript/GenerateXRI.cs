using UnityEngine;

public class GenerateXRI : MonoBehaviour
{
	[SerializeField]
	public XRIManager XriManager;

	public void Start()
	{
		XriManager.InitializeManager(null);
	}
}
