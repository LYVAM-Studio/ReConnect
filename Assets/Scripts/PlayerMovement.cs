using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{

    CharacterController characterController;
    float speed = 5f;
    Vector3 move = Vector3.zero;
    Vector3 velocity = Vector3.zero;
    float pitch = 0f;
    public Transform cameraPivot;
    public GameObject playerCamera;

    // Start is called before the first frame update
    public override void OnEnable()
    {
        if (photonView.IsMine)
        {
            characterController = GetComponent<CharacterController>();
        }
        else
        {
            playerCamera.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * 3f, 0);
            pitch -= Input.GetAxis("Mouse Y") * 3f;
            pitch = Mathf.Clamp(pitch, -60f, 60f);
            move.x = Input.GetAxisRaw("Horizontal");
            move.z = Input.GetAxisRaw("Vertical");
            move = Vector3.ClampMagnitude(move, 1f);
            velocity = transform.TransformVector(move) * speed;
            characterController.SimpleMove(velocity);
        }
        // done regardless of whether a client or a clone
        cameraPivot.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // this is the local client
            stream.SendNext(pitch);
        }
        else
        {
            // this is the clone
            pitch = (float)stream.ReceiveNext();
        }
    }
}
