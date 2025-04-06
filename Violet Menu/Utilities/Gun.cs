using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VioletTemp.Utilities;
using static VioletTemp.Utilities.ColorLib;
using static VioletTemp.Mods.Categories.Settings;
using BepInEx;
using static VioletTemp.Menu.Main;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace VioletTemp.Utilities
{
    public class ClientInput
    {
        public static bool GetInputValue(float grabValue)
        {
            return grabValue >= 0.75f;
        }
    }
    public class GunTemplate : MonoBehaviour
    {
        public static int LineCurve = 150;
        private const float PointerScale = 0.15f;
        private const float LineWidth = 0.025f;
        private const float LineSmoothFactor = 6f;
        private const float DestroyDelay = 0.02f;
        private const float PulseSpeed = 2f;
        private const float PulseAmplitude = 0.03f;

        public static GameObject spherepointer;
        public static VRRig LockedPlayer;
        public static Vector3 lr;
        public static Color32 PointerColor = SkyBlue;//rgba(159, 0, 255, 1)
        public static Color32 LineColor = SkyBlue;//rgba(159, 0, 255, 1)
        public static Color32 TriggeredPointerColor = WineRed;
        public static Color32 TriggeredLineColor = WineRed;


        private static Vector3 CalculateBezierPoint(Vector3 start, Vector3 mid, Vector3 end, float t)
        {
            return Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * mid + Mathf.Pow(t, 2) * end;
        }

        private static void CurveLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
        {
            lineRenderer.positionCount = LineCurve;
            for (int i = 0; i < LineCurve; i++)
            {
                float t = (float)i / (LineCurve - 1);
                lineRenderer.SetPosition(i, CalculateBezierPoint(start, mid, end, t));
            }
        }

        private static IEnumerator StartCurvyLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
        {
            while (true)
            {
                CurveLineRenderer(lineRenderer, start, mid, end);
                lineRenderer.startColor = Color.Lerp(lineRenderer.startColor, new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, Mathf.PingPong(Time.time, 1)), 0.5f);
                lineRenderer.endColor = lineRenderer.startColor;
                yield return null;
            }
        }

        private static IEnumerator PulsePointer(GameObject pointer)
        {
            Vector3 originalScale = pointer.transform.localScale;
            while (true)
            {
                float scaleFactor = 1 + Mathf.Sin(Time.time * PulseSpeed) * PulseAmplitude;
                pointer.transform.localScale = originalScale * scaleFactor;
                yield return null;
            }
        }
        public static RaycastHit raycastHit;
        public static void StartVrGun(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out raycastHit, float.MaxValue);
                if (spherepointer == null)
                {
                    spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    spherepointer.AddComponent<Renderer>();
                    spherepointer.transform.localScale = new Vector3(0.040f, 0.040f, 0.040f);
                    spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    GameObject.Destroy(spherepointer.GetComponent<BoxCollider>());
                    GameObject.Destroy(spherepointer.GetComponent<Rigidbody>());
                    GameObject.Destroy(spherepointer.GetComponent<Collider>());
                    lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;

                    spherepointer.AddComponent<GunTemplate>().StartCoroutine(PulsePointer(spherepointer));
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = PointerColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }
                lr = Vector3.Lerp(lr, (GorillaTagger.Instance.rightHandTransform.position + spherepointer.transform.position) / 2f, Time.deltaTime * 6f);
                GameObject gameObject = new GameObject("Line");
                LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.010f;
                lineRenderer.endWidth = 0.010f;
                lineRenderer.startColor = LineColor;
                lineRenderer.endColor = LineColor;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = new Material(Shader.Find("GUI/Text Shader"));
                gameObject.AddComponent<GunTemplate>().StartCoroutine(StartCurvyLineRenderer(lineRenderer, GorillaTagger.Instance.rightHandTransform.position, lr, spherepointer.transform.position));
                GameObject.Destroy(lineRenderer, Time.deltaTime);
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    trigger = true;
                    lineRenderer.startColor = TriggeredLineColor;
                    lineRenderer.endColor = TriggeredLineColor;
                    spherepointer.GetComponent<Renderer>().material.color = TriggeredPointerColor;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                        }
                        return;
                    }
                    action();
                    return;
                }
                else if (LockedPlayer != null)
                {
                    LockedPlayer = null;
                    return;
                }
            }
            else if (spherepointer != null)
            {
                GameObject.Destroy(spherepointer);
                spherepointer = null;
                LockedPlayer = null;
            }
        }

        public static void StartPcGun(Action action, bool LockOn)
        {
            Ray ray = GameObject.Find("Shoulder Camera").activeSelf ? GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition) : GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);

            if (Mouse.current.rightButton.isPressed)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, float.PositiveInfinity, -32777) && spherepointer == null)
                {
                    if (spherepointer == null)
                    {
                        spherepointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        spherepointer.AddComponent<Renderer>();
                        spherepointer.transform.localScale = new Vector3(0.040f, 0.040f, 0.040f);
                        spherepointer.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                        GameObject.Destroy(spherepointer.GetComponent<BoxCollider>());
                        GameObject.Destroy(spherepointer.GetComponent<Rigidbody>());
                        GameObject.Destroy(spherepointer.GetComponent<Collider>());
                        lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;

                        spherepointer.AddComponent<GunTemplate>().StartCoroutine(PulsePointer(spherepointer));
                    }
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = PointerColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }
                lr = Vector3.Lerp(lr, (GorillaTagger.Instance.rightHandTransform.position + spherepointer.transform.position) / 2f, Time.deltaTime * 6f);
                GameObject gameObject = new GameObject("Line");
                LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.010f;
                lineRenderer.endWidth = 0.010f;
                lineRenderer.startColor = LineColor;
                lineRenderer.endColor = LineColor;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = new Material(Shader.Find("GUI/Text Shader"));
                gameObject.AddComponent<GunTemplate>().StartCoroutine(StartCurvyLineRenderer(lineRenderer, GorillaTagger.Instance.rightHandTransform.position, lr, spherepointer.transform.position));
                GameObject.Destroy(lineRenderer, Time.deltaTime);
                if (Mouse.current.leftButton.isPressed)
                {
                    trigger = true;
                    lineRenderer.startColor = TriggeredLineColor;
                    lineRenderer.endColor = TriggeredLineColor;
                    spherepointer.GetComponent<Renderer>().material.color = TriggeredPointerColor;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                        }
                        return;
                    }
                    action();
                    return;
                }
                else if (LockedPlayer != null)
                {
                    LockedPlayer = null;
                    return;
                }
            }
            else if (spherepointer != null)
            {
                GameObject.Destroy(spherepointer);
                spherepointer = null;
                LockedPlayer = null;
            }
        }

        public static void StartBothGuns(Action action, bool locko)
        {
            if (XRSettings.isDeviceActive)
            {
                StartVrGun(action, locko);
            }
            if (!XRSettings.isDeviceActive)
            {
                StartPcGun(action, locko);
            }
        }
        public static bool trigger = false;
        
    }

}
