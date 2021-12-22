using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using PloobsEngine.Cameras;
using PloobsEngine.Light;
using PloobsEngine.Physics;
using PloobsEngine.SceneControl;
using PloobsEngine.Modelo;
using PloobsEngine.Material;
using PloobsEngine.Physics.Bepu;
using PloobsEngine.Features;
using PloobsEngine.Engine;
using PloobsEngine.Commands;
using PloobsEngine.Audio;
using PloobsEngine.SceneControl.GUI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using TGEditor;
using Microsoft.Xna.Framework.Graphics;
using TomShane.Neoforce.Controls;

namespace TGSceneView
{
    public class DeferredScreen : IScene
    {
        EngineStuff engine;
        CameraFirstPerson camera;

        public static List<BoundingBox> collideList = new List<BoundingBox>();
        string selectedScene;

        NeoforceGui guiManager;

        Window controlPanel;
        bool hasClosed;

        Button reloadButton;
        Button loadButton;

        KeyboardState oldKeyBoardSate;

        bool useMouse;

        public DeferredScreen() : base(new NeoforceGui()) { }

        public DeferredScreen(string fileToLoad)
        {
            selectedScene = fileToLoad;
        }

        protected override void SetWorldAndRenderTechnich(out IRenderTechnic renderTech, out IWorld world)
        {
            BepuPhysicWorld bepuPhysWorld = new BepuPhysicWorld(-0.98f, true, 1, true);
            bepuPhysWorld.isDebugDraw = true;
            world = new IWorld(bepuPhysWorld, new OctreeCuller(3000, 3, 5, Vector3.Zero));
            BepuPhysicWorld.ApplyHighStabilitySettings(bepuPhysWorld);

            ///Create the deferred description
            DeferredRenderTechnicInitDescription desc = DeferredRenderTechnicInitDescription.Default();
            ///Some custom parameter, this one allow light saturation. (and also is a pre requisite to use hdr)

            desc.UseFloatingBufferForLightMap = true;
            ShadowLightMap lightMap = new ShadowLightMap(ShadowFilter.PCF7x7SOFT, 1024, DirectionalShadowFilteringType.PCF7x7, 512, 0.75f);
            desc.DeferredLightMap = lightMap;

            desc.UseFloatingBufferForLightMap = true;
            ///set background color, default is black
            desc.BackGroundColor = Color.CornflowerBlue;
            ///create the deferred technich
            renderTech = new DeferredRenderTechnic(desc);
        }

        protected override void InitScreen(GraphicInfo GraphicInfo, EngineStuff engine)
        {
            oldKeyBoardSate = Keyboard.GetState();

            engine.IsMouseVisible = true;
            this.engine = engine;
            base.InitScreen(GraphicInfo, engine);
        }

        protected override void LoadContent(PloobsEngine.Engine.GraphicInfo GraphicInfo, PloobsEngine.Engine.GraphicFactory factory, IContentManager contentManager)
        {
            ///must be called before all
            base.LoadContent(GraphicInfo, factory, contentManager);

            engine.Content.RootDirectory = "C:/Users/Marcus/Documents/Visual Studio 2010/Projects/TGEditor-C/TGEditor-C/bin/x86/Debug/TGEditor-CContent"; // Change this to the root directory of the actualy editor
            
            ///DebugShapeRenderer.Initialize(engine.GraphicsDevice); // For debug only

            ///Add some directional lights to completely iluminate the world
            #region Lights
            DirectionalLightPE ld1 = new DirectionalLightPE(Vector3.Left, Color.White);
            DirectionalLightPE ld2 = new DirectionalLightPE(Vector3.Right, Color.White);
            DirectionalLightPE ld3 = new DirectionalLightPE(Vector3.Backward, Color.White);
            DirectionalLightPE ld4 = new DirectionalLightPE(Vector3.Forward, Color.White);
            DirectionalLightPE ld5 = new DirectionalLightPE(Vector3.Down, Color.White);
            float li = 0.4f;
            ld1.LightIntensity = li;
            ld2.LightIntensity = li;
            ld3.LightIntensity = li;
            ld4.LightIntensity = li;
            ld5.LightIntensity = li;
            this.World.AddLight(ld1);
            this.World.AddLight(ld2);
            this.World.AddLight(ld3);
            this.World.AddLight(ld4);
            this.World.AddLight(ld5);

            #endregion

            ///Add a AA post effect
            this.RenderTechnic.AddPostEffect(new AntiAliasingPostEffect());

            ///add a camera 
            this.World.CameraManager.AddCamera(camera = new CameraFirstPerson(false, new Vector3(75), this.GraphicInfo));

            guiManager = this.Gui as NeoforceGui;

            controlPanel = new Window(guiManager.Manager);
            controlPanel.Init();
            controlPanel.Text = "Control panel";
            controlPanel.Width = 250;
            controlPanel.Height = engine.GraphicsDevice.Viewport.Height;
            controlPanel.SetPosition(engine.GraphicsDevice.Viewport.Width - controlPanel.Width, engine.GraphicsDevice.Viewport.Height - controlPanel.Height);
            controlPanel.Closed += new WindowClosedEventHandler(controlPanel_Closed);

            reloadButton = new Button(guiManager.Manager);
            reloadButton.Init();
            reloadButton.Text = "Reload";
            reloadButton.Width = 200;
            reloadButton.Height = 25;
            reloadButton.Parent = controlPanel;
            reloadButton.SetPosition(reloadButton.Parent.ClientWidth - reloadButton.Width - ((reloadButton.Parent.ClientWidth - reloadButton.Width) / 2), reloadButton.Height);
            reloadButton.Click += new TomShane.Neoforce.Controls.EventHandler(reloadButton_Click);

            loadButton = new Button(guiManager.Manager);
            loadButton.Init();
            loadButton.Text = "Load";
            loadButton.Width = 200;
            loadButton.Height = 25;
            loadButton.Parent = controlPanel;
            loadButton.SetPosition(reloadButton.Parent.ClientWidth - loadButton.Width - ((loadButton.Parent.ClientWidth - reloadButton.Width) / 2), reloadButton.Height + 35);
            loadButton.Click += new TomShane.Neoforce.Controls.EventHandler(loadButton_Click);

            guiManager.Manager.Add(controlPanel);

            
        }

        void controlPanel_Closed(object sender, WindowClosedEventArgs e)
        {
            hasClosed = true;
        }

        void reloadButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            for (int i = 0; i < EngineStart.mainScreen.World.Objects.Count; i++)
            {
                EngineStart.mainScreen.World.RemoveObject(EngineStart.mainScreen.World.Objects[i]);
                i--;
            }
            for (int i = 0; i < EngineStart.mainScreen.World.Lights.Count; i++)
            {
                EngineStart.mainScreen.World.RemoveLight(EngineStart.mainScreen.World.Lights[i]);
                i--;
            }

            SceneSerializer loadedScene = new SceneSerializer();

            Stream openStream = new FileStream(selectedScene, FileMode.Open, FileAccess.Read, FileShare.None);

            IFormatter openFormatter = new BinaryFormatter();
            loadedScene = (SceneSerializer)openFormatter.Deserialize(openStream);

            openStream.Close();

            ArrayList testList;
            testList = loadedScene.listOfObjects;
            new SceneLoader(testList);

            // Create and add defualt lighting
            DirectionalLightPE[] defaultLight = new DirectionalLightPE[6];

            DirectionalLightPE upDir = new DirectionalLightPE(Vector3.Up, Microsoft.Xna.Framework.Color.White);
            DirectionalLightPE downDir = new DirectionalLightPE(Vector3.Down, Microsoft.Xna.Framework.Color.White);
            DirectionalLightPE rightDir = new DirectionalLightPE(Vector3.Right, Microsoft.Xna.Framework.Color.White);
            DirectionalLightPE leftDir = new DirectionalLightPE(Vector3.Left, Microsoft.Xna.Framework.Color.White);
            DirectionalLightPE backDir = new DirectionalLightPE(Vector3.Backward, Microsoft.Xna.Framework.Color.White);
            DirectionalLightPE forDir = new DirectionalLightPE(Vector3.Forward, Microsoft.Xna.Framework.Color.White);
            defaultLight[0] = upDir;
            defaultLight[1] = downDir;
            defaultLight[2] = rightDir;
            defaultLight[3] = leftDir;
            defaultLight[4] = backDir;
            defaultLight[5] = forDir;

            foreach (DirectionalLightPE light in defaultLight)
            {
                light.LightIntensity = 0.4f;
                EngineStart.mainScreen.World.AddLight(light);
            }

        }

        void loadButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.ShowDialog();
            selectedScene = openFile.FileName;

            //SceneSerializer loadedScene = new SceneSerializer();

            //Stream openStream = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read, FileShare.None);

            for (int i = 0; i < EngineStart.mainScreen.World.Objects.Count; i++)
            {
                EngineStart.mainScreen.World.RemoveObject(EngineStart.mainScreen.World.Objects[i]);
                i--;
            }
            for (int i = 0; i < EngineStart.mainScreen.World.Lights.Count; i++)
            {
                EngineStart.mainScreen.World.RemoveLight(EngineStart.mainScreen.World.Lights[i]);
                i--;
            }

            if (openFile.FileName != string.Empty)
            {
                SceneSerializer loadedScene = new SceneSerializer();

                Stream openStream = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read, FileShare.None);

                IFormatter openFormatter = new BinaryFormatter();
                loadedScene = (SceneSerializer)openFormatter.Deserialize(openStream);

                openStream.Close();

                ArrayList testList;
                testList = loadedScene.listOfObjects;
                new SceneLoader(testList);

                // Create and add defualt lighting
                DirectionalLightPE[] defaultLight = new DirectionalLightPE[6];

                DirectionalLightPE upDir = new DirectionalLightPE(Vector3.Up, Microsoft.Xna.Framework.Color.White);
                DirectionalLightPE downDir = new DirectionalLightPE(Vector3.Down, Microsoft.Xna.Framework.Color.White);
                DirectionalLightPE rightDir = new DirectionalLightPE(Vector3.Right, Microsoft.Xna.Framework.Color.White);
                DirectionalLightPE leftDir = new DirectionalLightPE(Vector3.Left, Microsoft.Xna.Framework.Color.White);
                DirectionalLightPE backDir = new DirectionalLightPE(Vector3.Backward, Microsoft.Xna.Framework.Color.White);
                DirectionalLightPE forDir = new DirectionalLightPE(Vector3.Forward, Microsoft.Xna.Framework.Color.White);
                defaultLight[0] = upDir;
                defaultLight[1] = downDir;
                defaultLight[2] = rightDir;
                defaultLight[3] = leftDir;
                defaultLight[4] = backDir;
                defaultLight[5] = forDir;

                foreach (DirectionalLightPE light in defaultLight)
                {
                    light.LightIntensity = 0.4f;
                    EngineStart.mainScreen.World.AddLight(light);
                }
            }

            //foreach (IObject sceneObject in World.Objects)
            //{
               // collideList.Add((BoundingBox)sceneObject.PhysicObject.BoundingBox);
            //}

        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keybaord = Keyboard.GetState();

            if (keybaord.IsKeyDown(Keys.C) && hasClosed)
            {
                controlPanel = new Window(guiManager.Manager);
                controlPanel.Init();
                controlPanel.Text = "Control panel";
                controlPanel.Width = 250;
                controlPanel.Height = engine.GraphicsDevice.Viewport.Height;
                controlPanel.SetPosition(engine.GraphicsDevice.Viewport.Width - controlPanel.Width, engine.GraphicsDevice.Viewport.Height - controlPanel.Height);

                reloadButton = new Button(guiManager.Manager);
                reloadButton.Init();
                reloadButton.Text = "Reload";
                reloadButton.Width = 200;
                reloadButton.Height = 25;
                reloadButton.Parent = controlPanel;
                reloadButton.SetPosition(reloadButton.Parent.ClientWidth - reloadButton.Width - ((reloadButton.Parent.ClientWidth - reloadButton.Width) / 2), reloadButton.Height);

                loadButton = new Button(guiManager.Manager);
                loadButton.Init();
                loadButton.Text = "Load";
                loadButton.Width = 200;
                loadButton.Height = 25;
                loadButton.Parent = controlPanel;
                loadButton.SetPosition(reloadButton.Parent.ClientWidth - loadButton.Width - ((loadButton.Parent.ClientWidth - reloadButton.Width) / 2), reloadButton.Height + 35);

                guiManager.Manager.Add(controlPanel);

                hasClosed = false;
            }

            KeyboardState newKeyBoardState = Keyboard.GetState();
            if (newKeyBoardState.IsKeyDown(Keys.Escape) && oldKeyBoardSate.IsKeyUp(Keys.Escape))
            {
                useMouse = !useMouse;
                camera.EnableMouse(useMouse);
            }

            oldKeyBoardSate = newKeyBoardState;

            

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime, RenderHelper render)
        {
            ///must be called before
            //engine.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            //engine.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            base.Draw(gameTime, render); // I get null reference exception here here! Render is null?
            ///Draw some text to the screen
            render.RenderTextComplete("TGSceneView", new Vector2(GraphicInfo.Viewport.Width - 400, 15), Color.White, Matrix.Identity);
            //DebugShapeRenderer.Draw(gameTime, camera.View, camera.Projection);

            // For debug only
            //foreach (BoundingBox box in collideList)
            //{
                //BoundingBox bb = new BoundingBox(box.BoundingBox.Value.Min, box.BoundingBox.Value.Max);
                //DebugShapeRenderer.AddBoundingBox(box, Color.Purple);
            //}
        }
    }
}

