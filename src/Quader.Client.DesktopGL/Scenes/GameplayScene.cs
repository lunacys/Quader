using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.ImGuiTools;
using Quader.Components;

namespace Quader.Scenes
{
    public class GameplayScene : Scene
    {
        public const int ScreenSpaceRenderLayer = 999;

        public readonly int Width = 1920;
        public readonly int Height = 1080;

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

            var data = RotationImageToJsonConverter.ConvertToJsonDebug(Content.Load<Texture2D>("data/srs_rotations"), out var timeSpent);
            Debug.DrawText($"Time Spent: {timeSpent}", 10f);

            /*var e = new Entity("test").AddComponent<TestComponent>();

            AddEntity(e.Entity);*/

            var e2 = new Entity("board").AddComponent<BoardComponent>();
            AddEntity(e2.Entity);
        }

        public override void Update()
        {
            base.Update();
        }
    }
}