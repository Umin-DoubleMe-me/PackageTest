using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectTestScript : MonoBehaviour
{
    [SerializeField] XRDirectInteractor xrDirectionInteractor;
	[SerializeField] ActionBasedController controller;
    [SerializeField] Button btn;

	private void Start()
	{
        //RegisButton();
	}

	// Update is called once per frame
	void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
		{
			xrDirectionInteractor.selectEntered.AddListener((_) =>
			{
				Debug.Log("Umin Interactor Name : " + _.interactorObject.transform.name);
				Debug.Log("Umin Interactable Name : " + _.interactableObject.transform.name);

			});

			xrDirectionInteractor.selectExited.AddListener((_) =>
			{
				Debug.Log("Umin Interactor exit Name : " + _.interactorObject.transform.name);
				Debug.Log("Umin Interactable exit Name : " + _.interactableObject.transform.name);

			});


			xrDirectionInteractor.hoverEntered.AddListener((_) =>
			{
				Debug.Log("Umin Interactor Name : " + _.interactorObject.transform.name);
				Debug.Log("Umin Interactable Name : " + _.interactableObject.transform.name);

			});
		}
    }

    private void RegisButton()
    {
        btn.onClick.AddListener(() => Debug.Log("Click Button"));
    }
}
