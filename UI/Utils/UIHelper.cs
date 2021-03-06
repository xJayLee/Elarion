﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace Elarion.UI.Utils {
    public static class UIHelper {
        public const string BlurShaderPath = "Materials/FrostedGlass"; 
        public const string ShadowImagePath = "Sprites/Shadow"; // needs an offset to function properly
        public const float ShadowImageOffset = -29;
        private const int DefaultRenderTextureDepth = 30;

        public static T Create<T>(string name = null, Transform parent = null) where T : Component {
            return CreateGO(name, parent).AddComponent<T>();
        }

        public static Canvas CreateAnimatorCanvas(out RectTransform transform, string name = null, Transform parent = null) {
            var canvas = CreateGO(name, parent).AddComponent<Canvas>();

            transform = canvas.gameObject.GetComponent<RectTransform>();

            return canvas;
        }
        
        public static Image CreateBlurImage(string name = null, Transform parent = null) {
            var image = CreateOverlayImage(name, parent);
            image.material = Resources.Load<Material>(BlurShaderPath);
            return image;
        }

        public static Image CreateShadowImage(string name = null, Transform parent = null) {
            var image = CreateOverlayImage(name, parent);
            image.sprite = Resources.Load<Sprite>(ShadowImagePath);
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            ResetShadowImage(image);
            return image;
        }
        
        public static Image CreateOverlayImage(string name = null, Transform parent = null) {
            var image = CreateGO(name, parent).AddComponent<Image>();
            ResetOverlayImage(image);
            image.transform.SetAsLastSibling();
            image.raycastTarget = false;
            return image;
        }

        public static void ResetOverlayImage(Image image) {
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
        }

        public static void ResetShadowImage(Image image) {
            ResetOverlayImage(image);
            image.rectTransform.offsetMax = new Vector2(-ShadowImageOffset, -ShadowImageOffset);
            image.rectTransform.offsetMin = new Vector2(ShadowImageOffset, ShadowImageOffset);
        }

        public static RenderTexture CreateRendureTexture(int width, int height) {
            return new RenderTexture(width, height, DefaultRenderTextureDepth);
        }

        public static Camera CreateUICamera(string name = null, Transform parent = null) {
            var camera = CreateGO(name, parent).AddComponent<Camera>();
            
            camera.useOcclusionCulling = false;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.clearFlags = CameraClearFlags.Nothing;
            camera.orthographic = true;
            camera.backgroundColor = Color.white;
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            
            return camera;
        }

        private static GameObject CreateGO(string name = null, Transform parent = null) {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);

            // Ignore layouts; we don't want helper UI elements to break automatic layouts
            var layout = gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            
            return gameObject;
        }

        public static void SetBlurIntensity(this Image blurImage, float intensity) {
            if(Math.Abs(blurImage.materialForRendering.GetFloat("_Radius") - intensity) < Mathf.Epsilon) {
                return;
            }
            
            blurImage.materialForRendering.SetFloat("_Radius", intensity);
        }

        public static Canvas CreateCanvas(string name = null, Transform parent = null) {
            var canvas = CreateGO(name, parent).AddComponent<Canvas>();
            return canvas;
        }
    }
}