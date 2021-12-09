using Nez;
using Nez.ImGuiTools;

namespace Tetramonio.Scenes
{
    public class GameplayScene : Scene
    {
        public const int ScreenSpaceRenderLayer = 999;

        public readonly int Width = 1280;
        public readonly int Height = 720;

        public GameplayScene()
        {
            AddRenderer(new ScreenSpaceRenderer(100, ScreenSpaceRenderLayer));
            AddRenderer(new RenderLayerExcludeRenderer(0, ScreenSpaceRenderLayer));
        }

        public override void Initialize()
        {
            base.Initialize();

            var imGuiManager = new ImGuiManager();
            Core.RegisterGlobalManager(imGuiManager);
            
            SetDesignResolution(Width, Height, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(Width, Height);


            var e = new Entity("test").AddComponent<TestComponent>();

            AddEntity(e.Entity);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}