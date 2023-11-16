using DoubleMe.Modules.Manipulator;

public enum ObjectManipulatorMode
{
	//None = 0, 해당 컴포넌트 끄는것으로 대체할것.
	distance = 1,
	ray = 2,
	grab = 4,
	poke = 8,

	none = 0
}

public interface IXRInteractorType
{
	public ObjectManipulatorMode GetInteractorType();

	public void Init(XRIPlatformManipulator xRIPlatformManipulator);
}
