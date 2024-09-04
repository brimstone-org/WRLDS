// Localization pacakge by Mike Hergaarden - M2H.nl
// DOCUMENTATION: http://www.m2h.nl/files/LocalizationPackage.pdf
// Thank you for buying this package!

using UnityEngine;


public class LocalizedAsset : MonoBehaviour
{
    public Object localizeTarget;

    public void Awake()
    {
        LocalizeAsset(localizeTarget);
    }

    public void LocalizeAsset()
    {
        LocalizeAsset(localizeTarget);
    }




    //Only overrides current asset if a translation is available
    public static void LocalizeAsset(Object target)
    {
        if (target == null)
        {
            //global::Logger.LogError("LocalizedAsset target is null");
            return;
        }

#if UNITY_2017 || UNITY_5
#pragma warning disable 618  //We know GUITexture will be removed soon, thank you.
        if (target.GetType() == typeof(GUITexture))
        {
            GUITexture gT = (GUITexture)target;
            if (gT.texture != null)
            {
                Texture text = (Texture)Language.GetAsset(gT.texture.name);
                if (text != null)
                    gT.texture = text;
            }
        }
        else
#pragma warning restore 618
#endif
        if (target.GetType() == typeof(Material))
        {
            Material mainT = (Material)target;
            if (mainT.mainTexture != null)
            {
                Texture text = (Texture)Language.GetAsset(mainT.mainTexture.name);
                if (text != null)
                    mainT.mainTexture = text;
            }
        }
        else if (target.GetType() == typeof(MeshRenderer))
        {
            MeshRenderer mainT = (MeshRenderer)target;
            if (mainT.sharedMaterial.mainTexture != null)
            {
                Texture text = (Texture)Language.GetAsset(mainT.sharedMaterial.mainTexture.name);
                if (text != null)
                    mainT.sharedMaterial.mainTexture = text;
            }
        }
        else
        {
            if (target.GetType() == typeof(UnityEngine.UI.Image))
            {
                global::Logger.Log("GUITexture is no longer supported in Unity2018+");
                return;
            }
            //global::Logger.LogError("Could not localize this object type: " + target.GetType());
        }
    }

}