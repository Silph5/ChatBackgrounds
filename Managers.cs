using SML;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Server.Shared.Extensions;
using System.ComponentModel;

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
        {BackgroundType.Rolelist, null},
        {BackgroundType.Graveyard, null}
    };

    public static void LoadNewSprite(BackgroundType spriteType)
    {
        BackgroundType duplicateType = FileUtils.getDuplicateUse(spriteType);
        
        if (bgImageSprites[spriteType] != null && duplicateType == BackgroundType.None) { //don't leak memory
            Object.Destroy(bgImageSprites[spriteType].texture); 
            Object.Destroy(bgImageSprites[spriteType]);
            bgImageSprites[spriteType] = null;
        }
        
        string selectedBackgroundPath = FileUtils.GetSelectedBackground(spriteType);
        if (selectedBackgroundPath != "No Background") {
            //trying to reuse background sprites to avoid unnecessary memory use
            //untested due to tos2 ddos
            if (duplicateType != BackgroundType.None)
            {
                bgImageSprites[spriteType] = bgImageSprites[duplicateType];
                return;
            }

            bgImageSprites[spriteType] = IMG2Sprite.LoadNewSprite(selectedBackgroundPath);
        }
    }
}

class BgImageObjectMaker
{
    public static GameObject MakeImageObject(BackgroundType type, GameObject container)
    {
        GameObject bgImageObject = new GameObject("customBG");
        bgImageObject.transform.SetParent(container.transform, false);

        Image bgImage = bgImageObject.AddComponent<Image>();
        bgImage.raycastTarget = false;

        RectTransform rt = bgImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        AspectRatioFitter fitter = bgImageObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

        bgImage.sprite = SpritesManager.bgImageSprites.GetValue(type);
        fitter.aspectRatio = bgImage.sprite.rect.width / bgImage.sprite.rect.height;

        return bgImageObject;
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

        bgContainerObject = Object.Instantiate(panelBackingObject, upperChatContents);
        bgContainerObject.name = "ChatBGContainer";
        bgContainerObject.transform.SetAsFirstSibling();

        bgContainerObject.AddComponent<RectMask2D>();   //container object is needed to mask the background when it scales beyond the size of a panel, 

        Image originalImage = bgContainerObject.GetComponent<Image>();
        originalImage.enabled = false;

        GameObject bgImageObject = BgImageObjectMaker.MakeImageObject(BackgroundType.Chatbox, bgContainerObject);
        bgImage = bgImageObject.GetComponent<Image>();
        UpdateImagePivot();
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
        bgContainerObject.SetActive(true);

        UpdateImageColour();
        UpdateImagePivot();

        bgImage.sprite = newSprite;
        panelBackingObject.SetActive(ModSettings.GetBool("Keep Panel Backing?", "Silph5.chatbackgrounds"));
        
        var fitter = bgImage.GetComponent<AspectRatioFitter>();
        fitter.aspectRatio = newSprite.rect.width / newSprite.rect.height;
    }
}

class RolelistBackgroundManager
{
    static GameObject bgContainerObject = null;
    static Image bgImage = null;

    public static void AttachBackground(Transform Panel)
    {
        Debug.Log("ChatBG: Attaching background to Rolelist");
        if (Panel == null)
        {
            Debug.Log("ChatBG: Unable to attach background: Rolelist+gy panel gameobject not found");
            return;
        }

        bgContainerObject = new GameObject("BGContainer");
        bgContainerObject.transform.SetParent(Panel);
        bgContainerObject.transform.SetAsFirstSibling();
        bgContainerObject.AddComponent<RectMask2D>();
        
        //go in and manually figure out what the values need to be because unity scaling is a bitch
        RectTransform containerTransform = bgContainerObject.AddComponent<RectTransform>();
        containerTransform.anchorMin = new Vector2(0f, 1f);
        containerTransform.anchorMax = new Vector2(1f, 0f);
        containerTransform.pivot = new Vector2(0f, 1f);
        containerTransform.anchoredPosition = new Vector2(10f, 10f);
        containerTransform.sizeDelta = new Vector2(10f, 10f);

        GameObject bgImageObject = BgImageObjectMaker.MakeImageObject(BackgroundType.Rolelist, bgContainerObject);
        bgImage = bgImageObject.GetComponent<Image>();


    }

}

class GraveyardBackgroundManager
{
    static GameObject bgContainerObject = null;
    static Image bgImage = null;

    public static void AttachBackground(Transform RolelistAndGraveyardPanel)
    {
        Debug.Log("ChatBG: Attaching background to Graveyard");
        if (RolelistAndGraveyardPanel == null)
        {
            Debug.Log("ChatBG: Unable to attach background: Rolelist+gy panel gameobject not found");
            return;
        }
    }
}