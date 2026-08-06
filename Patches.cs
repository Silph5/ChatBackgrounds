using SML;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Server.Shared.Extensions;
using Game.Interface;
using System.Reflection;
using System;

namespace ChatBackgrounds;

[HarmonyPatch]
class CanvasManagerPatch
{
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

    [HarmonyPostfix]
    static void AttachBackground(GameCanvasManager __instance, bool shouldShowUi, GameCanvas template, ref GameCanvas instance)
    {
        if (!shouldShowUi || instance == null)
            return;

        if (ReferenceEquals(template, __instance.GameCanvases.PooledChatElementsCanvas))
        {
            ChatBackgroundManager.AttachBackground(instance.transform.Find("Background/ChatContents/ChatUpperContents"));
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
        ChatBackgroundManager.SwitchBackground(BackgroundType.Chatbox);
    }

    [HarmonyPatch("SetViewToChatLog")]
    [HarmonyPostfix]
    static void SwitchToChatlogBG()
    {
        ChatBackgroundManager.SwitchBackground(BackgroundType.Chatlog);
    }
}