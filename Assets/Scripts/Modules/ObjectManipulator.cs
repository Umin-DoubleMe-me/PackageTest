using System;
using UnityEngine;

namespace DoubleMe.Modules.Manipulator
{
	public class ObjectManipulator : MonoBehaviour
	{
		public Action OnHoverEnter;
		public Action OnHoverExit;
		public Action OnSelectEnter;
		public Action OnSelectExit;

		private IXRGrabbable _objectGrabbable;

		public virtual void Initialize(IXRGrabbable objectGrabbable)
		{
			_objectGrabbable = objectGrabbable;
		}

		private void Start()
		{
			if(_objectGrabbable != null)
			{
				OnSelectEnter += _objectGrabbable.Select;
				OnSelectExit += _objectGrabbable.UnSelect;
				OnHoverEnter += _objectGrabbable.Hover;
				OnHoverExit += _objectGrabbable.UnHover;
			}
		}

		protected virtual void OnDestroy()
		{
			if (_objectGrabbable != null)
			{
				OnSelectEnter -= _objectGrabbable.Select;
				OnSelectExit -= _objectGrabbable.UnSelect;
				OnHoverEnter -= _objectGrabbable.Hover;
				OnHoverExit -= _objectGrabbable.UnHover;
			}
		}

		#region EventDetail 
		public virtual void OnWhenPointEventRaised(PointEventType eventType)
		{
			switch (eventType)
			{
				case PointEventType.Hover:
					OnHoverEnter?.Invoke();
					break;
				case PointEventType.UnHover:
					OnHoverExit?.Invoke();
					break;
				case PointEventType.Select:
					OnSelectEnter?.Invoke();
					break;
				case PointEventType.UnSelect:
					OnSelectExit?.Invoke();
					break;
				default:
					break;
			}
		}
		#endregion

	}
}
