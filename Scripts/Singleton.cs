using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fusion.XR
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T main
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<T>();
                if (instance == null)
                    Debug.Log($"No object of type {typeof(T).ToString()} found.");
                return instance;
            }
        }

        protected void OnEnable()
        {
            if (instance == null)
                instance = this as T;
            if (instance != this)
                DestroyMe();
        }

        void DestroyMe()
        {
            if (Application.isPlaying)
                Destroy(this);
            else
                DestroyImmediate(this);
        }
    }
}
