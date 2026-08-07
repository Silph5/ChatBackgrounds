using SML;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Server.Shared.Extensions;

namespace ChatBackgrounds;

struct Constants
{
    public static readonly Dictionary<string, Vector2> pivots = new() 
    {
        {"Centre", new Vector2(0.5f, 0.5f)},
        {"Bottom", new Vector2(0.5f, 0f)},
        {"Top", new Vector2(0.5f, 1f)},
        {"Left", new Vector2(0f, 0.5f)},
        {"Right", new Vector2(1f, 0.5f)},
    };
}

class SpritesManager
{
    public static Dictionary<BackgroundType, Sprite> bgImageSprites = new()
    {
        {BackgroundType.Chatbox, null},
        {BackgroundType.Chatlog, null},
    };

    public static void LoadNewSprite(BackgroundType spriteType)
    {
        if (bgImageSprites[spriteType] != null) { //don't leak memory
            Object.Destroy(bgImageSprites[spriteType].texture); 
            Object.Destroy(bgImageSprites[spriteType]);
            bgImageSprites[spriteType] = null;
        }
        string selectedBackgroundPath = FileUtils.GetSelectedBackground(spriteType);
        if (selectedBackgroundPath != "No Background") {
            bgImageSprites[spriteType] = IMG2Sprite.LoadNewSprite(selectedBackgroundPath);
        }
    }
}

class ChatBackgroundManager
{
    static GameObject bgContainerObject = null;
    static Image bgImage = null;
    static BackgroundType activeBgType;

    static GameObject panelBackingObject = null;

    public static void AttachBackground(Transform upperChatContents)
    {
        Debug.Log("ChatBG: Attaching background to chatbox");
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

        bgImage.sprite = SpritesManager.bgImageSprites.GetValue(BackgroundType.Chatbox);
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
            
                bgImage.rectTransform.pivot = Constants.pivots[ModSettings.GetString("BG Scaling Pivot (chatbox)", "Silph5.chatbackgrounds")];
                break;

            case BackgroundType.Chatlog:

                bgImage.rectTransform.pivot = Constants.pivots[ModSettings.GetString("BG Scaling Pivot (chatlog)", "Silph5.chatbackgrounds")];
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

    public static void SwitchBackground(BackgroundType type)
    {
        activeBgType = type;

        Sprite newSprite = SpritesManager.bgImageSprites[type];
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