using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaserGunScript : MonoBehaviourPunCallbacks
{
    public TrailRenderer laserTrailPrefab;
    public ParticleSystem hitParticleSystem;
    public Transform originTransform;
    public Animator animator;
    public AudioSource laserSoundAudioSource;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1") && photonView.IsMine)
        {
            photonView.RPC("Shoot", RpcTarget.All);
        }
    }

    [PunRPC]
    public void Shoot()
    {
        //play the shoot animation
        animator.SetTrigger("Shoot");
        //playe the sound
        laserSoundAudioSource.Play();
        var trail = Instantiate(laserTrailPrefab, originTransform.position, originTransform.rotation);
        trail.AddPosition(originTransform.position);
        //cast forward to see where we hit
        var ray = new Ray(originTransform.position, originTransform.forward);
        Vector3 hitPoint = originTransform.position + originTransform.forward * 1000f;
        if (Physics.Raycast(ray, out RaycastHit hit,1000f))
        {
            hitPoint = hit.point;
            Instantiate(hitParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        }
        trail.transform.position = hitPoint;
    }
}
