using HarmonyLib;
using Game.Interface;
using System.Reflection;
using System;
using Mono.Cecil.Cil;

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

    [HarmonyPrefix]
    static void CheckIfNew(ref bool __state, GameCanvasManager __instance, bool shouldShowUi, GameCanvas template, ref GameCanvas instance)
    {
        __state = (instance == null);
    }

    [HarmonyPostfix]
    static void AttachBackground(GameCanvasManager __instance, bool __state, bool shouldShowUi, GameCanvas template, ref GameCanvas instance)
    {
        if (!shouldShowUi || instance == null || !__state)
            return;

        if (ReferenceEquals(template, __instance.GameCanvases.PooledChatElementsCanvas))
        {
            ChatBackgroundManager.AttachBackground(instance.transform.Find("Background/ChatContents/ChatUpperContents"));
            return;
        }

        if (ReferenceEquals(template, __instance.GameCanvases.RoleListAndGraveyardElementsCanvas))
        {
            var panel = instance.transform.Find("MainCanvasGroup/MainPanel/RoleListAndGraveyardPanel");
            RolelistBackgroundManager.AttachBackground(panel);
            GraveyardBackgroundManager.AttachBackground(panel);
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