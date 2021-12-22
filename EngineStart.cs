using PloobsEngine.Engine;
using PloobsEngine.SceneControl;
using PloobsEngine.Engine.Logger;
using System;

namespace TGSceneView
{
    /// <summary>
    /// Engine entry point
    /// </summary>
    public class EngineStart
    {
        public static EngineStuff engine;
        public static DeferredScreen mainScreen;
        public EngineStart()
        {
            ///Create the default Engine Description
            InitialEngineDescription desc = InitialEngineDescription.Default();
            ///optional parameters, the default is good for most situations
            //desc.UseVerticalSyncronization = true;
            //desc.isFixedGameTime = true;
            //desc.isMultiSampling = true;
            desc.BackBufferWidth = 1280;
            desc.BackBufferHeight = 720;
            desc.useMipMapWhenPossible = true;
            desc.Logger = new SimpleLogger();
            desc.UnhandledException_Handler = UnhandledException;
            ///start the engine
            using (engine = new EngineStuff(ref desc, LoadScreen))
            {
                ///start the engine internal flow
                engine.Run();
            }
        }

        [STAThread]
        static void LoadScreen(ScreenManager manager)
        {
            ///add the first screen here
            ///WE ARE ADDING THE DEFERRED SCREEN, you can add wherever you want
            manager.AddScreen(mainScreen = new DeferredScreen());

        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ///handle unhandled excetption here (log, send to a server ....)
            Console.WriteLine("Exception: " + e.ToString());
        }
    }

    /// <summary>
    /// Custom log class
    /// When using the Release version of the engine, the log wont be used by the engine.
    /// </summary>
    class SimpleLogger : ILogger
    {
        #region ILogger Members

        public override void Log(string Message, LogLevel logLevel)
        {
            ///handle messages logs the way you want here
            Console.WriteLine(Message + "Shit" + logLevel.ToString());
        }
        #endregion
    }
}




