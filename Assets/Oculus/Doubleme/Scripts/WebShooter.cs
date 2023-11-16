using Oculus.Interaction.Input;
using UnityEngine;

public enum WebShooterState { None, Shoot, UnShoot}

public class WebShooter : MonoBehaviour
{
    [Header("Core Variables")]
    public Transform shooterTip;
    public GameObject webEnd;
    public LineRenderer lineRenderer;
    public Hand ovrHand;

    [Header("Web Settings")]
    public float moveMentSpeed;
    public float maxDistance;
    public LayerMask webLayers;

    private Vector3 webPoint;
    private float distanceFromPoint;
    private Rigidbody tempRigidbody;
    [SerializeField] private Transform palm;

    private WebShooterState eWebShooterState = WebShooterState.None;

    private void FixedUpdate()
    {
        ovrHand.GetJointPose(HandJointId.HandMiddle1, out Pose pose);
        shooterTip.transform.position = pose.position;

        if (eWebShooterState != WebShooterState.None && tempRigidbody != null)
        {
            distanceFromPoint = Vector3.Distance(shooterTip.transform.position, tempRigidbody.position) * 0.75f;

            switch (eWebShooterState)
            {
                case WebShooterState.Shoot:
                    if (distanceFromPoint > 0.3f)
                    {
                        tempRigidbody.position = Vector3.MoveTowards(tempRigidbody.position, shooterTip.position, moveMentSpeed);
                    }
                    break;
                case WebShooterState.UnShoot:
                    Vector3 direction = Camera.main.transform.forward;
                    tempRigidbody.transform.Translate(direction * moveMentSpeed, UnityEngine.Space.World);
                    break;
            }

        }
    }

    public void ShootWeb()
    {
        eWebShooterState = WebShooterState.Shoot;
        RaycastHit hit;
        Ray ray = new Ray(shooterTip.position, -palm.right);
        
        lineRenderer.positionCount = 0;
        if (Physics.Raycast(ray, out hit, maxDistance, webLayers))
        {
            webPoint = hit.point;
            webEnd.transform.position = webPoint;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, shooterTip.position);
            lineRenderer.SetPosition(1, webEnd.transform.position);

            if (hit.transform.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbody))
            {
                tempRigidbody = rigidbody;
                tempRigidbody.isKinematic = false;
            }
        }
        else
        {
            Vector3 endPoint = ray.GetPoint(maxDistance);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, shooterTip.position);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    public void StopWeb()
    {
        if(tempRigidbody != null) tempRigidbody.isKinematic = true;
        lineRenderer.positionCount = 0;
        eWebShooterState = WebShooterState.None;
    }

    public void UnShootWeb()
    {
        eWebShooterState = WebShooterState.UnShoot;
    }

    public void ResetShootWeb()
    {
        StopWeb();
        tempRigidbody = null;
    }
}
