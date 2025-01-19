using Reconnect.Interactions;
using UnityEngine;

namespace Reconnect.Player
{
    public class PlayerInteractions : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            // Left button to interact
            if (Input.GetMouseButtonDown(3))
            {
                // Raycasting => see if there is an aimed iteractable object
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out var hit, 6f))
                {
                    // draw the ray
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

                    var target = hit.transform.gameObject;
                    var interactableComponent = target.GetComponent<IInteractable>();
                    if (interactableComponent is null)
                    {
                        Debug.Log("Not an interactable");
                    }
                    else if (!interactableComponent.CanInteract())
                    {
                        Debug.Log("Cannot interact");
                    }
                    else
                    {
                        interactableComponent.Interact();
                    }
                }
                else
                {
                    // draw the ray
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                }
            }
        }
    }

}

