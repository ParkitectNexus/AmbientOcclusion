using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AmbientOcclusion
{
    public class Main : IMod, IModSettings
    {
        private Shader _shader;
        private Texture2D _texture2D;

        public string Name { get { return "Ambient Occlusion"; } }
        public string Description { get { return "Ambient Occlusion effect"; } }
        public string Identifier { get; set; }
        public string Path { get; set; }

        private ScreenSpaceAmbientObscurance _ssao;

        public void onEnabled()
        {
            Object.Destroy(Camera.main.GetComponent<SSAOPro>());
            using (WWW www = new WWW("file://" + System.IO.Path.Combine(Path, "shader")))
            {
                if (www.error != null)
                    Debug.Log("Loading had an error:" + www.error);

                AssetBundle bundle = www.assetBundle;

                Shader shader = bundle.LoadAsset<Shader>("ScreenSpaceAmbientObscurance");
                Texture2D texture = bundle.LoadAsset<Texture2D>("RandomVectors");

                _ssao = Camera.main.gameObject.AddComponent<ScreenSpaceAmbientObscurance>();

                _ssao.aoShader = shader;
                _ssao.rand = texture;


                // check if ini exists, if it does load the settings from it, otherwise set defaults
                if (File.Exists(Path + @"/settings.ini"))
                {
                    IniFile ini = new IniFile(Path + @"/settings.ini");

                    _ssao.intensity = float.Parse(ini.IniReadValue("General", "intensity"));
                    _ssao.radius = float.Parse(ini.IniReadValue("General", "radius"));
                    _ssao.blurIterations = int.Parse(ini.IniReadValue("General", "blur_iterations"));
                    _ssao.blurFilterDistance = float.Parse(ini.IniReadValue("General", "blur_distance"));
                    _ssao.downsample = int.Parse(ini.IniReadValue("General", "downsample"));
                }
                else
                {
                    bool found = false;
                    foreach (ModManager.ModEntry modEntry in ModManager.Instance.getModEntries())
                    {
                        if (modEntry.isEnabled && (modEntry.mod.Name == "BetterPerspective" || modEntry.mod.Name == "Perspective"))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        SetDefaultsFor3D();
                    }
                    else
                    {
                        SetDefaultsForIsometric();
                    }
                }


                bundle.Unload(false);
            }
        }

        private void SetDefaultsFor3D()
        {
            _ssao.intensity = 0.5f;
            _ssao.blurIterations = 3;
            _ssao.blurFilterDistance = 1f;
            _ssao.radius = 0.3f;
            _ssao.downsample = 0;
        }

        private void SetDefaultsForIsometric()
        {
            _ssao.intensity = 0.12f;
            _ssao.blurIterations = 3;
            _ssao.blurFilterDistance = 0.4f;
            _ssao.radius = 1.2f;
            _ssao.downsample = 0;
        }

        public void onDisabled()
        {
            Object.Destroy(Camera.main.gameObject.GetComponent<ScreenSpaceAmbientObscurance>());
        }

        public void onDrawSettingsUI()
        {
            if (GUILayout.Button("Defaults isometric"))
            {
                SetDefaultsForIsometric();
            }

            if (GUILayout.Button("Defaults 3d cam"))
            {
                SetDefaultsFor3D();
            }

            int sliderWidth = 150;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Intensity");
            _ssao.intensity = GUILayout.HorizontalSlider(_ssao.intensity, 0, 3, GUILayout.Width(sliderWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius");
            _ssao.radius = GUILayout.HorizontalSlider(_ssao.radius, 0.1f, 3, GUILayout.Width(sliderWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Blur iterations");
            _ssao.blurIterations = Mathf.RoundToInt(GUILayout.HorizontalSlider(_ssao.blurIterations, 0, 3, GUILayout.Width(sliderWidth)));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Blur filter distance");
            _ssao.blurFilterDistance = GUILayout.HorizontalSlider(_ssao.blurFilterDistance, 0, 5, GUILayout.Width(sliderWidth));
            GUILayout.EndHorizontal();

            _ssao.downsample = Convert.ToInt32(GUILayout.Toggle(Convert.ToBoolean(_ssao.downsample), "Downsample"));
        }

        public void onSettingsOpened()
        {

        }

        public void onSettingsClosed()
        {
            IniFile ini = new IniFile(Path + @"/settings.ini");

            ini.IniWriteValue("General", "intensity", _ssao.intensity.ToString());
            ini.IniWriteValue("General", "radius", _ssao.radius.ToString());
            ini.IniWriteValue("General", "blur_iterations", _ssao.blurIterations.ToString());
            ini.IniWriteValue("General", "blur_distance", _ssao.blurFilterDistance.ToString());
            ini.IniWriteValue("General", "downsample", _ssao.downsample.ToString());
        }
    }
}
