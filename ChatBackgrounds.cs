using SML;
using HarmonyLib;
using UnityEngine;
using Game.Chat;
using UnityEngine.UI;
using System.Collections.Generic;
using Server.Shared.Extensions;
using Game.Interface;
using System.Reflection;
using System;

namespace ChatBackgrounds;

public enum BackgroundType
{
    Chatbox,
    Chatlog
}

[Mod.SalemMod]
public class ChatBackgrounds
{
    public static void Start()
    {
        Debug.Log("ChatBackgrounds works!");
        FileUtils.OnStart();
        BackgroundManager.LoadNewSprite(BackgroundType.Chatbox);
        BackgroundManager.LoadNewSprite(BackgroundType.Chatlog);
    }
}

[Mod.SalemMenuItem]
public class MenuItem
{
   public static Mod.SalemMenuButton menuButtonName = new()
   {
      Label = "Chat Backgrounds",
      OnClick = FileUtils.OpenBackgroundsDir
   };
}

[DynamicSettings]
public class Settings
{
    public ModSettings.DropdownSetting ChatboxPMode
    {
        get
        {
            ModSettings.DropdownSetting ChatboxPMode = new()
            {
                Name = "BG Scaling Pivot (chatbox)",
                Description = "Determines which part of the BG remains visible when scaled to fill the chatbox",
                Options = new(){"Centre", "Bottom", "Top", "Left", "Right"},
                AvailableInGame = true,
                OnChanged = _ => BackgroundManager.UpdateImagePivot()
            };
            return ChatboxPMode;
        }
    }
    public ModSettings.DropdownSetting ChatlogPMode
    {
        get
        {
            ModSettings.DropdownSetting ChatlogPmode = new()
            {
                Name = "BG Scaling Pivot (chatlog)",
                Description = "Determines which part of the BG remains visible when scaled to fill the chatlog",
                Options = new(){"Centre", "Bottom", "Top", "Left", "Right"},
                AvailableInGame = true,
                OnChanged = _ => BackgroundManager.UpdateImagePivot()
            };
            return ChatlogPmode;
        }
    }
    public ModSettings.DropdownSetting SelectedBackground
    {
        get
        {
            ModSettings.DropdownSetting SelectedBackground = new()
            {
                Name = "Chatbox Background",
                Description = "The background to be applied to your chatbox!",
                Options = FileUtils.GetBackgroundOptions(),
                AvailableInGame = false,
                OnChanged = (s) => {
                    FileUtils.SelectBackground(s, BackgroundType.Chatbox);
                    BackgroundManager.LoadNewSprite(BackgroundType.Chatbox);
                }
            };
            return SelectedBackground;
        }
    }
    public ModSettings.DropdownSetting SelectedChatlogBackground
    {
        get
        {
            ModSettings.DropdownSetting SelectedChatlogBackground = new()
            {
                Name = "Chatlog Background",
                Description = "The background to be applied to your chatlog",
                Options = FileUtils.GetBackgroundOptions(),
                AvailableInGame = false,
                OnChanged = (s) => {
                    FileUtils.SelectBackground(s, BackgroundType.Chatlog);
                    BackgroundManager.LoadNewSprite(BackgroundType.Chatlog);
                }
            };
            return SelectedChatlogBackground;
        }
    }
    public ModSettings.IntegerInputSetting BackgroundTransparency
    {
        get
        {
            ModSettings.IntegerInputSetting BackgroundTransparency = new()
            {
                Name = "BG Transparency (chatbox)",
                Description = "The transparency of the chatbox background. 0 = opaque, 100 = fully transparent.",
                DefaultValue = 20,
                MinValue = 0,
                MaxValue = 100,
                OnChanged = _ => BackgroundManager.UpdateImageColour() //allow player to change background properties midgame
            };
            return BackgroundTransparency;
        }
    }
    public ModSettings.IntegerInputSetting ChatlogBackgroundTransparency
    {
        get
        {
            ModSettings.IntegerInputSetting ChatlogBackgroundTransparency = new()
            {
                Name = "BG Transparency (chatlog)",
                Description = "The transparency of the chatlog background. 0 = opaque, 100 = fully transparent.",
                DefaultValue = 20,
                MinValue = 0,
                MaxValue = 100,
                OnChanged = _ => BackgroundManager.UpdateImageColour() //allow player to change background properties midgame
            };
            return ChatlogBackgroundTransparency;
        }
    }
    public ModSettings.IntegerInputSetting BackgroundDarkness
    {
        get
        {
            ModSettings.IntegerInputSetting BackgroundDarkness = new()
            {
              Name = "BG Darkness (chatbox)",
              Description = "The darkness of the chatbox background. 0 = normal image brightness, 100 = black",
              DefaultValue = 20,
              MinValue = 0,
              MaxValue = 100,
              OnChanged = _ => BackgroundManager.UpdateImageColour()
            };
            return BackgroundDarkness;
        }
    }

    public ModSettings.IntegerInputSetting ChatlogBackgroundDarkness
    {
        get
        {
            ModSettings.IntegerInputSetting ChatlogBackgroundDarkness = new()
            {
              Name = "BG Darkness (chatlog)",
              Description = "The darkness of the chatlog background. 0 = normal image brightness, 100 = black",
              DefaultValue = 20,
              MinValue = 0,
              MaxValue = 100,
              OnChanged = _ => BackgroundManager.UpdateImageColour()
            };
            return ChatlogBackgroundDarkness;
        }
    }

    public ModSettings.CheckboxSetting KeepBacking
    {
        get
        {
            ModSettings.CheckboxSetting KeepBacking = new()
            {
                Name = "Keep Panel Backing As Overlay?",
                Description = "If enabled, the backing of the chatbox/chatlog panel will be overlayed on your background",
                DefaultValue = true,
                AvailableInGame = true,
                OnChanged = (b) => BackgroundManager.UpdatePanelBackingState(b)
            };
            return KeepBacking;
        }
    }
}

[HarmonyPatch(typeof(PooledChatViewSwitcher))]
class ViewSwitcherPatch
{
    [HarmonyPatch("SetViewToChatOnly")]
    [HarmonyPostfix]
    static void SwitchToChatboxBG()
    {
        BackgroundManager.SwitchBackground(BackgroundType.Chatbox);
    }

    [HarmonyPatch("SetViewToChatLog")]
    [HarmonyPostfix]
    static void SwitchToChatlogBG()
    {
        BackgroundManager.SwitchBackground(BackgroundType.Chatlog);
    }
}

[HarmonyPatch]
class BackgroundManager
{
    static GameObject bgContainerObject = null;
    static Image bgImage = null;
    static Dictionary<BackgroundType, Sprite> bgImageSprites = new()
    {
        {BackgroundType.Chatbox, null},
        {BackgroundType.Chatlog, null},
    };

    static readonly Dictionary<string, Vector2> pivots = new() 
    {
        {"Centre", new Vector2(0.5f, 0.5f)},
        {"Bottom", new Vector2(0.5f, 0f)},
        {"Top", new Vector2(0.5f, 1f)},
        {"Left", new Vector2(0f, 0.5f)},
        {"Right", new Vector2(1f, 0.5f)},
    };

    static BackgroundType activeBgType;

    static GameObject panelBackingObject = null;

    static MethodBase TargetMethod() 
    {
        return AccessTools.Method(
            typeof(GameCanvasManager),
            "ManageUi",
            new Type[]
            {
                typeof(bool),
                typeof(GameCanvas),
                typeof(GameCanvas).MakeByRefType()
            });
    }

    //todo: this should be separated to a different class/cs file alongside other patches
    [HarmonyPostfix]
    static void AttachBackground(GameCanvasManager __instance, bool shouldShowUi, GameCanvas template, ref GameCanvas instance)
    {
        if (!shouldShowUi || instance == null)
            return;

        if (!ReferenceEquals(template, __instance.GameCanvases.PooledChatElementsCanvas))
            return;

        Debug.Log("ChatBG: attaching");
        
        Transform upperChatContents = instance.transform.Find("Background/ChatContents/ChatUpperContents");
        if (upperChatContents == null)
        {
             Debug.Log("ChatBG: Unable to attach background: Chat panel contents gameobject not found");
             return;
        }

        activeBgType = BackgroundType.Chatbox;

        Transform panelBacking = upperChatContents.GetChild(0);
        panelBackingObject = panelBacking.gameObject;

        bgContainerObject = UnityEngine.Object.Instantiate(panelBackingObject, upperChatContents);
        bgContainerObject.name = "ChatBGContainer";
        bgContainerObject.transform.SetAsFirstSibling();

        bgContainerObject.AddComponent<RectMask2D>();   //container object is needed to mask the background when it scales beyond the size of a panel, 

        Image originalImage = bgContainerObject.GetComponent<Image>();
        originalImage.enabled = false;

        GameObject bgImageObject = new GameObject("ChatBG");
        bgImageObject.transform.SetParent(bgContainerObject.transform, false);

        bgImage = bgImageObject.AddComponent<Image>();
        bgImage.raycastTarget = false;

        RectTransform rt = bgImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        UpdateImagePivot();

        AspectRatioFitter fitter = bgImageObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

        bgImage.sprite = bgImageSprites.GetValue(BackgroundType.Chatbox);
        fitter.aspectRatio = bgImage.sprite.rect.width / bgImage.sprite.rect.height;

        UpdateImageColour();

        if (bgImage.sprite != null)
        {
            panelBackingObject.SetActive(ModSettings.GetBool("Keep Panel Backing?", "Silph5.chatbackgrounds"));
        }
        else
        {
            bgContainerObject.SetActive(false);
        }

    }
    public static void UpdateImagePivot()
    {
        if (bgImage == null) return;

        switch (activeBgType)
        {
            case BackgroundType.Chatbox:
            
                bgImage.rectTransform.pivot = pivots[ModSettings.GetString("BG Scaling Pivot (chatbox)", "Silph5.chatbackgrounds")];
                break;

            case BackgroundType.Chatlog:

                bgImage.rectTransform.pivot = pivots[ModSettings.GetString("BG Scaling Pivot (chatlog)", "Silph5.chatbackgrounds")];
                break;
        }
    }

    public static void UpdateImageColour()
    {
        if (bgImage == null) return;

        float darkness;
        switch (activeBgType)
        {
            case BackgroundType.Chatbox:
            
                darkness = 1f - ModSettings.GetInt("BG Darkness (chatbox)", "Silph5.chatbackgrounds") / 100f;
                bgImage.color = new Color (
                    darkness,
                    darkness,
                    darkness,
                    1 - (ModSettings.GetInt("BG Transparency (chatbox)", "Silph5.chatbackgrounds") / 100f)
                );
                break;

            case BackgroundType.Chatlog:

                darkness = 1f - ModSettings.GetInt("BG Darkness (chatlog)", "Silph5.chatbackgrounds") / 100f;
                bgImage.color = new Color (
                    darkness,
                    darkness,
                    darkness,
                    1 - (ModSettings.GetInt("BG Transparency (chatlog)", "Silph5.chatbackgrounds") / 100f)
                );
                break;
        }
    }

    public static void UpdatePanelBackingState(bool setting)
    {
        if (panelBackingObject == null) return;

        panelBackingObject.SetActive(setting);
    }

    public static void LoadNewSprite(BackgroundType spriteType)
    {
        if (bgImageSprites[spriteType] != null) { //don't leak memory
            UnityEngine.Object.Destroy(bgImageSprites[spriteType].texture); 
            UnityEngine.Object.Destroy(bgImageSprites[spriteType]);
            bgImageSprites[spriteType] = null;
        }
        string selectedBackgroundPath = FileUtils.GetSelectedBackground(spriteType);
        if (selectedBackgroundPath != "No Background") {
            bgImageSprites[spriteType] = IMG2Sprite.LoadNewSprite(selectedBackgroundPath);
        }
    }

    public static void SwitchBackground(BackgroundType type)
    {
        activeBgType = type;

        Sprite newSprite = bgImageSprites[type];
        if (newSprite == null)
        {
            bgContainerObject.SetActive(false);
            panelBackingObject.SetActive(true);
            return;
        }

        UpdateImageColour();
        UpdateImagePivot();

        bgImage.sprite = newSprite;
        panelBackingObject.SetActive(ModSettings.GetBool("Keep Panel Backing?", "Silph5.chatbackgrounds"));
        
        var fitter = bgImage.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = newSprite.rect.width / newSprite.rect.height;
    }
}

