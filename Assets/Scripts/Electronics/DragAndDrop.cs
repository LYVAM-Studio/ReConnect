using System;
using UnityEngine;

namespace Reconnect.Electronics
{
    public class DragAndDrop : MonoBehaviour
    {
        private bool _dragging = false;
        private float _distance;
        private Vector3 _starDist;

        void OnMouseDown()
        {
            _distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            _dragging = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(_distance);
            _starDist = transform.position - rayPoint;
        }

        void OnMouseUp()
        {
            _dragging = false;
        }

        void Update()
        {
            if (_dragging)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayPoint = ray.GetPoint(_distance);
                transform.localPosition = new Vector3(rayPoint.x + _starDist.x, rayPoint.y + _starDist.y, 0);
            }
        }

    }

}

