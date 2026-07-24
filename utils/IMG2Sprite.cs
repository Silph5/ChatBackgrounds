// https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
// stolen without shame

using UnityEngine;
using System.IO;
 
namespace ChatBackgrounds;
public class IMG2Sprite : MonoBehaviour
{
    private static IMG2Sprite _instance;
 
    public static IMG2Sprite instance
    {
        get
        {
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<IMG2Sprite>();
            return _instance;
        }
    }
 
    static public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {      
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
 
        return NewSprite;
    }
 
    static public Texture2D LoadTexture(string FilePath)
    {
 
        Texture2D Tex2D;
        byte[] FileData;
 
        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);         
            if (Tex2D.LoadImage(FileData))
            
                Tex2D.Compress(false);

                return Tex2D;                 
        }
        return null;                     
    }
}