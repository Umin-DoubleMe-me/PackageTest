using Oculus.Interaction;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class OculusPlatformController : APlatfromController
{
	public override Task InitializeManipulator(GameObject targetObj)
	{
		return null;
	}

	protected override void GenerateEventSystem()
	{
		GameObject eventSystemObj = new GameObject("EventSystem");

		eventSystemObj.AddComponent<EventSystem>();
		eventSystemObj.AddComponent<PointableCanvasModule>();
	}
}

