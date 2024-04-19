using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [SerializeField] private float recoilAmount;
    [SerializeField, Range(0f, 1f)] private float durationMultiplier;
    [SerializeField] private ParticleSystem muzzleFlash;

    private Tweener tween;
    public void Attack(float recoilDuration)
    {
        tween = transform.DOPunchPosition(recoilAmount * -Vector3.forward, recoilDuration * durationMultiplier);
        muzzleFlash.Play();
    }

    private void OnDestroy()
    {
        tween.Kill();
    }
}
