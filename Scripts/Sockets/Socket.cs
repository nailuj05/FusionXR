using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class Socket : MonoBehaviour
    {
        [Header("Attraction")]
        [Tooltip("The AttractType defines which kind of objects can be attracted (attached) to the Socket")]
        public AttractType attractType = AttractType.Grabables;

        [Min(0)]
        public float attractionRange = 0.1f;
        public Vector3 attractionZoneOffset = Vector3.zero;

        [Tooltip("Should a grabable forcefully detach if it is still grabbed?")]
        public bool forceDetach;

        [Tooltip("Should the socket release the object if the player grabs it?")]
        public bool canBeGrabbed = true;

        [Tooltip("Should a hologram of the possible attachment be shown?")]
        public bool showPreview = true;

        [Tooltip("The material of the hologram")]
        public Material prevMaterial;

        private GameObject prevObject;
        private MeshFilter prevMeshFilter;

        private SphereCollider _collider;

        [Header("Attachment")]
        [Tooltip("The Track Driver used for attaching the object. Works best with Passive Joint and Velocity tracking.")]
        public TrackingMode attachedTrackingMode;

        [Tooltip("The settings for the Track Driver")]
        public TrackingBase attachedTrackingBase = new TrackingBase();
        private TrackDriver trackDriver;

        [Tooltip("The object attached to this socket, can be set from the editor as a default attachment")]
        public GameObject attachedObject;
        private Grabable attachedGrabable;

        private bool hasAttachedObject;

        private List<Grabable> possibleAttachObjects = new List<Grabable>();

        private void Start()
        {
            InitCollider();

            if (attachedObject) Attach(attachedObject);

            if (showPreview) InitPrevObject();
        }

        private void Update()
        {
            SocketTick();
        }

        public void OnTriggerEnter(Collider newColl)
        {
            if (hasAttachedObject)
                return;
            
            if (ObjectMatchesAttractType(newColl.gameObject, attractType))
            {
                if(newColl.TryGetComponent(out Grabable grabable) && grabable.isGrabbed)
                {
                    if (forceDetach)
                    {
                        for(int i = grabable.attachedHands.Count - 1; i >= 0; i--)
                        {
                            grabable.attachedHands[i].Release();
                        }
                    }
                    else
                    {
                        possibleAttachObjects.Add(grabable);
                        return;
                    }
                }

                //Attach Object
                Attach(newColl.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (canBeGrabbed && attachedGrabable && attachedGrabable.isGrabbed) Release();

            if (showPreview) DrawPrevObject();

            if (hasAttachedObject) return;

            for (int i = 0; i < possibleAttachObjects.Count; i++)
            {
                //Check whether a grabable has been released
                if (!possibleAttachObjects[i].isGrabbed)
                {
                    Attach(possibleAttachObjects[i].gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider exitingColl)
        {
            if (exitingColl.TryGetComponent(out Grabable grabable) && possibleAttachObjects.Contains(grabable))
                possibleAttachObjects.Remove(grabable);

            if (hasAttachedObject && exitingColl.gameObject == attachedObject)
            {
                //Release Object
                Release();
            }
        }

        protected virtual void SocketTick()
        {
            if (hasAttachedObject)
            {
                trackDriver.UpdateTrack(transform.position, transform.rotation);
            }
        }

        public virtual void Release()
        {
            //Debug.Log($"Release object: {attachedObject.name}");

            trackDriver.EndTrack();

            attachedGrabable = null;
            attachedObject = null;
            hasAttachedObject = false;
        }

        public virtual void Attach(GameObject objectToAttach)
        {
            //Debug.Log($"Attach object: {objectToAttach.name}");

            attachedTrackingBase.tracker = gameObject;

            trackDriver = Utilities.DriverFromEnum(attachedTrackingMode);
            trackDriver.StartTrack(objectToAttach.transform, attachedTrackingBase);

            attachedObject = objectToAttach;

            if (attachedObject.TryGetComponent(out Grabable grabable))
            {
                if (possibleAttachObjects.Contains(grabable))
                {
                    possibleAttachObjects.Remove(grabable);
                }

                attachedGrabable = grabable;
            }

            hasAttachedObject = true;
        }

        public virtual bool CheckForAttachement(out List<GameObject> possibleObjects)
        {
            var offsetPos = transform.TransformPoint(attractionZoneOffset);
            Collider[] collisions = Physics.OverlapSphere(offsetPos, attractionRange);
            possibleObjects = new List<GameObject>();

            if (collisions.Length == 0)
                return false;

            foreach (var collider in collisions)
            {
                if (ObjectMatchesAttractType(collider.gameObject, attractType))
                {
                    possibleObjects.Add(collider.gameObject);
                }
            }

            if (possibleObjects.Count > 0)
                return true;
            else
                return false;
        }

        public static bool ObjectMatchesAttractType(GameObject obj, AttractType attractType)
        {
            //Dont attach hands
            if (obj.TryGetComponent<FusionXRHand>(out FusionXRHand hand))
            {
                return false;
            }

            if (attractType == AttractType.Grabables)
            {
                return obj.TryGetComponent<Grabable>(out Grabable g);
            }
            else if (attractType == AttractType.Rigidbodys)
            {
                return obj.TryGetComponent<Rigidbody>(out Rigidbody r);
            }
            else //if (attractType == AttractType.AllCollisionObjects)
            {
                return true;
            }
        }

        public virtual void InitCollider()
        {
            _collider = gameObject.AddComponent<SphereCollider>();
            _collider.center = attractionZoneOffset;
            _collider.radius = attractionRange;
            _collider.isTrigger = true;
        }

        public void DrawPrevObject()
        {
            prevObject.SetActive(!hasAttachedObject & possibleAttachObjects.Count > 0);

            prevObject.transform.position = transform.position;
            prevObject.transform.rotation = transform.rotation;

            if (possibleAttachObjects.Count > 0)
            {
                prevMeshFilter.mesh = possibleAttachObjects[0].GetComponent<MeshFilter>().mesh;
                prevObject.transform.localScale = possibleAttachObjects[0].transform.lossyScale;
            }
        }

        public void InitPrevObject()
        {
            prevObject = new GameObject();
            prevObject.transform.parent = transform;
            prevObject.AddComponent<MeshRenderer>().material = prevMaterial;
            prevMeshFilter = prevObject.AddComponent<MeshFilter>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var offsetPos = transform.TransformPoint(attractionZoneOffset);
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawSphere(offsetPos, attractionRange);

            if (showPreview & attachedObject & attachedObject.TryGetComponent(out MeshFilter filter))
            {
                Gizmos.DrawWireMesh(filter.sharedMesh, 
                    transform.position, transform.rotation, 
                    attachedObject.transform.lossyScale);
            }
        }
#endif
    }
}