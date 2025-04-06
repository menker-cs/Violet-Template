using VioletTemp.Mods;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static VioletTemp.Utilities.Variables;
using static VioletTemp.Utilities.ColorLib;
using static VioletTemp.Menu.Optimizations;
using static VioletTemp.Menu.ButtonHandler;
using static VioletTemp.Mods.Categories.Room;
using BepInEx;
using UnityEngine.InputSystem;
using HarmonyLib;
using static VioletTemp.Initialization.PluginInfo;
using VioletTemp.Utilities;
using System.IO;
//using g3;
using Valve.VR;
using UnityEngine.Animations.Rigging;
using Photon.Pun;
using UnityEngine.ProBuilder.MeshOperations;
using GorillaNetworking;
using System.Net;
using System.Threading;
using VioletTemp.Mods.Categories;
using GorillaExtensions;
using TMPro;
using System.Reflection;
using VioletTemp.Menu;
using static GorillaTelemetry;
using VioletTemp.Menu;
using VioletTemp.Mods;
using VioletTemp.Utilities;
using static VioletTemp.Menu.ButtonHandler;
using static VioletTemp.Menu.Optimizations;
using UnityEngine.UIElements;
using BepInEx;
using HarmonyLib;
using PlayFab.ExperimentationModels;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Photon.Realtime;

namespace VioletTemp.Menu
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer), "LateUpdate")]
    public class Main : MonoBehaviour
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            try
            {
                if (playerInstance == null || taggerInstance == null)
                {
                    UnityEngine.Debug.LogError("Player instance or GorillaTagger is null. Skipping menu updates.");
                    return;
                }

                foreach (ButtonHandler.Button bt in ModButtons.buttons)
                {
                    try
                    {
                        if (bt.Enabled && bt.onEnable != null)
                        {
                            bt.onEnable.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error invoking button action: {bt.buttonText}. Exception: {ex}");
                    }
                }

                if (NotificationLib.Instance != null)
                {
                    try
                    {
                        NotificationLib.Instance.UpdateNotifications();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error updating notifications. Exception: {ex}");
                    }
                }

                if (UnityInput.Current.GetKeyDown(PCMenuKey))
                {
                    PCMenuOpen = !PCMenuOpen;
                }

                HandleMenuInteraction();
            }
            catch (NullReferenceException ex)
            {
                UnityEngine.Debug.LogError($"NullReferenceException: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Unexpected error: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
            #region Layout Manager
            // sets layout to default
            // change int to however many layouts you have
            if (Laytou > 3)
            {
                Laytou = 1;
                RefreshMenu();
            }
            #endregion
        }
        public static float j = 0f;
        public static float k = 0.2f;
        public static void Trail(GameObject obj, Color clr)
        {
            GameObject trailObject = new GameObject("trail");
            trailObject.transform.position = obj.transform.position;
            trailObject.transform.SetParent(obj.transform);
            TrailRenderer trailRenderer = trailObject.AddComponent<TrailRenderer>();
            trailRenderer.material = new Material(Shader.Find("Unlit/Color"));
            trailRenderer.material.color = clr;
            trailRenderer.time = 0.5f;
            trailRenderer.startWidth = 0.025f;
            trailRenderer.endWidth = 0f;
            trailRenderer.startColor = clr;
            trailRenderer.endColor = clr;
        }
        public void Awake()
        {
            ResourceLoader.LoadResources();
            taggerInstance = GorillaTagger.Instance;
            playerInstance = GorillaLocomotion.GTPlayer.Instance;
            pollerInstance = ControllerInputPoller.instance;
            thirdPersonCamera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
            cm = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
        }
        public static void HandleMenuInteraction()
        {
            try
            {
                if (PCMenuOpen && !InMenuCondition && !pollerInstance.leftControllerPrimaryButton && !pollerInstance.rightControllerPrimaryButton && !menuOpen)
                {
                    InPcCondition = true;
                    cm?.SetActive(false);

                    if (menuObj == null)
                    {
                        Draw();
                        AddButtonClicker(thirdPersonCamera?.transform);
                    }
                    else
                    {
                        
                        AddButtonClicker(thirdPersonCamera?.transform);

                        if (thirdPersonCamera != null)
                        {
                            PositionMenuForKeyboard();

                            AddTitleAndFPSCounter();

                            try
                            {
                                if (Mouse.current.leftButton.isPressed)
                                {
                                    Ray ray = thirdPersonCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
                                    if (Physics.Raycast(ray, out RaycastHit hit))
                                    {
                                        BtnCollider btnCollider = hit.collider?.GetComponent<BtnCollider>();
                                        if (btnCollider != null && clickerObj != null)
                                        {
                                            btnCollider.OnTriggerEnter(clickerObj.GetComponent<Collider>());
                                        }
                                    }
                                }
                                else if (clickerObj != null)
                                {
                                    Optimizations.DestroyObject(ref clickerObj);
                                }
                            }
                            catch (Exception ex)
                            {
                                UnityEngine.Debug.LogError($"Error handling mouse click. Exception: {ex}");
                            }
                        }
                    }
                }
                else if (menuObj != null && InPcCondition)
                {
                    InPcCondition = false;
                    CleanupMenu(0);
                    cm?.SetActive(true);
                }

                openMenu = rightHandedMenu ? ControllerInputPoller.instance.rightGrab : ControllerInputPoller.instance.leftControllerSecondaryButton;

                if (openMenu && !InPcCondition)
                {
                    InMenuCondition = true;
                    if (menuObj == null)
                    {
                        
                        Draw();
                        AddRigidbodyToMenu();
                        AddButtonClicker(rightHandedMenu ? playerInstance.leftControllerTransform : playerInstance.rightControllerTransform);
                    }
                    else
                    {
                        AddTitleAndFPSCounter();
                        Trail(menuObj, MenuColor);
                        PositionMenuForHand();
                    }
                }
                else if (menuObj != null && InMenuCondition)
                {
                    InMenuCondition = false;
                    AddRigidbodyToMenu();

                    Vector3 currentVelocity = rightHandedMenu ? playerInstance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0f, false) : playerInstance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0f, false);
                    if (Vector3.Distance(currentVelocity, previousVelocity) > velocityThreshold)
                    {
                        currentMenuRigidbody.velocity = currentVelocity;
                        previousVelocity = currentVelocity;
                    }

                    CleanupMenu(1);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error handling menu interaction. Exception: {ex}");
            }
        }
        public static void Draw()
        {
            if (menuObj != null)
            {
                ClearMenuObjects();
                return;
            }
            CreateMenuObject();
            CreateBackground();
            CreateMenuCanvasAndTitle();
            AddDisconnectButton();
            AddReturnButton();
            AddPageButton(">");
            AddPageButton("<");

            ButtonPool.ResetPool();
            var PageToDraw = GetButtonInfoByPage(currentPage).Skip(currentCategoryPage * ButtonsPerPage).Take(ButtonsPerPage).ToArray();
            for (int i = 0; i < PageToDraw.Length; i++)
            {
                AddModButtons(i * 0.09f, PageToDraw[i]);
            }
        }
        private static void CreateMenuObject()
        {
            // Menu Object
            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
           
            menuObj.GetComponent<BoxCollider>();
            Destroy(menuObj.GetComponent<Renderer>());
            menuObj.name = "menu";
            menuObj.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
        }
        public static void Outline(GameObject obj, Color clr)
        {
           
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            UnityEngine.Object.Destroy(gameObject.GetComponent<BoxCollider>());
            gameObject.transform.parent = obj.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localPosition = obj.transform.localPosition;
            gameObject.transform.localScale = obj.transform.localScale + new Vector3(-0.01f, 0.0145f, 0.0145f);
            gameObject.GetComponent<Renderer>().material.color = clr;
        }

        private static void CreateBackground()
        {
            // Background
            background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(background.GetComponent<Rigidbody>());
            Destroy(background.GetComponent<BoxCollider>());
            Outline(background, outColor);
            Outline(background, outColor);
            background.GetComponent<MeshRenderer>().material.color = MenuColor;
            background.transform.parent = menuObj.transform;
            background.transform.rotation = Quaternion.identity;
            background.transform.localScale = new Vector3(0.1f, 1f, 1.03f);
            background.name = "menucolor";
            background.transform.position = new Vector3(0.05f, 0f, 0f);

            background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(background.GetComponent<Rigidbody>());
            Destroy(background.GetComponent<BoxCollider>());
            Outline(background, outColor);
            background.GetComponent<MeshRenderer>().material.color = MenuColor;
            background.transform.parent = menuObj.transform;
            background.transform.rotation = Quaternion.identity;
            background.transform.localScale = new Vector3(0.11f, 0.97f, 1f);
            background.name = "menucolor";
            background.transform.position = new Vector3(0.05f, 0f, 0f);

        }

        #region settings
        public static int Theme = 1;
        public static Color MenuColorT = SkyBlueTransparent;
        public static Color MenuColor = SkyBlue;
        public static Color ButtonColorOff = RoyalBlue;
        public static Color ButtonColorOn = DodgerBlue;
        public static Color outColor = DarkDodgerBlue;
        public static Color DisconnecyColor = Crimson;
        public static Color disOut = WineRed;
        public static void ChangeTheme()
        {
            Theme++;
            if (Theme > 6)
            {
                Theme = 1;
                MenuColorT = SkyBlueTransparent;
                MenuColor = SkyBlue;
                ButtonColorOff = RoyalBlue;
                ButtonColorOn = DodgerBlue;
                outColor = DarkDodgerBlue;
                DisconnecyColor = Crimson;
                disOut = WineRed;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Blue</color>");
                RefreshMenu();
            }
            if (Theme == 1)
            {
                MenuColorT = SkyBlueTransparent;
                MenuColor = SkyBlue;
                ButtonColorOff = RoyalBlue;
                ButtonColorOn = DodgerBlue;
                outColor = DarkDodgerBlue;
                DisconnecyColor = Crimson;
                disOut = WineRed;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Blue</color>");
                RefreshMenu();
            }
            if (Theme == 2)
            {
                MenuColorT = FireBrickTransparent;
                MenuColor = FireBrick;
                ButtonColorOff = WineRed;
                ButtonColorOn = IndianRed;
                outColor = IndianRed;
                DisconnecyColor = Crimson;
                disOut = WineRed;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Red</color>");
                RefreshMenu();
            }
            if (Theme == 3)
            {
                MenuColorT = new Color32(171, 129, 182, 80);
                MenuColor = new Color32(171, 129, 182, 255);
                ButtonColorOff = Plum;
                ButtonColorOn = MediumOrchid;
                outColor = DarkSlateBlue;
                DisconnecyColor = ButtonColorOff;
                disOut = outColor;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Lavendar</color>");
                RefreshMenu();
            }
            if (Theme == 4)
            {
                MenuColorT = MediumAquamarineTransparent;
                MenuColor = MediumAquamarine;
                ButtonColorOff = MediumSeaGreen;
                ButtonColorOn = SeaGreen;
                DisconnecyColor = ButtonColorOff;
                outColor = Lime;
                disOut = outColor;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Green</color>");
                RefreshMenu();
            }
            if (Theme == 5)
            {
                MenuColorT = BlackTransparent;
                MenuColor = Black;
                ButtonColorOff = DarkerGrey;
                ButtonColorOn = WineRed;
                DisconnecyColor = WineRed;
                outColor = DarkDodgerBlue;
                disOut = Maroon;
                NotificationLib.SendNotification("<color=white>[</color><color=blue>Theme</color><color=white>] Dark</color>");
                RefreshMenu();
            }
        }
        public static int ActuallSound = 66;
        public static int LOJUHFDG = 1;
        public static void ChangeSound()
        {
            LOJUHFDG++;
            if (LOJUHFDG > 6)
            {
                LOJUHFDG = 1;
                ActuallSound = 66;
            }
            if (LOJUHFDG == 1)
            {
                ActuallSound = 66;
            }
            if (LOJUHFDG == 2)
            {
                ActuallSound = 8;
            }
            if (LOJUHFDG == 3)
            {
                ActuallSound = 203;
            }
            if (LOJUHFDG == 4)
            {
                ActuallSound = 50;
            }
            if (LOJUHFDG == 5)
            {
                ActuallSound = 67;
            }
            if (LOJUHFDG == 6)
            {
                ActuallSound = 114;
            }
        }
        #endregion
        public static void AddDisconnectButton()
        {
            if (toggledisconnectButton)
            {
                // Disconnect Button
                disconnectButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(disconnectButton.GetComponent<Rigidbody>());
                disconnectButton.GetComponent<BoxCollider>().isTrigger = true;
                //RoundObj(disconnectButton);
                Outline(disconnectButton, disOut);
                disconnectButton.transform.parent = menuObj.transform;
                disconnectButton.transform.rotation = Quaternion.identity;
                disconnectButton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                disconnectButton.transform.localPosition = new Vector3(0.56f, 0f, 0.59f);
                disconnectButton.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button("DisconnectButton", Category.Home, false, false, null, null);
                disconnectButton.GetComponent<Renderer>().material.color = DisconnecyColor;

                // Disconnect Button Text
                Text discontext = new GameObject { transform = { parent = canvasObj.transform } }.AddComponent<Text>();
                discontext.text = "Disconnect";
                discontext.font = Font.CreateDynamicFontFromOSFont("Anton", 1);
                discontext.fontStyle = FontStyle.Bold;
                if (Theme == 3)
                {
                    discontext.color = Black;
                }
                else
                {
                    discontext.color = White;
                }
                discontext.alignment = TextAnchor.MiddleCenter;
                discontext.resizeTextForBestFit = true;
                discontext.resizeTextMinSize = 0;
                RectTransform rectt = discontext.GetComponent<RectTransform>();
                rectt.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                rectt.sizeDelta = new Vector2(0.13f, 0.023f);
                rectt.localPosition = new Vector3(0.064f, 0f, 0.227f);
                rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
        }
        private static void CreateMenuCanvasAndTitle()
        {
            // Menu Canvas
            canvasObj = new GameObject();
            canvasObj.transform.parent = menuObj.transform;
            canvasObj.name = "canvas";
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            CanvasScaler canvasScale = canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScale.dynamicPixelsPerUnit = 1000;

            // Menu Title
            GameObject titleObj = new GameObject();
            titleObj.transform.parent = canvasObj.transform;
            titleObj.transform.localScale = new Vector3(0.875f, 0.875f, 1f);
            title = titleObj.AddComponent<Text>();
            title.font = ResourceLoader.ArialFont;
            title.fontStyle = FontStyle.Bold;
            if (Theme == 3)
            {
                title.color = Black;
            }
            else
            {
                title.color = White;
            }

            title.fontSize = 7;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 0;
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            titleTransform.localPosition = Vector3.zero;
            titleTransform.position = new Vector3(0.07f, 0f, .16f);
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            titleTransform.sizeDelta = new Vector2(0.2f, 0.06f);
        }
        private static GameObject gameObject = new GameObject("Display");
        public static void Display()
        {
            if (gameObject == null)
            {
                gameObject = new GameObject("Display");
            }
            if (gameObject.GetComponent<TextMeshPro>() == null)
            {
                TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
                textMeshPro.fontSize = 2;
                textMeshPro.fontStyle = FontStyles.Bold;
                textMeshPro.characterSpacing = 1f;
                textMeshPro.alignment = TextAlignmentOptions.Center;
                textMeshPro.color = UnityEngine.Color.white;
                textMeshPro.text = "STATUS: <color=green>UND</color>\n<color=purple>Violet Paid</color>\nVERSION: <color=yellow>" + Initialization.PluginInfo.menuVersion.ToString() + "</color>";
            }

            gameObject.transform.position = new Vector3(-63.83f, 12.57f, -82.77f);
            gameObject.transform.LookAt(Camera.main.transform);
            gameObject.transform.Rotate(0f, 180f, 0f);
        }
        public static void UnDisplay()
        {
            GameObject.Destroy(gameObject);
        }
        public static bool Page = false;
        public static void AddTitleAndFPSCounter()
        {
            fps = (Time.deltaTime > 0) ? Mathf.RoundToInt(1.0f / Time.deltaTime) : 0;
            title.fontStyle = FontStyle.Bold;
            
            if (Page == true)
                title.text = 
                $"{menuName}\n" + 
                $"FPS: {fps} | Version: {menuVersion}";
            else
                title.text =
                $"{menuName} [Page: {currentCategoryPage + 1}]\n" +
                $"FPS: {fps} | Version: {menuVersion}";
            
        }
        public static void AddModButtons(float offset, ButtonHandler.Button button)
        {
            // Mod Buttons
            ModButton = ButtonPool.GetButton();
            Rigidbody btnRigidbody = ModButton.GetComponent<Rigidbody>();
            if (btnRigidbody != null)
            {
                Destroy(btnRigidbody);
            }
            BoxCollider btnCollider = ModButton.GetComponent<BoxCollider>();
            if (btnCollider != null)
            {
                btnCollider.isTrigger = true;
            }
            ModButton.transform.SetParent(menuObj.transform, false);
            ModButton.transform.rotation = Quaternion.identity;
            ModButton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            if (Laytou == 3)
            {
                ModButton.transform.localPosition = new Vector3(0.56f, 0f, 0.225f - offset);
            }
            else
            {
                ModButton.transform.localPosition = new Vector3(0.56f, 0f, 0.32f - offset);
            }
            BtnCollider btnColScript = ModButton.GetComponent<BtnCollider>() ?? ModButton.AddComponent<BtnCollider>();
            btnColScript.clickedButton = button;
            // Mod Buttons Text
            GameObject titleObj = TextPool.GetTextObject();
            titleObj.transform.SetParent(canvasObj.transform, false);
            titleObj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            Text title = titleObj.GetComponent<Text>();
            title.text = button.buttonText;
            title.font = ResourceLoader.ArialFont;
            title.fontStyle = FontStyle.Bold;
            if (Theme == 3)
            {
                title.color = Black;
            }
            else
            {
                title.color = White;
            }
            RectTransform titleTransform = title.GetComponent<RectTransform>();
            if (Laytou == 3)
            {
                titleTransform.localPosition = new Vector3(.064f, 0, .089f - offset / 2.6f);
            }
            else
            {
                titleTransform.localPosition = new Vector3(.064f, 0, .126f - offset / 2.6f);
            }
            titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            titleTransform.sizeDelta = new Vector2(0.21f, 0.02225f);

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(gameObject.GetComponent<Collider>());
            if (Theme == 0)
            {
                Outline(gameObject, DarkerGrey);
            }
            else
            {
                Outline(gameObject, outColor);
            }
            gameObject.transform.parent = menuObj.transform;
            gameObject.transform.position = ModButton.transform.position;
            gameObject.transform.rotation = ModButton.transform.rotation;
            gameObject.GetComponent<Renderer>().material.color = ModButton.GetComponent<Renderer>().material.color;
            gameObject.transform.localScale = new Vector3(ModButton.transform.localScale.x - 0.006f, ModButton.transform.localScale.y + 0.005f, ModButton.transform.localScale.z + 0.005f);

            Renderer btnRenderer = ModButton.GetComponent<Renderer>();
            if (btnRenderer != null)
            {
                if (button.Enabled)
                {
                    btnRenderer.material.color = ButtonColorOn;
                }
                else
                {
                    btnRenderer.material.color = ButtonColorOff;
                }
            }
        }
        public static int Laytou = 1;
        public static void ChangeMenuLayout()
        {
            Laytou++;
            RefreshMenu();
        }
        public static void AddPageButton(string button)
        {
            if (Laytou == 1)
            {
                PageButtons = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(PageButtons.GetComponent<Rigidbody>());
                PageButtons.GetComponent<BoxCollider>().isTrigger = true;
                //RoundObj(PageButtons);
                Outline(PageButtons, outColor);
                PageButtons.transform.parent = menuObj.transform;
                PageButtons.transform.rotation = Quaternion.identity;
                PageButtons.transform.localScale = new Vector3(0.09f, 0.15f, 0.9f);
                PageButtons.transform.localPosition = new Vector3(0.56f, button.Contains("<") ? 0.65f : -0.65f, -0);
                PageButtons.GetComponent<Renderer>().material.color = ButtonColorOff;
                PageButtons.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button(button, Category.Home, false, false, null, null);

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = PageButtons.transform.position;
                gameObject.transform.rotation = PageButtons.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(PageButtons.transform.localScale.x - 0.006f, PageButtons.transform.localScale.y + 0.005f, PageButtons.transform.localScale.z + 0.005f);

                // Page Buttons Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 5;
                title.fontStyle = FontStyle.Normal;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.5f, 0.06f);
                title.text = button.Contains("<") ? "⋘" : "⋙";
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                titleTransform.position = new Vector3(0.064f, button.Contains("<") ? 0.1955f : -0.1955f, 0f);
            }
            if (Laytou == 2)
            {
                // Page Buttons
                PageButtons = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(PageButtons.GetComponent<Rigidbody>());
                PageButtons.GetComponent<BoxCollider>().isTrigger = true; ;
                PageButtons.transform.parent = menuObj.transform;
                PageButtons.transform.rotation = Quaternion.identity;
                PageButtons.transform.localScale = new Vector3(0.09f, 0.25f, 0.079f);
                PageButtons.transform.localPosition = new Vector3(0.56f, button.Contains("<") ? 0.2925f : -0.2925f, -0.435f);
                PageButtons.GetComponent<Renderer>().material.color = ButtonColorOff;
                PageButtons.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button(button, Category.Home, false, false, null, null);

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = PageButtons.transform.position;
                gameObject.transform.rotation = PageButtons.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(PageButtons.transform.localScale.x - 0.006f, PageButtons.transform.localScale.y + 0.005f, PageButtons.transform.localScale.z + 0.005f);

                // Page Buttons Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 5;
                title.fontStyle = FontStyle.Normal;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.5f, 0.06f);
                title.text = button.Contains("<") ? "⋘" : "⋙";
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                titleTransform.position = new Vector3(0.064f, button.Contains("<") ? 0.087f : -.087f, -0.165f);
            }
            if (Laytou == 3)
            {
                // Page Buttons
                PageButtons = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(PageButtons.GetComponent<Rigidbody>());
                PageButtons.GetComponent<BoxCollider>().isTrigger = true; ;
                PageButtons.transform.parent = menuObj.transform;
                PageButtons.transform.rotation = Quaternion.identity;
                PageButtons.transform.localScale = new Vector3(0.09f, 0.25f, 0.079f);
                PageButtons.transform.localPosition = new Vector3(0.56f, button.Contains("<") ? 0.2925f : -0.2925f, 0.3223f);
                PageButtons.GetComponent<Renderer>().material.color = ButtonColorOff;
                PageButtons.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button(button, Category.Home, false, false, null, null);

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = PageButtons.transform.position;
                gameObject.transform.rotation = PageButtons.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(PageButtons.transform.localScale.x - 0.006f, PageButtons.transform.localScale.y + 0.005f, PageButtons.transform.localScale.z + 0.005f);

                // Page Buttons Text
                // Page Buttons Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 5;
                title.fontStyle = FontStyle.Normal;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.5f, 0.06f);
                title.text = button.Contains("<") ? "⋘" : "⋙";
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                titleTransform.position = new Vector3(0.064f, button.Contains("<") ? 0.087f : -.087f, 0.1235f);
            }
        }
        public static void AddReturnButton()
        {
            if (Laytou == 2)
            {
                // Return Button
                GameObject BackToStartButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(BackToStartButton.GetComponent<Rigidbody>());
                BackToStartButton.GetComponent<BoxCollider>().isTrigger = true;
                BackToStartButton.transform.parent = menuObj.transform;
                BackToStartButton.transform.rotation = Quaternion.identity;
                BackToStartButton.transform.localScale = new Vector3(0.09f, 0.30625f, 0.08f);
                BackToStartButton.transform.localPosition = new Vector3(0.56f, 0f, -0.435f);
                BackToStartButton.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button("ReturnButton", Category.Home, false, false, null, null);
                BackToStartButton.GetComponent<Renderer>().material.color = ButtonColorOff;

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = BackToStartButton.transform.position;
                gameObject.transform.rotation = BackToStartButton.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(BackToStartButton.transform.localScale.x - 0.006f, BackToStartButton.transform.localScale.y + 0.005f, BackToStartButton.transform.localScale.z + 0.005f);

                // Return Button Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                titleObj.transform.localPosition = new Vector3(0.85f, 0.85f, 0.85f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                title.fontStyle = FontStyle.Bold;
                title.text = "Return";
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 3;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.25f, 0.025f);
                titleTransform.localPosition = new Vector3(.064f, 0f, -0.165f);
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
            else if (Laytou == 3)
            {
                // Return Button
                GameObject BackToStartButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(BackToStartButton.GetComponent<Rigidbody>());
                BackToStartButton.GetComponent<BoxCollider>().isTrigger = true;
                BackToStartButton.transform.parent = menuObj.transform;
                BackToStartButton.transform.rotation = Quaternion.identity;
                BackToStartButton.transform.localScale = new Vector3(0.09f, 0.30625f, 0.08f);
                BackToStartButton.transform.localPosition = new Vector3(0.56f, 0f, 0.3223f);
                BackToStartButton.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button("ReturnButton", Category.Home, false, false, null, null);
                BackToStartButton.GetComponent<Renderer>().material.color = ButtonColorOff;

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = BackToStartButton.transform.position;
                gameObject.transform.rotation = BackToStartButton.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(BackToStartButton.transform.localScale.x - 0.006f, BackToStartButton.transform.localScale.y + 0.005f, BackToStartButton.transform.localScale.z + 0.005f);

                // Return Button Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                titleObj.transform.localPosition = new Vector3(0.85f, 0.85f, 0.85f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                title.fontStyle = FontStyle.Bold;
                title.text = "Return";
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 3;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.25f, 0.025f);
                titleTransform.localPosition = new Vector3(.064f, 0f, 0.1235f);
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
            else
            {
                // Return Button
                GameObject BackToStartButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(BackToStartButton.GetComponent<Rigidbody>());
                BackToStartButton.GetComponent<BoxCollider>().isTrigger = true;
                BackToStartButton.transform.parent = menuObj.transform;
                BackToStartButton.transform.rotation = Quaternion.identity;
                BackToStartButton.transform.localScale = new Vector3(0.09f, 0.82f, 0.08f);
                BackToStartButton.transform.localPosition = new Vector3(0.56f, 0f, -0.435f);
                BackToStartButton.AddComponent<BtnCollider>().clickedButton = new ButtonHandler.Button("ReturnButton", Category.Home, false, false, null, null);
                BackToStartButton.GetComponent<Renderer>().material.color = ButtonColorOff;

                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(gameObject.GetComponent<Collider>());
                if (Theme == 0)
                {
                    Outline(gameObject, DarkerGrey);
                }
                else
                {
                    Outline(gameObject, outColor);
                }
                gameObject.transform.parent = menuObj.transform;
                gameObject.transform.position = BackToStartButton.transform.position;
                gameObject.transform.rotation = BackToStartButton.transform.rotation;
                gameObject.GetComponent<Renderer>().material.color = ButtonColorOff;
                gameObject.transform.localScale = new Vector3(BackToStartButton.transform.localScale.x - 0.006f, BackToStartButton.transform.localScale.y + 0.005f, BackToStartButton.transform.localScale.z + 0.005f);

                // Return Button Text
                GameObject titleObj = new GameObject();
                titleObj.transform.parent = canvasObj.transform;
                titleObj.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                titleObj.transform.localPosition = new Vector3(0.85f, 0.85f, 0.85f);
                Text title = titleObj.AddComponent<Text>();
                title.font = ResourceLoader.ArialFont;
                title.fontStyle = FontStyle.Bold;
                title.text = "Return";
                if (Theme == 3)
                {
                    title.color = Black;
                }
                else
                {
                    title.color = White;
                }
                title.fontSize = 3;
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 0;
                RectTransform titleTransform = title.GetComponent<RectTransform>();
                titleTransform.localPosition = Vector3.zero;
                titleTransform.sizeDelta = new Vector2(0.25f, 0.025f);
                titleTransform.localPosition = new Vector3(.064f, 0f, -0.165f);
                titleTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
            }
        }

        public static void AddButtonClicker(Transform parentTransform)
        {
            // Button Clicker
            if (clickerObj == null)
            {
                clickerObj = new GameObject("buttonclicker");
                BoxCollider clickerCollider = clickerObj.AddComponent<BoxCollider>();
                if (clickerCollider != null)
                {
                    clickerCollider.isTrigger = true;
                }
                MeshFilter meshFilter = clickerObj.AddComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                }
                Renderer clickerRenderer = clickerObj.AddComponent<MeshRenderer>();
                if (clickerRenderer != null)
                {
                    clickerRenderer.material.color = White;
                    clickerRenderer.material.shader = Shader.Find("GUI/Text Shader");
                }
                if (parentTransform != null)
                {
                    clickerObj.transform.parent = parentTransform;
                    clickerObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    clickerObj.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                }
            }
        }
        public static bool bark = false;
        private static void PositionMenuForHand()
        {
            if (bark)
            {
                menuObj.transform.position = GorillaTagger.Instance.headCollider.transform.position + GorillaTagger.Instance.headCollider.transform.forward * 0.5f + GorillaTagger.Instance.headCollider.transform.up * -0.1f;
                menuObj.transform.LookAt(GorillaTagger.Instance.headCollider.transform);
                Vector3 rotModify = menuObj.transform.rotation.eulerAngles;
                rotModify += new Vector3(-90f, 0f, -90f);
                menuObj.transform.rotation = Quaternion.Euler(rotModify);
            }
            else if (rightHandedMenu)
            {
                menuObj.transform.position = playerInstance.rightControllerTransform.position;
                Vector3 rotation = playerInstance.rightControllerTransform.rotation.eulerAngles;
                rotation += new Vector3(0f, 0f, 180f);
                menuObj.transform.rotation = Quaternion.Euler(rotation);
            }
            else
            {
                menuObj.transform.position = playerInstance.leftControllerTransform.position;
                menuObj.transform.rotation = playerInstance.leftControllerTransform.rotation;
            }
        }
        private static void PositionMenuForKeyboard()
        {
            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.transform.position = new Vector3(-65.8373f, 21.6568f, -80.9763f);
                thirdPersonCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                menuObj.transform.SetParent(thirdPersonCamera.transform, true);

                Vector3 headPosition = thirdPersonCamera.transform.position;
                Quaternion headRotation = thirdPersonCamera.transform.rotation;
                float offsetDistance = 0.65f;
                Vector3 offsetPosition = headPosition + headRotation * Vector3.forward * offsetDistance;
                menuObj.transform.position = offsetPosition;

                Vector3 directionToHead = headPosition - menuObj.transform.position;
                menuObj.transform.rotation = Quaternion.LookRotation(directionToHead, Vector3.up);
                menuObj.transform.Rotate(Vector3.up, -90.0f);
                menuObj.transform.Rotate(Vector3.right, -90.0f);
            }
        }

        public static void AddRigidbodyToMenu()
        {
            if (currentMenuRigidbody == null && menuObj != null)
            {
                currentMenuRigidbody = menuObj.GetComponent<Rigidbody>();
                if (currentMenuRigidbody == null)
                {
                    currentMenuRigidbody = menuObj.AddComponent<Rigidbody>();
                }
                currentMenuRigidbody.useGravity = true;
            }
        }
    }
}
