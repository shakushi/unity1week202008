/*****************************************
from https://baba-s.hatenablog.com/entry/2015/05/12/133032
*****************************************/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AnimationClipCopyer
{
    private const string ITEM_NAME = "Assets/Copy Animation Clip";

    [MenuItem(ITEM_NAME)]
    private static void CopyAnimationClip()
    {
        if (!CanCopyAnimationClip())
        {
            return;
        }

        var clipList = new List<AnimationClip>();

        foreach (var clip in Selection.objects.OfType<AnimationClip>())
        {
            var clone = new AnimationClip();
            var events = AnimationUtility.GetAnimationEvents(clip);
            var srcClipInfo = AnimationUtility.GetAnimationClipSettings(clip);

            AnimationUtility.SetAnimationEvents(clone, events);
            AnimationUtility.SetAnimationClipSettings(clone, srcClipInfo);

            foreach (var n in AnimationUtility.GetAllCurves(clip, true))
            {
                clone.SetCurve(
                    relativePath: n.path,
                    type: n.type,
                    propertyName: n.propertyName,
                    curve: n.curve
                );
            }

            var path = AssetDatabase.GetAssetPath(clip);
            var directory = Path.GetDirectoryName(path);
            var outputPath = directory + "/" + clip.name + ".anim";

            /* kattenikaizou */
            //Assets/Frank_Katana/Assets/Animations/FBX/Frank_RPG_Katana_8Way_GuardWalk_BR.FBX
            
            //string indexstr = "Animations/FBX/";
            //int idx = path.IndexOf(indexstr);
            //int startp = idx + indexstr.Length;
            //int endp = path.Length - 4 - startp;
            //string fbxname = path.Substring(startp, endp);
            //var outputPath = directory + "/" + fbxname + ".anim";
            

            AssetDatabase.CreateAsset(clone, outputPath);
            clipList.Add(clone);
        }

        AssetDatabase.Refresh();
        Selection.objects = clipList.ToArray();
    }

    [MenuItem(ITEM_NAME, true)]
    private static bool CanCopyAnimationClip()
    {
        return Selection.objects.All(c => c is AnimationClip);
    }
}