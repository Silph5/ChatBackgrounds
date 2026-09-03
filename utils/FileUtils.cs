using System;
using System.Collections.Generic;
using System.IO;
using Server.Shared.Extensions;
using SML;
using UnityEngine;

namespace ChatBackgrounds;

//JAN's soundpack mod helped a lot figuring out how to handle file stuff. This mod is significantly simpler, though.
//https://github.com/JustAnotherNoob3/The-Soundpack-Mod/blob/main/Utils/SoundpackUtils.cs

public static class FileUtils
{
    private static string directoryPath;
    private static Dictionary<string, string> backgrounds = new();
    private static Dictionary<BackgroundType, string> selectedBackgroundPaths = new()
    {
        {BackgroundType.Chatbox, "No Background"},
        {BackgroundType.Chatlog, "No Background"},
        {BackgroundType.Rolelist, "No Background"},
        {BackgroundType.Graveyard, "No Background"},
    };

    public static void OnStart()
    {
        directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "SalemModLoader", "ModFolders", "ChatBackgrounds");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("chatBG: modfolder missing, created new modfolder");
            return;
        }

        foreach (string file in Directory.GetFiles(directoryPath))
        {
            string type = file.Substring(file.Length - 4);
            //Debug.Log("chatBG: file: " + file + "" + type);
            if (type != ".png" && type != ".jpg")
            {
                continue;
            }

            backgrounds.Add(Path.GetFileName(file), file);
        }

        SelectBackground(ModSettings.GetString("Chatbox Background", "Silph5.chatbackgrounds"), BackgroundType.Chatbox);
        SelectBackground(ModSettings.GetString("Chatlog Background", "Silph5.chatbackgrounds"), BackgroundType.Chatlog);
    }

    public static List<string> GetBackgroundOptions()
    {
        List<string> options = new()
        {
            "No Background"
        };

        foreach (KeyValuePair<string, string> pair in backgrounds)
        {
            options.Add(pair.Key);
        }

        return options;
    }

    public static void SelectBackground(string choice, BackgroundType type)
    {
        if (choice == "No Background")
        {
            selectedBackgroundPaths[type] = choice;
            return;
        }
        selectedBackgroundPaths[type] = backgrounds.GetValue(choice);
        Debug.Log("chatBG: selected " + selectedBackgroundPaths[type]);
    }

    public static string GetSelectedBackground(BackgroundType type)
    {
        return selectedBackgroundPaths[type];
    }

    public static void OpenBackgroundsDir()
    {
        string text = Path.Combine(Directory.GetCurrentDirectory(), "SalemModLoader", "ModFolders", "ChatBackgrounds");
        if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
        {
            System.Diagnostics.Process.Start("open", "\"" + text + "\""); //code stolen from JAN who stole it from tuba
        }
        else
        {
            Application.OpenURL("file://" + text);
        }

    }

    public static BackgroundType getDuplicateUse(string path)
    {
        if (path == "No Background")
        {
            return BackgroundType.None;
        }

        foreach(var pair in selectedBackgroundPaths)
        {
            if (pair.Value == path)
            {
                return pair.Key;
            }
        }
        return BackgroundType.None;
    }
}