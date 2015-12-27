using UnityEngine;

namespace AmbientOcclusion
{
    public class Main : IMod
    {
        private Shader _shader;
        private Texture2D _texture2D;

        public void onEnabled()
        {
            using (WWW www = new WWW("file://" + System.IO.Path.Combine(Path, "shader")))
            {
                if (www.error != null)
                    Debug.Log("Loading had an error:" + www.error);
                
                AssetBundle bundle = www.assetBundle;

                Shader shader = bundle.LoadAsset<Shader>("ScreenSpaceAmbientObscurance");
                Texture2D texture = bundle.LoadAsset<Texture2D>("RandomVectors");

                ScreenSpaceAmbientObscurance ssao = Camera.main.gameObject.AddComponent<ScreenSpaceAmbientObscurance>();

                ssao.aoShader = shader;
                ssao.rand = texture;

                bool found = false;
                foreach (ModManager.ModEntry modEntry in ModManager.Instance.getModEntries())
                {
                    if (modEntry.active && (modEntry.mod.Name == "BetterPerspective" || modEntry.mod.Name == "Perspective"))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    ssao.intensity = 0.5f;
                    ssao.blurIterations = 3;
                    ssao.blurFilterDistance = 1f;
                    ssao.radius = 0.3f;
                }
                else
                {
                    ssao.intensity = 0.12f;
                    ssao.blurIterations = 3;
                    ssao.blurFilterDistance = 0.4f;
                    ssao.radius = 1.2f;
                }

                bundle.Unload(false);
            }
        }
        
        public void onDisabled()
        {
            Object.DestroyImmediate(Camera.main.gameObject.GetComponent<ScreenSpaceAmbientObscurance>());
        }

        public string Name { get { return "Ambient Occlusion"; } }
        public string Description { get { return "Ambient Occlusion"; } }
        public string Path { get; set; }
    }
}
