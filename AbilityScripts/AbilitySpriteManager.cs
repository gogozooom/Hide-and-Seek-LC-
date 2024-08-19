using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Debug = Debugger.Debug;

namespace HideAndSeek.AbilityScripts
{
    public static class AbilitySpriteManager
    {
        public static List<Sprite> abilityUISprites;

        public static Sprite nullSprite = null;

        public static void LoadSprites()
        {
            abilityUISprites = new();
            nullSprite = null;

            var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
            var abilityDirectory = Path.Combine(dllFolderPath, "Abilities");

            if (!Directory.Exists(abilityDirectory))
            {
                Directory.CreateDirectory(abilityDirectory);
            }

            var files = Directory.GetFiles(dllFolderPath, "*.png", SearchOption.AllDirectories);

            Debug.LogWarning($"Now loading all ability sprites! Path = '{dllFolderPath}', Files found = '{files.Length}'");
            foreach (var fName in files)
            {
                //Debug.Log($"Trying to load sprite '{fName}'");
                byte[] fileData = File.ReadAllBytes(fName);

                var splitString = fName.Split("\\");
                var fileName = splitString[splitString.Length - 1];
                var abilityName = fileName.Replace("Ability - ", "").Replace("Sprite - ", "").Replace(".png", "");


                if (fileData != null)
                {
                    Texture2D tex = new Texture2D(1200, 1200, TextureFormat.RGB24, false);

                    if (tex.LoadImage(fileData))
                    {
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new(0.5f, 0.5f));

                        sprite.name = abilityName;

                        if (sprite.name == "[Null]")
                        {
                            tex.filterMode = FilterMode.Point;
                            Debug.LogError("Null sprite initalized!");
                            nullSprite = sprite;
                        }
                        else
                            abilityUISprites.Add(sprite);

                        Debug.Log($"Loaded sprite name '{sprite.name}'");
                    }

                }


                if (abilityName == "icon" || abilityName == "old-icon")
                {
                    continue;
                }
                File.Move(fName, Path.Combine(abilityDirectory, fileName));
            }
            Debug.LogWarning($"All Sprites Loaded!");
        }

        public static Sprite GetSprite(string name)
        {
            Sprite found = null;

            foreach (var sprite in abilityUISprites)
            {
                if (string.Equals(sprite.name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    found = sprite;
                }
            }

            if (found == null)
            {
                if (!nullSprite)
                {
                    Debug.LogError("GetSpite(); No null sprite!");
                }
                found = nullSprite;
            }

            return found;
        }
    }
}
