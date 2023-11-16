using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetPokeToFingerAttachPoint : MonoBehaviour
{
    public Transform PokeAttachPoint;

    private XRPokeInteractor _xrPokeInteractor; 

    // Start is called before the first frame update
    void Start()
    {
        _xrPokeInteractor = transform.GetComponentInChildren<XRPokeInteractor>();
		SetPokeAttachPoint();
    }

    private void SetPokeAttachPoint()
    {
        if (PokeAttachPoint == null) { Debug.LogError("Poke Attach point is null"); return; }
        if (_xrPokeInteractor == null) { Debug.LogError("XR Poke Interactor is null"); return; }

        _xrPokeInteractor.attachTransform = PokeAttachPoint;
    }
}
