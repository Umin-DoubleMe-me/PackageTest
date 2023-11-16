using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class TwoHandTransformer : MonoBehaviour, ITransformer
{
    // The active rotation for this transformation is tracked because it
    // cannot be derived each frame from the grab point information alone.
    private Quaternion _activeRotation;
    private Vector3 _initialLocalScale;
    private float _initialDistance;
    private float _initialScale = 1.0f;
    private float _activeScale = 1.0f;

    private Pose _previousGrabPointA;
    private Pose _previousGrabPointB;

    [Serializable]
    public class TwoGrabFreeConstraints
    {
        [Tooltip("If true then the constraints are relative to the initial scale of the object " +
                 "if false, constraints are absolute with respect to the object's x-axis scale.")]
        public bool ConstraintsAreRelative;
        public FloatConstraint MinScale;
        public FloatConstraint MaxScale;
    }

    [SerializeField]
    private TwoGrabFreeConstraints _constraints;

    public TwoGrabFreeConstraints Constraints
    {
        get
        {
            return _constraints;
        }

        set
        {
            _constraints = value;
        }
    }

    private IGrabbable _grabbable;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
    }

    public void BeginTransform()
    {
        //if (InteractionManager.Instance == null)
        return;

        Pose grabA, grabB;

        if (InteractionManager.Instance.LeftInteractor != null && InteractionManager.Instance.RightInteractor != null)
        {
            IInteractor leftInteractor = InteractionManager.Instance.LeftInteractor;
            IInteractor rightInteractor = InteractionManager.Instance.RightInteractor;

            if (leftInteractor is DistanceHandGrabInteractor && rightInteractor is DistanceHandGrabInteractor)
            {
                DistanceHandGrabInteractor leftHandGrab = leftInteractor as DistanceHandGrabInteractor;
                DistanceHandGrabInteractor rightHandGrab = rightInteractor as DistanceHandGrabInteractor;

                Vector3 combineA = Vector3.Lerp(leftHandGrab.Origin.position, leftHandGrab.HitPoint, 0.5f);
                Vector3 combineB = Vector3.Lerp(rightHandGrab.Origin.position, rightHandGrab.HitPoint, 0.5f);
                grabA = new Pose(combineA, leftHandGrab.Origin.rotation);
                grabB = new Pose(combineB, rightHandGrab.Origin.rotation);
            }
            else
            {
                grabA = _grabbable.GrabPoints[0];
                grabB = _grabbable.GrabPoints[1];
            }
        }
        else
        {
            grabA = _grabbable.GrabPoints[0];
            grabB = _grabbable.GrabPoints[1];
        }

        // Initialize our transformer rotation
        Vector3 diff = grabB.position - grabA.position;
        _activeRotation = Quaternion.LookRotation(diff, Vector3.up).normalized;
        _initialDistance = diff.magnitude;
        if (!_constraints.ConstraintsAreRelative)
        {
            _activeScale = _grabbable.Transform.localScale.x;
        }
        _initialScale = _activeScale;
        _initialLocalScale = _grabbable.Transform.localScale / _initialScale;

        _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
        _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
    }

    public void UpdateTransform()
    {
        if (InteractionManager.Instance == null)
            return;

        var targetTransform = _grabbable.Transform;

        Pose grabA, grabB;

        if (InteractionManager.Instance.LeftInteractor != null && InteractionManager.Instance.RightInteractor != null)
        {
            IInteractor leftInteractor = InteractionManager.Instance.LeftInteractor;
            IInteractor rightInteractor = InteractionManager.Instance.RightInteractor;

            if (leftInteractor is DistanceHandGrabInteractor && rightInteractor is DistanceHandGrabInteractor)
            {
                DistanceHandGrabInteractor leftHandGrab = leftInteractor as DistanceHandGrabInteractor;
                DistanceHandGrabInteractor rightHandGrab = rightInteractor as DistanceHandGrabInteractor;

                Vector3 combineA = Vector3.Lerp(leftHandGrab.Origin.position, leftHandGrab.HitPoint, 0.5f);
                Vector3 combineB = Vector3.Lerp(rightHandGrab.Origin.position, rightHandGrab.HitPoint, 0.5f);

                grabA = new Pose(combineA, leftHandGrab.Origin.rotation);
                grabB = new Pose(combineB, rightHandGrab.Origin.rotation);
            }
            else
            {
                grabA = _grabbable.GrabPoints[0];
                grabB = _grabbable.GrabPoints[1];
            }
        }
        else
        {
            grabA = _grabbable.GrabPoints[0];
            grabB = _grabbable.GrabPoints[1];
        }
        // Use the centroid of our grabs as the transformation center
        Vector3 initialCenter = Vector3.Lerp(_previousGrabPointA.position, _previousGrabPointB.position, 0.5f);
        Vector3 targetCenter = Vector3.Lerp(grabA.position, grabB.position, 0.5f);

        // Our transformer rotation is based off our previously saved rotation
        Quaternion initialRotation = _activeRotation;

        // The base rotation is based on the delta in vector rotation between grab points
        Vector3 initialVector = _previousGrabPointB.position - _previousGrabPointA.position;
        Vector3 targetVector = grabB.position - grabA.position;
        Quaternion baseRotation = Quaternion.FromToRotation(initialVector, targetVector);

        // Any local grab point rotation contributes 50% of its rotation to the final transformation
        // If both grab points rotate the same amount locally, the final result is a 1-1 rotation
        Quaternion deltaA = grabA.rotation * Quaternion.Inverse(_previousGrabPointA.rotation);
        Quaternion halfDeltaA = Quaternion.Slerp(Quaternion.identity, deltaA, 0.5f);

        Quaternion deltaB = grabB.rotation * Quaternion.Inverse(_previousGrabPointB.rotation);
        Quaternion halfDeltaB = Quaternion.Slerp(Quaternion.identity, deltaB, 0.5f);

        // Apply all the rotation deltas
        Quaternion baseTargetRotation = baseRotation * halfDeltaA * halfDeltaB * initialRotation;

        // Normalize the rotation
        Vector3 upDirection = baseTargetRotation * Vector3.up;
        Quaternion targetRotation = Quaternion.LookRotation(targetVector, upDirection).normalized;

        // Save this target rotation as our active rotation state for future updates
        _activeRotation = targetRotation;

        // Scale logic
        float activeDistance = targetVector.magnitude;
        if (Mathf.Abs(activeDistance) < 0.0001f) activeDistance = 0.0001f;

        float scalePercentage = activeDistance / _initialDistance;

        float previousScale = _activeScale;
        _activeScale = _initialScale * scalePercentage;

        if (_constraints.MinScale.Constrain)
        {
            _activeScale = Mathf.Max(_constraints.MinScale.Value, _activeScale);
        }
        if (_constraints.MaxScale.Constrain)
        {
            _activeScale = Mathf.Min(_constraints.MaxScale.Value, _activeScale);
        }

        // Apply the positional delta initialCenter -> targetCenter and the
        // rotational delta initialRotation -> targetRotation to the target transform
        Vector3 worldOffsetFromCenter = targetTransform.position - initialCenter;

        Vector3 offsetInTargetSpace = Quaternion.Inverse(initialRotation) * worldOffsetFromCenter;
        offsetInTargetSpace /= previousScale;

        Quaternion rotationInTargetSpace = Quaternion.Inverse(initialRotation) * targetTransform.rotation;

        targetTransform.position = (targetRotation * (_activeScale * offsetInTargetSpace)) + targetCenter;

        Quaternion rotation = targetRotation * rotationInTargetSpace;

        //targetTransform.rotation = Quaternion.Euler(0f,rotation.eulerAngles.y,0f);
        targetTransform.rotation = rotation;

        targetTransform.localScale = _activeScale * _initialLocalScale;

        _previousGrabPointA = new Pose(grabA.position, grabA.rotation);
        _previousGrabPointB = new Pose(grabB.position, grabB.rotation);
    }

    public void MarkAsBaseScale()
    {
        _activeScale = 1.0f;
    }

    public void EndTransform() { }

    #region Inject

    public void InjectOptionalConstraints(TwoGrabFreeConstraints constraints)
    {
        _constraints = constraints;
    }

    #endregion

}
