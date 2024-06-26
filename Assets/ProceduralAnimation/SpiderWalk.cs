using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWalk : MonoBehaviour
{
    [SerializeField] private Transform[] legTargets;
    [SerializeField] private float stepSize = 1f;
    [SerializeField] private int smoothness = 1;
    [SerializeField] private float stepHeight = 0.1f;
    [SerializeField] private bool bodyOrientation = true;

    [SerializeField] private float raycastDistance = 1f;
    [SerializeField] private LayerMask whatIsGround;

    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private Vector3 lastBodyUp;
    private bool legMoving;
    private int nbLegs;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;

    private int counter;

    [SerializeField] private float velocityMultiplier = 15f;

    private Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        Ray ray = new(point + halfRange * up, -up);

        if (Physics.Raycast(ray, out RaycastHit hit, 2f * halfRange, whatIsGround))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }

    private void Awake()
    {
        lastBodyUp = transform.up;

        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = false;
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
        }
        lastBodyPos = transform.position;
        counter = 0;
    }

    private IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for (int i = 1; i <= smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));
            legTargets[index].position += Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight * transform.up;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        counter = (counter + 1) % 3;
        if(counter == 0) AudioManager.instance.Play("Footstep");
        legMoving = false;
    }


    private void FixedUpdate()
    {
        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;


        Vector3[] desiredPositions = new Vector3[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up).magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];

        if (indexToMove != -1 && !legMoving)
        {
            Vector3 targetPoint = desiredPositions[indexToMove] + velocity * velocityMultiplier;
            Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastDistance, transform.up);
            legMoving = true;
            StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
        }

        lastBodyPos = transform.position;
        if (nbLegs > 3 && bodyOrientation)
        {
            Vector3 v1 = legTargets[0].position - legTargets[1].position;
            Vector3 v2 = legTargets[2].position - legTargets[3].position;
            Vector3 normal = Vector3.Cross(v1, v2).normalized;
            Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
            transform.up = up;
            lastBodyUp = up;
        }
    }
}
