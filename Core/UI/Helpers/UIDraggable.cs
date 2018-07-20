﻿using Elarion.Attributes;
using Elarion.Extensions;
using Elarion.Saved.Events.UnityEvents;
using GameSparks.Api.Responses;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Elarion.UI.Helpers {
    
    [UIComponentHelper]
    [RequireComponent(typeof(RectTransform))]
    public class UIDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
        public enum MovementBounds {
            None,
            Parent,
            Screen,
            Custom
        }

        [SerializeField]
        private MovementBounds _bounds = MovementBounds.None;
        
        [SerializeField]
        [ConditionalVisibility("_bounds == MovementBounds.Custom")]
        private bool _dragX = true;
        
        [SerializeField]
        [ConditionalVisibility("_bounds == MovementBounds.Custom", "_dragX")]
        private Vector2 _xBounds = new Vector2(-400, 400);
        
        [SerializeField]
        [ConditionalVisibility("_bounds == MovementBounds.Custom")]
        private bool _dragY = true;

        [SerializeField]
        [ConditionalVisibility("_bounds == MovementBounds.Custom", "_dragY")]
        private Vector2 _yBounds = new Vector2(-400, 400);

        [SerializeField]
        private UnityEvent _onEndDrag = new UnityEvent();
        [SerializeField]
        private UnityEvent _onStartDrag = new UnityEvent();

        private Vector2 _dragDelta;

        private RectTransform _transform;

        public UnityEvent OnStartDragEvent => _onStartDrag;
        public UnityEvent OnEndDragEvent => _onEndDrag;
        
        /// <summary>
        /// Bounds to the movement. Can be restricted to Parent, Screen, or custom values.
        /// </summary>
        public MovementBounds Bounds {
            get { return _bounds; }
            set {
                _bounds = value;
                ClampToBounds();
            }
        }
        
        /// <summary>
        /// The custom bounds for X and Y. Limits the local position.
        /// Format: Vector4(minX, maxX, minY, maxY)
        /// </summary>
        public Vector4 CustomBounds {
            get {
                return new Vector4(_xBounds.x, _xBounds.y, _yBounds.x, _yBounds.y);
            }
            set {
                _xBounds = new Vector2(value.x, value.y);
                _yBounds = new Vector2(value.z, value.w);
                ClampToBounds();
            }
        }

        protected void Awake() {
            _transform = transform as RectTransform;
        }

        private void OnEnable() {
            ClampToBounds();
        }

        public void OnBeginDrag(PointerEventData eventData) {
            OnStartDragEvent.Invoke();
        }

        public void OnDrag(PointerEventData eventData) {
            var delta = eventData.delta;

            if(Bounds == MovementBounds.Custom) {
                if(!_dragX) {
                    delta.x = 0;
                }

                if(!_dragY) {
                    delta.y = 0;
                }
            }
            
            transform.Translate(delta);
            ClampToBounds();
        }

        public void OnEndDrag(PointerEventData eventData) {
            _onEndDrag.Invoke();
        }
        
        private void ClampToBounds() {
            var position = _transform.localPosition;

            switch(Bounds) {
                case MovementBounds.Screen: {
                    Vector2 screenSWCorner;
                    Vector2 screenNECorner;
            
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, Vector2.zero, null, out screenSWCorner);

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, new Vector2(Screen.width, Screen.height), null, out screenNECorner);

                    screenNECorner -= _transform.sizeDelta;
            
                    var deltaX = Mathf.Clamp(screenSWCorner.x, 0, Mathf.Infinity);
            
                    if(deltaX == 0) {
                        deltaX = Mathf.Clamp(screenNECorner.x, Mathf.NegativeInfinity, 0);
                    }
            
                    position.x += deltaX;

                    var deltaY = Mathf.Clamp(screenSWCorner.y, 0, Mathf.Infinity);
            
                    if(deltaY == 0) {
                        deltaY = Mathf.Clamp(screenNECorner.y, Mathf.NegativeInfinity, 0);
                    }
            
                    position.y += deltaY;
                    
                    break;
                }
                case MovementBounds.Parent: {
                    var parentTransform = transform.parent as RectTransform;

                    if(!parentTransform) {
                        break;
                    }
                    
                    Vector3 minPosition = parentTransform.rect.min - _transform.rect.min;
                    Vector3 maxPosition = parentTransform.rect.max - _transform.rect.max;
 
                    position.x = Mathf.Clamp(_transform.localPosition.x, minPosition.x, maxPosition.x);
                    position.y = Mathf.Clamp(_transform.localPosition.y, minPosition.y, maxPosition.y);
                    
                    break;
                }
                case MovementBounds.Custom: {

                    if(_dragX) {
                        position.x = Mathf.Clamp(_transform.localPosition.x, _xBounds.x, _xBounds.y);
                    }

                    if(_dragY) {
                        position.y = Mathf.Clamp(_transform.localPosition.y, _yBounds.x, _yBounds.y);
                    }
                    
                    break;
                }
                case MovementBounds.None:
                    break;
                default:
                    goto case MovementBounds.None;
            }

            _transform.localPosition = position;
        }

    }
}