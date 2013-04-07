using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Shadows
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        FpsViewer fps;
        SpriteManager spriteManager;
        LightManager lightManager; 
        InputManager inputManager;
        CollisionManager collisionManager; 

        Vector2 inverseMatrixPostion;

        Texture2D floorTexture;
        Texture2D blood; 
        RenderTarget2D toApplyLight;

        int screenWidth = 1440;
        int screenHeight = 900;

        Camera camera;

        Song song;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;

            //AA 
            graphics.PreferMultiSampling = true;
            //IsMouseVisible = true;
            /*Unlimited FPS :) */
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / 100.0f);
            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

        }

        protected override void Initialize()
        {

            floorTexture = Content.Load<Texture2D>(@"World\tile");

            // TODO: Add your initialization logic here
            fps = new FpsViewer(this);
            spriteManager = new SpriteManager(this);
            inputManager = new InputManager(this);
            lightManager = new LightManager(this, graphics);
            collisionManager = new CollisionManager(this, new Rectangle(0, 0, screenWidth, screenHeight)); 

            camera = new Camera(new Vector2(screenWidth, screenHeight), 1.5f); 

            // Add Component
            Components.Add(fps);
            Components.Add(spriteManager);
            Components.Add(inputManager);
            Components.Add(lightManager);
            Components.Add(collisionManager); 

            // AddService
            Services.AddService(typeof(GraphicsDeviceManager), graphics);
            Services.AddService(typeof(SpriteManager), spriteManager);
            Services.AddService(typeof(LightManager), lightManager);
            Services.AddService(typeof(InputManager), inputManager);
            Services.AddService(typeof(CollisionManager), collisionManager); 

            // draworder 
            fps.DrawOrder = 10;
            inputManager.DrawOrder = 9;
            spriteManager.DrawOrder = 8;
            lightManager.DrawOrder = 7; 
            base.Initialize();
          

            

            // ONLY REMOVE THIS IF YOU HAVE A MULTIPLE MONITOR SETUP!! ( DRAWS THE GAME ON THE SECOND MONITOR IF IN DEBUG MODE ) 
/* #if DEBUG
            var form = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(this.Window.Handle);
            form.Location = new System.Drawing.Point(1920 + 1920/2, 0);
#endif */
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            toApplyLight = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            blood = Content.Load<Texture2D>(@"World\blood");
            song = Content.Load<Song>(@"Sound\Music\song");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            camera.Update(spriteManager.GetPlayerPosition());
            spriteManager.setViewMatrix(camera.ViewMatrix);
            lightManager.setViewMatrix(camera.ViewMatrix);
            inputManager.setViewMatrix(camera.ViewMatrix);

            inverseMatrixPostion = Vector2.Transform(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Matrix.Invert(camera.ViewMatrix));
            spriteManager.setInverseMatrixMosue(inverseMatrixPostion);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

    

            //set render target to toApplyLight which will be used to blend together with the screenLight render target
            GraphicsDevice.SetRenderTarget(toApplyLight);
            GraphicsDevice.Clear(Color.Black);
            drawFloor();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.ViewMatrix);
            spriteBatch.Draw(blood, Vector2.Zero, Color.White);
            spriteBatch.End(); 
            GraphicsDevice.SetRenderTarget(null); 
            // sends the rendertarget to the light manager; 
            lightManager.setRendertarget(toApplyLight); 

            base.Draw(gameTime);
        }

        public void drawFloor()
        {
            Rectangle source = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, camera.ViewMatrix);
            spriteBatch.Draw(floorTexture, Vector2.Zero, source, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f); 
            spriteBatch.End();
        }
    }
}
