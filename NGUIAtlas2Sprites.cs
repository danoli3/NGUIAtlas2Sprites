using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System;

// MIT 

// Feel free to use / modify! 
// https://github.com/danoli3/NGUIAtlas2Sprites
// Helper Editor class to extract old NGUI atlas to just single file sprites / images

public class NGUIAtlas2Sprites : EditorWindow
{
  
    static string path;
    static INGUIAtlas atlas;

    static string outputPath = "/NGUIAtlas2Sprites/output/";

    [MenuItem("NGUI/NGUIAtlas2Sprites - Selected Atlas to Sprites")]
    static void Init()
    {
     
        path = UnityUtil.GetSelectedINGUIAtlasPathOrFallback();

        List<SpriteEntry> sprites = new List<SpriteEntry>();

        path = path.Replace(".prefab", "");

        Debug.Log("Extracting sprites for " + path);

        ExtractSprites(atlas, sprites, path);

        Debug.Log("Extracted for " + path);

    }

  

    public static class UnityUtil
    {
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetFileName(path);
                    break;
                }
            }
            return path;
        }

        public static string GetSelectedINGUIAtlasPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(INGUIAtlas), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetFileName(path);
                    atlas = obj as INGUIAtlas;
                    break;
                }
            }
            return path;
        }
    }

   

    class SpriteEntry : UISpriteData
    {
        // Sprite texture -- original texture or a temporary texture
        public Texture2D tex;

        // Whether the texture is temporary and should be deleted
        public bool temporaryTexture = false;
    }

    static void ExtractSprites(INGUIAtlas atlas, List<SpriteEntry> finalSprites, string _path)
    {
        // Make the atlas texture readable
        Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false, !atlas.premultipliedAlpha);

        if (atlasTex != null)
        {
            Color32[] oldPixels = null;
            int oldWidth = atlasTex.width;
            int oldHeight = atlasTex.height;
            List<UISpriteData> existingSprites = atlas.spriteList;

            if (!Directory.Exists(Application.dataPath)) Directory.CreateDirectory(Application.dataPath);
            if (!Directory.Exists(Application.dataPath + outputPath)) Directory.CreateDirectory(Application.dataPath + outputPath);
            if (!Directory.Exists(Application.dataPath + outputPath + _path)) Directory.CreateDirectory(Application.dataPath + outputPath + _path);


            foreach (UISpriteData es in existingSprites)
            {
                bool found = false;

                foreach (SpriteEntry fs in finalSprites)
                {

                    if (es.name == fs.name)
                    {
                        fs.CopyBorderFrom(es);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {

            
                    // Read the atlas
                    if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

                    int xmin = Mathf.Clamp(es.x, 0, oldWidth);
                    int ymin = Mathf.Clamp(es.y, 0, oldHeight);
                    int newWidth = Mathf.Clamp(es.width, 0, oldWidth);
                    int newHeight = Mathf.Clamp(es.height, 0, oldHeight);
                    if (newWidth == 0 || newHeight == 0) continue;

                    Color32[] newPixels = new Color32[newWidth * newHeight];

                    for (int y = 0; y < newHeight; ++y)
                    {
                        for (int x = 0; x < newWidth; ++x)
                        {
                            int newIndex = (newHeight - 1 - y) * newWidth + x;
                            int oldIndex = (oldHeight - 1 - (ymin + y)) * oldWidth + (xmin + x);
                            newPixels[newIndex] = oldPixels[oldIndex];
                        }
                    }

                    // Create a new sprite
                    SpriteEntry sprite = new SpriteEntry();
                    sprite.CopyFrom(es);
                    sprite.SetRect(0, 0, newWidth, newHeight);
                    //sprite.temporaryTexture = true;
                    sprite.temporaryTexture = false;
                    sprite.tex = new Texture2D(newWidth, newHeight);
                    sprite.tex.SetPixels32(newPixels);
                    sprite.tex.Apply();
                    finalSprites.Add(sprite);


                    // Encode texture into PNG
                    var bytes = sprite.tex.EncodeToPNG();
                    // For testing purposes, also write to a file in the project folder

                    

                    File.WriteAllBytes(Application.dataPath + outputPath + _path + "/" + es.name + ".png", bytes);

                    Debug.Log("Writing out:" + es.name);
                }
            }
        }


    }



    // Ref Maybe for older NGUI's Original Code from : Smiley https://www.tasharen.com/forum/index.php?topic=11856.0
    //static void ExtractSprites(UIAtlas atlas, List<SpriteEntry> finalSprites, string _path)
    //{
    //    // Make the atlas texture readable
    //    Texture2D atlasTex = NGUIEditorTools.ImportTexture(atlas.texture, true, false, !atlas.premultipliedAlpha);

    //    if (atlasTex != null)
    //    {
    //        Color32[] oldPixels = null;
    //        int oldWidth = atlasTex.width;
    //        int oldHeight = atlasTex.height;
    //        List<UISpriteData> existingSprites = atlas.spriteList;

    //        foreach (UISpriteData es in existingSprites)
    //        {
    //            bool found = false;

    //            foreach (SpriteEntry fs in finalSprites)
    //            {

    //                if (es.name == fs.name)
    //                {
    //                    fs.CopyBorderFrom(es);
    //                    found = true;
    //                    break;
    //                }
    //            }

    //            if (!found)
    //            {


    //                // Read the atlas
    //                if (oldPixels == null) oldPixels = atlasTex.GetPixels32();

    //                int xmin = Mathf.Clamp(es.x, 0, oldWidth);
    //                int ymin = Mathf.Clamp(es.y, 0, oldHeight);
    //                int newWidth = Mathf.Clamp(es.width, 0, oldWidth);
    //                int newHeight = Mathf.Clamp(es.height, 0, oldHeight);
    //                if (newWidth == 0 || newHeight == 0) continue;

    //                Color32[] newPixels = new Color32[newWidth * newHeight];

    //                for (int y = 0; y < newHeight; ++y)
    //                {
    //                    for (int x = 0; x < newWidth; ++x)
    //                    {
    //                        int newIndex = (newHeight - 1 - y) * newWidth + x;
    //                        int oldIndex = (oldHeight - 1 - (ymin + y)) * oldWidth + (xmin + x);
    //                        newPixels[newIndex] = oldPixels[oldIndex];
    //                    }
    //                }

    //                // Create a new sprite
    //                SpriteEntry sprite = new SpriteEntry();
    //                sprite.CopyFrom(es);
    //                sprite.SetRect(0, 0, newWidth, newHeight);
    //                //sprite.temporaryTexture = true;
    //                sprite.temporaryTexture = false;
    //                sprite.tex = new Texture2D(newWidth, newHeight);
    //                sprite.tex.SetPixels32(newPixels);
    //                sprite.tex.Apply();
    //                finalSprites.Add(sprite);


    //                // Encode texture into PNG
    //                var bytes = sprite.tex.EncodeToPNG();
    //                // For testing purposes, also write to a file in the project folder
    //                if(Directory.Exists(Application.dataPath + outputPath + _path)) Directory.CreateDirectory(Application.dataPath + outputPath + _path);

    //                File.WriteAllBytes(Application.dataPath + outputPath + _path + "/" + es.name + ".png", bytes);
    //            }
    //        }
    //    }

        // The atlas no longer needs to be readable
        //NGUIEditorTools.ImportTexture(atlas.texture, false, false, !atlas.premultipliedAlpha);
    //}

}

#endif
