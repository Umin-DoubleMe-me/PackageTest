using UnityEngine;

namespace DoubleMe.Modules.Manipulator
{
	public interface IXRGrabbable
	{
		public void Init() { }
		public void Select() { }
		public void UnSelect() { }
		public void Hover() { }
		public void UnHover() { }
	}

	public class XRIObjectGrabbable : MonoBehaviour, IXRGrabbable
	{
		public virtual void Select()
		{
			
		}
		public virtual void UnSelect()
		{

		}
		public virtual void Hover()
		{ 
		
		}
		public virtual void UnHover() 
		{ 
		
		}
	}
}