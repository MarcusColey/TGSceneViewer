using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using PloobsEngine.Modelo;
using PloobsEngine.Material;
using PloobsEngine.Physics.Bepu;
using PloobsEngine.Physics;
using Microsoft.Xna.Framework;
using PloobsEngine.SceneControl;
using Microsoft.Xna.Framework.Content;
using PloobsEngine.Light;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace TGSceneView
{
    public class SceneLoader
    {
        ArrayList objectList;
        public SceneLoader(ArrayList objectList)
        {
            this.objectList = objectList;

            foreach (object[] sceneObject in objectList)
            {
                if((string)sceneObject[0] == "isModel")
                {
                    bool customAreUsedTextures = false;
                    IModelo model;

                    for (int i = 6; i < sceneObject.Length; i++)
                    {
                        if (sceneObject[i] == null)
                            customAreUsedTextures = false;
                        else
                            customAreUsedTextures = true;
                    }

                    if (customAreUsedTextures)
                        model = new SimpleModel(EngineStart.mainScreen.GraphicFactory, (string)sceneObject[1], "Textures/" + (string)sceneObject[6], "Textures/" + (string)sceneObject[7], "Textures/" + (string)sceneObject[8], "Textures/" + (string)sceneObject[9]);
                    else
                        model = new SimpleModel(EngineStart.mainScreen.GraphicFactory, (string)sceneObject[1]);

                    DeferredNormalShader shader = new DeferredNormalShader();
                    DeferredMaterial material = new DeferredMaterial(shader);

                    BoundingBox boundingBox = ModelBuilderHelper.CreateBoundingBoxFromModel(model.GetBatchInformation(0)[0], model);
                    Vector3 boxDimensions = boundingBox.Max = boundingBox.Min;
                    BoxObject modelPhysicsObject = new BoxObject((Vector3)sceneObject[2], boxDimensions.X, boxDimensions.Y, boxDimensions.Z, 10f, Vector3.One, (Matrix)sceneObject[4], MaterialDescription.DefaultBepuMaterial());
                    modelPhysicsObject.isMotionLess = (bool)sceneObject[5];
                    //modelPhysicsObject.BoundingBox = boundingBox;
               
                    IObject modelObject = new IObject(material, model, modelPhysicsObject);
                    EngineStart.mainScreen.World.AddObject(modelObject);
                }

                if ((string)sceneObject[0] == "isPointLight")
                {
                    PointLightPE pointLight = new PointLightPE((Vector3)sceneObject[1], (Color)sceneObject[2], (float)sceneObject[3], (float)sceneObject[4]);
                    pointLight.Enabled = (bool)sceneObject[5];
                    pointLight.CastShadown = (bool)sceneObject[6];
                    EngineStart.mainScreen.World.AddLight(pointLight);
                }

                if ((string)sceneObject[0] == "isSpotLight")
                {
                    SpotLightPE spotLight = new SpotLightPE((Vector3)sceneObject[1], (Vector3)sceneObject[2], 50, (float)sceneObject[4], (Color)sceneObject[3], (float)Math.Cos(Math.PI / 7), (float)sceneObject[5]);
                    spotLight.Enabled = (bool)sceneObject[6];
                    spotLight.CastShadown = (bool)sceneObject[7];
                    EngineStart.mainScreen.World.AddLight(spotLight);
                }

                if ((string)sceneObject[0] == "isPlane")
                {
                    bool customAreUsedTextures = false;
                    IModelo model;

                    for (int i = 6; i < sceneObject.Length; i++)
                    {
                        if (sceneObject[i] == null)
                            customAreUsedTextures = false;
                        else
                            customAreUsedTextures = true;
                    }

                    if (customAreUsedTextures)
                        model = new SimpleModel(EngineStart.mainScreen.GraphicFactory, "Models/box", "Textures/" + (string)sceneObject[6], "Textures/" + (string)sceneObject[7], "Textures/" + (string)sceneObject[8], "Textures/" + (string)sceneObject[9]);
                    else
                        model = new SimpleModel(EngineStart.mainScreen.GraphicFactory, "Models/box");

                    //model.SetTexture((Texture2D)sceneObject[6], TextureType.DIFFUSE);
                    model.SetTexture(EngineStart.mainScreen.GraphicFactory.CreateTexture2DColor(1, 1, Color.Red), TextureType.DIFFUSE);
                    DeferredNormalShader shader = new DeferredNormalShader();
                    DeferredMaterial material = new DeferredMaterial(shader);
                    Vector3 boxXYZ = (Vector3)sceneObject[4];
                    Vector3 scale = (Vector3)sceneObject[3];
                    BoxObject modelPhysObject = new BoxObject(Vector3.Zero, 1, 1, 1, 10f, new Vector3(scale.X, 1, scale.Z), Matrix.Identity, MaterialDescription.DefaultBepuMaterial());
                    modelPhysObject.isMotionLess = true;
                    BasicMaterialDecorator newMaterial = new BasicMaterialDecorator(material, RasterizerState.CullNone);
                    IObject modelObject = new IObject(newMaterial, model, modelPhysObject);
                    EngineStart.mainScreen.World.AddObject(modelObject);

                }

                if((string)sceneObject[0] == "isCollideObject")
                {
                    IModelo model = new SimpleModel(EngineStart.mainScreen.GraphicFactory, (string)sceneObject[1]);
                    DeferredNormalShader shader = new DeferredNormalShader();
                    DeferredMaterial material = new DeferredMaterial(shader);
                    Vector3 dimensions = (Vector3)sceneObject[3];
                    BoxObject modelPhysObject = new BoxObject((Vector3)sceneObject[2], dimensions.X, dimensions.Y, dimensions.Z, 10f, Vector3.One, Matrix.Identity, MaterialDescription.DefaultBepuMaterial());
                    modelPhysObject.isMotionLess = true;
                    IObject modelObject = new IObject(material, model, modelPhysObject);
                    material.IsVisible = false;
                    EngineStart.mainScreen.World.AddObject(modelObject);
                  
                    DeferredScreen.collideList.Add((BoundingBox)modelPhysObject.BoundingBox); // For debug Only
                }

                if ((string)sceneObject[0] == "isAudioObject")
                {
                    Stream soundFile = new FileStream(EngineStart.engine.Content.RootDirectory + "/Audio/" + (string)sceneObject[1] + ".wav", FileMode.Open, FileAccess.Read, FileShare.None);
                    SoundEffect soundEffect = SoundEffect.FromStream(soundFile);
                    SoundEffectInstance soundInstatnce = soundEffect.CreateInstance();
                    soundInstatnce.Play();
                    //soundInstatnce.Apply3D(
                }
            }
        }
    }
}
