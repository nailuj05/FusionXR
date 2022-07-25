using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Fusion.XR
{
    public class TestGrabber : MonoBehaviour
    {
        public Transform palm;
        public float radius;

        public LayerMask grabMask;

        public bool isGrabbing = false;

        private TestGrabbable currentGrabbable;
        private GameObject grabPoint;

        public TrackingMode mode;
        public TrackingBase trackingBase;
        private TrackDriver driver;

        [HideInInspector]
        public Vector3 targetPosition;
        [HideInInspector]
        public Quaternion targetRotation;

        public Transform controller;

        private void Start()
        {
            driver = Utils.DriverFromEnum(mode);
            trackingBase.tracker = this.transform;
            trackingBase.palm = palm;
            driver.StartTrack(transform, trackingBase);
        }

        private void FixedUpdate()
        {
            driver.UpdateTrackFixed(controller.position, controller.rotation);
        }

        void Update()
        {
            driver.UpdateTrack(controller.position, controller.rotation);

            targetPosition = controller.position;
            targetRotation = controller.rotation;

            if (isGrabbing)
                return;

            GameObject[] possibleGrabs = Physics.OverlapSphere(palm.position, radius, grabMask).Select(x => x.gameObject).ToArray();
    
            if (possibleGrabs.Length > 0)
            {
                GameObject nextGrab = Utils.ClosestGameObject(possibleGrabs, palm.position);
                Grab(nextGrab);
            }
        }

        private void Grab(GameObject nextGrab)
        {
            var nextGrabbable = nextGrab.GetComponent<TestGrabbable>();

            grabPoint = nextGrabbable.GetClosestGrabPoint(palm.position);
            if (!grabPoint)
            {
                grabPoint = new GameObject("GrabPoint");
                grabPoint.transform.parent = nextGrab.transform;
                grabPoint.transform.position = nextGrab.GetComponent<Collider>().ClosestPoint(palm.position);
                grabPoint.transform.rotation = transform.rotation;
            }

            isGrabbing = true;
            currentGrabbable = nextGrabbable;

            StartCoroutine(Grab(currentGrabbable));
        }

        private IEnumerator Grab(TestGrabbable grabbable)
        {
            GetComponentInChildren<Collider>().enabled = false;
            transform.position = grabPoint.transform.position;
            transform.rotation = grabPoint.transform.rotation;
            yield return null;

            grabbable.Grab(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(palm.position, radius);
        }
    } 
}
