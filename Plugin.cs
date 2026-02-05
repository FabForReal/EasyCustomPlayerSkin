using System.Collections;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace EasyPackageVariety
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        
        private static string texturePath = Application.streamingAssetsPath + "/CustomPlayer/player.png";
        private static Texture2D playerTexture;
        private static bool hasTexture = false;
        
        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Harmony harmony = new(MyPluginInfo.PLUGIN_NAME);
            harmony.PatchAll();
            
            StartCoroutine(GetTexture());
        }
        
        private static IEnumerator GetTexture()
        {
            if (File.Exists(texturePath))
            {
                using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + texturePath);
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Log.LogError(uwr.error);
                }
                else
                { 
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    playerTexture = texture;
                    
                    Log.LogInfo($"custom player texture loaded from file :3");
                    hasTexture = true;
                }
            }
            else
            {
                Log.LogError($"no custom player texture at expected path");
            }
        }
        
        [HarmonyPatch]
        public class sCosmeticAppearancePatch : HarmonyPatch
        {
            private static MethodInfo TargetMethod()
            {
                return typeof(sCosmeticAppearance).GetMethod("SetAppearance", 
                    BindingFlags.Instance | BindingFlags.Public);
            }
            
            private static void Postfix(sCosmeticAppearance __instance)
            {
                if(!hasTexture) return;
                
                // ok this is kinda scuffed but who cares
                
                Transform head = __instance.transform.Find("head");
                if(head == null) return;
                Transform head2 = head.transform.Find("head");
                if(head2 == null) return;
                MeshRenderer r = head2.GetComponent<MeshRenderer>();
                if(r == null) return;
                foreach (Material m in r.sharedMaterials)
                {
                    m.SetTexture("_BaseMap", playerTexture);
                    m.SetTexture("_MainTex", playerTexture);
                }

                Transform cylinder = __instance.transform.Find("cylinder");
                if(cylinder == null) return;
                MeshRenderer r2 = cylinder.GetComponent<MeshRenderer>();
                if(r2 == null) return;
                foreach (Material m in r2.sharedMaterials)
                {
                    m.SetTexture("_BaseMap", playerTexture);
                    m.SetTexture("_MainTex", playerTexture);
                }
            }
        }
    }
}