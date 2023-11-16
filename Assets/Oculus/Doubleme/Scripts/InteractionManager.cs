using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get => _instance; }
    public static InteractionManager _instance;

    [SerializeField] private InteractorGroup _interactorGroupLeft = null;
    [SerializeField] private InteractorGroup _interactorGroupRight = null;
    public IInteractor LeftInteractor => _interactorGroupLeft?.BestInteractor;
    public IInteractor RightInteractor => _interactorGroupRight?.BestInteractor;

    public void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);
    }
}
