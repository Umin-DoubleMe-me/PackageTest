using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMe.Modules.Platform
{
    public class OculusPlatform : IPlatform
    {
        private const float _boundsClipperZ = 0.01f;
        private const float _offsetClipperZ = 0.001f;
        private const string _maskColliderName = "MaskCollider";
        private const float _enterHoverNormal = 0.05f;
        private const float _exitHoverNormal = 0.05f;
        private const float _cancelSelectNormal = 0.05f;
        private const float _maxPinDistance = 0f;

        public PlatformUserData UserData { get; set; }
        public Action<bool> OnResult { get; set; }
        public Action<bool> OnNonce { get; set; }
        public void GetPlatformFriends() => Users.GetLoggedInUserFriends().OnComplete(GetFriends);
        public void GetPlatformLoginUser() => Users.GetLoggedInUser().OnComplete(GetUserData);
        public void GetPlatformUserNonce() => Users.GetUserProof().OnComplete(GetNonceData);
        public BoundsClipper BoundsClipper;
        private void GetFriends(Message<UserList> message)
        {
            Debug.Log($"{DebugData.LogFormat} GetFriends Start");
            if (message == null || (message != null && message.IsError))
                return;

            if (UserData == null) return;

            if (UserData.UserFriends == null)
                UserData.UserFriends = new List<PlatformUserData>();
            else if (UserData.UserFriends != null && UserData.UserFriends.Count > 0)
                UserData.UserFriends.Clear();
            try
            {
                for (int i = 0; i < message.Data.Count; i++)
                {
                    UserData.UserFriends.Add(new PlatformUserData() { UserID = message.Data[i].ID.ToString(), UserNickName = message.Data[i].OculusID });
                    Debug.Log($"{DebugData.LogFormat} Friend NickName {message.Data[i].OculusID} Oculus ID {message.Data[i].ID}");
                }
            }
            catch
            {
                Debug.Log($"{DebugData.LogFormat} User Friend Data Miss");
            }

            Debug.Log($"{DebugData.LogFormat} GetFriends {message.Data.Count}");
        }

        private void GetNonceData(Message<UserProof> message)
        {
            if (message == null || (message != null && message.IsError))
            {
                if (string.IsNullOrEmpty(UserData.Nonce))
                    OnResult?.Invoke(false);
                else
                    OnNonce?.Invoke(false);
                return;
            }

            if (string.IsNullOrEmpty(UserData.Nonce))
            {
                UserData.Nonce = message.Data.Value;
                OnResult?.Invoke(true);
            }
            else
            {
                UserData.Nonce = message.Data.Value;
                OnNonce?.Invoke(true);
            }

            Debug.Log($"{DebugData.LogFormat} {UserData.Nonce}");
        }

        private void GetUserData(Message<User> message)
        {
            if (message == null || (message != null && message.IsError))
            {
                OnResult?.Invoke(false);
                return;
            }
            UserData.UserID = message.Data.ID.ToString();
            UserData.UserNickName = message.Data.OculusID;
            GetPlatformFriends();
            GetPlatformUserNonce();
        }

        public void SetOculusUICanvas(Canvas canvas, Bounds bounds)
        {
            if (canvas == null && bounds == null) return;

            #region AddComponent
            GameObject maskCollider = new GameObject(_maskColliderName);
            BoundsClipper = maskCollider.AddComponent<BoundsClipper>();
            ClippedPlaneSurface clipped = maskCollider.AddComponent<ClippedPlaneSurface>();
            PlaneSurface planeSurface = canvas.gameObject.AddComponent<PlaneSurface>();
            PointableCanvas pointable = canvas.gameObject.AddComponent<PointableCanvas>();
            PokeInteractable pokeInteractable = canvas.gameObject.AddComponent<PokeInteractable>();
            RayInteractable rayInteractable = canvas.gameObject.AddComponent<RayInteractable>();
            #endregion

            #region SetMaskCollider
            maskCollider.transform.SetParent(canvas.transform, false);
            float calibrateScale = maskCollider.transform.localScale.x / canvas.transform.localScale.x;
            maskCollider.transform.localScale = new Vector3(calibrateScale, calibrateScale, calibrateScale);
            #endregion

            #region InjectSetting
            pointable.InjectCanvas(canvas);
            pokeInteractable.InjectOptionalPointableElement(pointable);
            pokeInteractable.InjectSurfacePatch(clipped);
            rayInteractable.InjectOptionalPointableElement(pointable);
            rayInteractable.InjectSurface(clipped);
            List<IBoundsClipper> clippers = new List<IBoundsClipper>();
            clippers.Add(BoundsClipper);
            clipped.InjectPlaneSurface(planeSurface);
            clipped.InjectClippers(clippers);
            #endregion

            #region SetBoundClipper
            BoundsClipper.Size = (bounds.size.z <= _boundsClipperZ) ? new Vector3(bounds.size.x, bounds.size.y, _boundsClipperZ) : bounds.size;
            BoundsClipper.Size = new Vector3(BoundsClipper.Size.x, BoundsClipper.Size.y, (BoundsClipper.Size.z + _offsetClipperZ));
            BoundsClipper.Position = maskCollider.transform.InverseTransformPoint(bounds.center);
            #endregion

            #region SetProperty
            pokeInteractable.EnterHoverNormal = _enterHoverNormal;
            pokeInteractable.ExitHoverNormal = _exitHoverNormal;
            pokeInteractable.PositionPinning.MaxPinDistance = _maxPinDistance;
            pokeInteractable.CancelSelectNormal = _cancelSelectNormal;
			#endregion
		}

        public void Initialize()
        {
            try
            {
                UserData = new PlatformUserData();
                Core.AsyncInitialize().OnComplete(OnInitialize);
            }
            catch
            {
                OnResult?.Invoke(false);
            }
        }

        private void OnInitialize(Message<PlatformInitialize> message)
        {
            if (message == null || (message != null && message.IsError))
            {
                OnResult?.Invoke(false);
                return;
            }
            Entitlements.IsUserEntitledToApplication().OnComplete(OnUserEntitledCheck);
        }

        private void OnUserEntitledCheck(Message message)
        {
            if (message == null || (message != null && message.IsError))
            {
                OnResult?.Invoke(false);
                return;
            }
            GetPlatformLoginUser();
        }
    }
}