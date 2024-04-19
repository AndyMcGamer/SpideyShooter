using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float height;

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (followTarget != null)
        {
            transform.position = new Vector3(followTarget.position.x, height, followTarget.position.z);
        }
    }
}
