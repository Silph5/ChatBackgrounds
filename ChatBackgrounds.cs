using SML;
using UnityEngine;

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
        SpritesManager.LoadNewSprite(BackgroundType.Chatbox);
        SpritesManager.LoadNewSprite(BackgroundType.Chatlog);
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
                OnChanged = _ => ChatBackgroundManager.UpdateImagePivot()
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
                OnChanged = _ => ChatBackgroundManager.UpdateImagePivot()
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
                    SpritesManager.LoadNewSprite(BackgroundType.Chatbox);
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
                    SpritesManager.LoadNewSprite(BackgroundType.Chatlog);
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
                OnChanged = _ => ChatBackgroundManager.UpdateImageColour() //allow player to change background properties midgame
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
                OnChanged = _ => ChatBackgroundManager.UpdateImageColour() //allow player to change background properties midgame
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
              OnChanged = _ => ChatBackgroundManager.UpdateImageColour()
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
              OnChanged = _ => ChatBackgroundManager.UpdateImageColour()
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
                OnChanged = (b) => ChatBackgroundManager.UpdatePanelBackingState(b)
            };
            return KeepBacking;
        }
    }
}

