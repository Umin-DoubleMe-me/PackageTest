using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DoubleMe.Modules.Manipulator
{
	public enum ManipulatorMode
	{
		// 활성화 목록
		SpaceNomalMode = 0,                 //손 - distance, grab      / 오브젝트 - distance, grab
		SpaceUIMixMode,                     //손 - distnace, ray, grab / 오브젝트 - distance, ray, grab
		SpaceUIMixMode_NotObjectRay,        //손 - distnace, ray, grab / 오브젝트 - distance, grab
		SpaceUIMixMode_NotObjectDis,        //손 - distnace, ray, grab / 오브젝트 - ray, grab
		SpaceUIMixMode_NotObjectAnything,   //손 - distnace, ray, grab / 오브젝트 - x
		SpaceDrawLineMode,					//손 - distance, ray, grab / 오브젝트 - grab(지우개)
		OnlyUIMode,                         //손 - ray, grab           / 오브젝트 - ray
		OnlyUIMode_NotObject,               //손 - ray, grab           / 오브젝트 - x
		None,                               //모든 기능 정지
		Default                             //변경 없음
	}

	public class FingerEnum
	{
		public const int IKPOINT = 5;
		public const int THUMB = 0;
		public const int INDEX = 1;
		public const int MIDDLE = 2;
		public const int RING = 3;
		public const int PINKY = 4;
	}


	public enum InteractionType { GrabOnly, DistanceOnly, Both }

	public interface IPlatformManipulator
	{
		public bool IsAvailableDistanceGrab { get; }
		public bool IsAvailableHandGrab { get; }


		void InitData();

		/// <summary>
		/// 오브젝트 Manipulator 설정 및 초기화
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		//Task InitializeManipulator(Item item);

		//public void ChangeMode(ManipulatorMode AddMode, bool modeCount = true);
		//public void ChangeModeReset(ManipulatorMode MinuMode);
		//public Task ChangeSpaceItemMode(Item item, ManipulatorMode mode);

		/// <summary>
		/// mode가 null인 경우 현재 Mode로 적용됨
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		//public Task ChangeModeRightHand(ManipulatorMode? mode = null);

		/// <summary>
		/// mode가 null인 경우 현재 Mode로 적용됨
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		//public Task ChangeModeLeftHand(ManipulatorMode? mode = null);


		public void SetDefaultInteractor(InteractionType interactionType);

		public float[] Blendshapes { get; }
		public GameObject BoundBoxPrefab { get; }
	}

	public interface IPlatformHand
	{
		public Pose LeftHandPose { get; }
		public Pose RightHandPose { get; }

		public Pose LeftPointPose { get; }
		public Pose RightPointPose { get; }

		public bool IsLeftPinch { get; }
		public bool IsRightPinch { get; }

		public bool IsLeftTracking { get; }
		public bool IsRightTracking { get; }

		public Pose GetFinger(bool direction, int fingerIdx, bool isLocal);
		public void GetIKFingers(out Vector3[] position, out Quaternion[] rotation);

		public void SetOriginFingerLocalPose(List<Pose> localPoseData);
	}
}