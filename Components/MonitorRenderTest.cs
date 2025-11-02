using UnityEngine;
using System.Collections.Generic;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        public class MonitorRenderTest
        {
            static List<Camera> renderCams = new List<Camera>();

            public static void HookAllSecurityMonitors()
            {
                foreach (var mon in Object.FindObjectsOfType<SecurityMonitors>())
                {
                    if (mon == null || mon.monitors == null) continue;

                    // Clone camera
                    var camSrc = References.camLeft; // your main player camera
                    var camObj = new GameObject($"{mon.name}_RenderCam");
                    var cam = camObj.AddComponent<Camera>();
                    cam.CopyFrom(camSrc);
                    cam.enabled = true;

                    // Place the camera somewhere interesting (e.g. facing the monitor itself)
                    cam.transform.position = mon.transform.position + mon.transform.forward * 0.5f + Vector3.up * 0.2f;
                    cam.transform.rotation = Quaternion.LookRotation(-mon.transform.forward, Vector3.up);

                    // Create unique render texture
                    var rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                    rt.Create();

                    // Assign render texture
                    cam.targetTexture = rt;

                    // Find the screen material and assign texture
                    var mats = mon.monitors.materials;
                    int screenIdx = 1; // same as SecurityMonitors.screenIndex
                    if (screenIdx >= 0 && screenIdx < mats.Length)
                    {
                        var screenMat = mats[screenIdx];
                        screenMat.mainTexture = rt;
                        mon.monitors.materials = mats;
                        Debug.Log($"✅ Assigned RenderTexture to {mon.name}");
                    }

                    renderCams.Add(cam);
                }
            }
        }

    }
}