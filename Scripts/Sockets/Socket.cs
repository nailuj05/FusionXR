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
        public AttractType attractType = AttractType.Grabbables;

        [Tooltip("The Layers used to filter the possible attachments")]
        public LayerMask attractLayers = ~0;

        [Tooltip("A list of tags used to filter the possible attachments. Leave empty if all tags are allowed.")]
        public string[] tagMask = new string[0];

        [Min(0)]
        public float attractionRange = 0.1f;
        public Vector3 attractionZoneOffset = Vector3.zero;

        [Tooltip("Should a grabbable forcefully detach if it is still grabbed?")]
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
        private IGrabbable attachedGrabbable;

        private bool hasAttachedObject;

        private string storedTag;

        private List<IGrabbable> possibleAttachObjects = new List<IGrabbable>();

        //-----------------------------//

        private void Start()
        {
            InitCollider();

            if (attachedObject)
            {
                //Does the inital attached object matches the requirements?
                if (CheckAttachementRequirements(attachedObject))
                    Attach(attachedObject);
                else
                    attachedObject = null;
            }

            if (showPreview) InitPrevObject();
        }

        private void Update()
        {
            SocketTick();
        }

        public void OnTriggerEnter(Collider newColl)
        {
            //If there already is a attached Object or the collider is also set to isTrigger
            if (hasAttachedObject | newColl.isTrigger)
                return;

            if (CheckAttachementRequirements(newColl.gameObject))
            {
                if(newColl.TryGetComponent(out IGrabbable grabbable) && grabbable.isGrabbed)
                {
                    if (forceDetach)
                    {
                        for(int i = grabbable.attachedHands.Count - 1; i >= 0; i--)
                        {
                            grabbable.attachedHands[i].Release();
                        }
                    }
                    else
                    {
                        possibleAttachObjects.Add(grabbable);
                        return;
                    }
                }

                //Attach Object
                Attach(newColl.gameObject);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (canBeGrabbed && attachedGrabbable != null && attachedGrabbable.isGrabbed) Release();

            if (showPreview) DrawPrevObject();

            if (hasAttachedObject) return;

            for (int i = 0; i < possibleAttachObjects.Count; i++)
            {
                //Check whether a grabbable has been released
                if (!possibleAttachObjects[i].isGrabbed)
                {
                    //Check if the object is still possible to attach
                    //Also checks if it has been attached by another object
                    if (CheckAttachementRequirements(possibleAttachObjects[i].GameObject))
                    {
                        Attach(possibleAttachObjects[i].GameObject);
                    }
                    else
                    {
                        possibleAttachObjects.RemoveAt(i);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider exitingColl)
        {
            if (exitingColl.TryGetComponent(out IGrabbable grabbable) && possibleAttachObjects.Contains(grabbable))
                possibleAttachObjects.Remove(grabbable);

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

        //TODO: Make the public functions protected if possible
        public virtual void Release()
        {
            //Debug.Log($"Release object: {attachedObject.name}");

            trackDriver.EndTrack();

            attachedObject.tag = storedTag;
            attachedGrabbable = null;
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

            if (attachedObject.TryGetComponent(out Grabbable grabbable))
            {
                if (possibleAttachObjects.Contains(grabbable))
                {
                    possibleAttachObjects.Remove(grabbable);
                }

                attachedGrabbable = grabbable;
            }

            //Tag the attached GameObject as "Attached"
            //SOLVES: Multiple overlapping sockets can attach the same object, 
            //because they don't know that a object is already attached by a different socket
            //---------------------------------------------------------------------------------//
            //This allows any object to be marked as attached, even if it has no scripts attached
            //Needed because any rigidbody can be attached
            storedTag = objectToAttach.tag;
            try { objectToAttach.tag = "Attached"; }
            catch { Debug.LogError("The 'Attached' Tag was not defined. Please make sure all layers and tags are properly setup."); }

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
                if (CheckAttachementRequirements(collider.gameObject))
                {
                    possibleObjects.Add(collider.gameObject);
                }
            }

            if (possibleObjects.Count > 0)
                return true;
            else
                return false;
        }

        public virtual bool CheckAttachementRequirements(GameObject obj)
        {
            //Check if already attached
            if (obj.tag == "Attached") { Debug.Log("Already attached"); return false; }

            //Check Tags
            bool tagCorrect = Utilities.ObjectMatchesTags(obj, tagMask);

            //Check Attract Type
            bool typeCorrect = Utilities.ObjectMatchesAttractType(obj, attractType);

            //Check Layers
            bool layerCorrect = Utilities.ObjectMatchesLayermask(obj, attractLayers);

            //All true?
            return (layerCorrect & tagCorrect & typeCorrect);
        }

        #region Initalizing

        public virtual void InitCollider()
        {
            _collider = gameObject.GetOrAddComponent<SphereCollider>();
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
                prevMeshFilter.mesh = possibleAttachObjects[0].GameObject.GetComponent<MeshFilter>().mesh;
                prevObject.transform.localScale = possibleAttachObjects[0].Transform.lossyScale;
            }
        }

        public void InitPrevObject()
        {
            prevObject = new GameObject();
            prevObject.transform.parent = transform;
            prevObject.AddComponent<MeshRenderer>().material = prevMaterial;
            prevMeshFilter = prevObject.AddComponent<MeshFilter>();
        } 

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!attachedObject) return;

            var offsetPos = transform.TransformPoint(attractionZoneOffset);
            Gizmos.color = new Color(0, 0, 1, 0.2f);
            Gizmos.DrawSphere(offsetPos, attractionRange);

            if (showPreview & attachedObject.TryGetComponent(out MeshFilter filter))
            {
                Gizmos.DrawWireMesh(filter.sharedMesh, 
                    transform.position, transform.rotation, 
                    attachedObject.transform.lossyScale);
            }
        }
#endif
    }
}