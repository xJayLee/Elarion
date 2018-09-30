﻿using System.Diagnostics;
using System.Reflection;
using Elarion.Attributes;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Elarion {
    public abstract class ExtendedBehaviour : MonoBehaviour {
    
        protected virtual void Reset() {
            WriteDefaults();
        }

        protected virtual void OnValidate() {
            WriteDefaults();
        }
        
        [Conditional("UNITY_EDITOR")]
        protected virtual void OnWriteDefaults() { }

        [Conditional("UNITY_EDITOR")]
        private void WriteDefaults() {
            OnWriteDefaults();
            
            var type = GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
            for(int i = 0; i < fields.Length; ++i) {
                var field = fields[i];
            
                if(field.GetCustomAttributes(typeof(GetComponentAttribute), false).Length < 1) {
                    continue;
                }

                if(!field.IsPublic && field.GetCustomAttributes(typeof(SerializeField), false).Length < 1) {
                    Debug.LogError($"Trying to automatically GetComponent on a non-serialized field {field.Name}", this);
                    continue;
                }

                if(!field.FieldType.IsSubclassOf(typeof(Component))) {
                    Debug.LogError($"Trying to automatically GetComponent for the {field.Name} field, but the field doesn't inherit from UnityEngine.Component.", this);
                    continue;
                }
            
                var value = field.GetValue(this);

                if(value != null) {
                    var stringValue = value.ToString();
                
                    if(stringValue.Length > 0 && stringValue.ToLowerInvariant() != "null") {
                        continue;
                    }
                }
            
                var component = GetComponent(field.FieldType);
                
                if(component == null) {
                    Debug.LogError($"Trying to automatically GetComponent for the {field.Name} field, but a component of that type is missing. You can use [RequireComponent] to ensure that the component will be available.", this);
                    continue;
                }
            
                field.SetValue(this, component);
            }
        } 
    }
}