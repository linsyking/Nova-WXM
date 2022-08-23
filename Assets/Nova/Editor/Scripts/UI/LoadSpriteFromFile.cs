using System.Collections;
using UnityEngine;
using System.IO;

namespace Nova.Editor
{
    public class LoadSpriteFromFile : MonoBehaviour
    {

        public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
        {

            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            var fpath = Path.Combine("Assets/NewResources", FilePath);
            var rpath = "";
            if (File.Exists(fpath + ".jpg"))
            {
                rpath = (fpath + ".jpg");

            }
            else if (File.Exists(fpath + ".png"))
            {
                rpath = (fpath + ".png");
            }
            else
            {
                Debug.LogError($"Not Found {fpath}.");
                return null;
            }

            Texture2D SpriteTexture = LoadTexture(rpath);
            if (SpriteTexture == null)
            {
                return null;
            }
            Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

            return NewSprite;
        }

        private static Texture2D LoadTexture(string FilePath)
        {

            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                    return Tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }
    }
}
