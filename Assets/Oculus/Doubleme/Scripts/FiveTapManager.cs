using Oculus.Interaction;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FiveTapManager : MonoBehaviour, ISelector
{
    [SerializeField, Interface(typeof(IActiveState))]
    private MonoBehaviour _activeState;

    protected IActiveState ActiveState { get; private set; }

    private bool _selecting = false;
    private int _tapCount = 0;
    private float _time;
    private bool _isTimerStarted;

    public bool IsTimerStarted
    { 
        get
        { 
            return _isTimerStarted;
        }
        set
        {
            _time = 0;
            _isTimerStarted = value;
        }   
    }

    public TextMeshProUGUI textMesh;
    public float tapInterval;
    public event Action WhenSelected = delegate { };
    public event Action WhenUnselected = delegate { };

    [SerializeField]
    private UnityEvent _oneTapEvent;
    [SerializeField]
    private UnityEvent _twoTapEvent;

    public UnityEvent OneTapEvent => _oneTapEvent;
    public UnityEvent TwoTapEvent => _twoTapEvent;

    protected virtual void Awake()
    {
        ActiveState = _activeState as IActiveState;
    }

    protected virtual void Start()
    {
        this.AssertField(ActiveState, nameof(ActiveState));
        WhenSelected += InsertTap;
        WhenUnselected += ResetTapCount;
    }

    protected virtual void Update()
    {
        if (IsTimerStarted)
        {
            _time += Time.deltaTime;
            if (_time > tapInterval)
            {
                TapEvent();
            }
        }

        textMesh.text = _tapCount.ToString();

        if (_selecting != ActiveState.Active)
        {
            _selecting = ActiveState.Active;
            if (_selecting)
            {
                WhenSelected();
            }
            else
            {
                WhenUnselected();
            }
        }
    }
    
    void InsertTap()
    {
        if (_time < tapInterval) _tapCount += 1; 
    }

    void ResetTapCount()
    {
        IsTimerStarted = true;
    }

    void TapEvent()
    {
        IsTimerStarted = false;
        switch (_tapCount)
        {
            case 1:
                OneTapEvent.Invoke();
                break;
            case 2:
                TwoTapEvent.Invoke();
                break;
        }
        _tapCount = 0;
    }
}