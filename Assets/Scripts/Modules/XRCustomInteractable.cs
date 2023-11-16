using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRCustomInteractable : XRGrabInteractable
{
	protected override void OnHoverEntered(HoverEnterEventArgs args)
	{
		base.OnHoverEntered(args);
	}

	protected override void OnHoverExited(HoverExitEventArgs args)
	{
		base.OnHoverExited(args);
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		base.OnSelectEntering(args);
	}

	protected override void OnSelectExited(SelectExitEventArgs args)
	{
		base.OnSelectExited(args);
	}

	protected override void Grab()
	{
		Transform nowParent = transform.parent;

		base.Grab();

		transform.SetParent(nowParent);
	}
}
