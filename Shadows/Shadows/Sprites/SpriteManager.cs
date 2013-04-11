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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SpriteManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        
        // Stuff to draw

        List<DrawData> toDraw = new List<DrawData>();
        List<UserControlledSprite> players = new List<UserControlledSprite>();
        List<Projectile> bullets = new List<Projectile>();
        List<DrawData> toDrawNoMatrix = new List<DrawData>();
        List<Level> levels = new List<Level>();

        Level currentLevel;

        Texture2D line;
        DrawData dot;

        // Components and such
        SpriteBatch spriteBatch;
        SoundManager sound;
        CollisionManager collisionManager;
        InputManager input;
        
        // Camera related
        Matrix viewMatrix;
        Vector2 inverseMatrixMosue;

        public void setViewMatrix(Matrix viewMatrix)
        {
            this.viewMatrix = viewMatrix;
        }

        public void setInverseMatrixMosue(Vector2 pos)
        {
            this.inverseMatrixMosue = pos;
        }

        Vector2 miniplayerposition;
        float timer;
        public bool isPaused = false;

        public SpriteManager(Game game)
            : base(game)
        {
        }

        public void setCurrentLevel(int nr)
        {
            if (nr <= levels.Count )
                currentLevel = levels[nr-1]; 
        }

        public Vector2 GetPlayerPosition(int playerIndex)
        {
                return players[0].GetPostion;
            return Vector2.Zero;
        }

        public void addToDraw(string textureName, Vector2 position, float scale)
        {
            toDraw.Add(new DrawData(Game.Content.Load<Texture2D>(@"Sprite\" + textureName), position, scale)); 
        }

        public void addToDrawNoMatrix(string textureName, Vector2 position, float scale)
        {
            toDrawNoMatrix.Add(new DrawData(Game.Content.Load<Texture2D>(@"World\" + textureName), position, scale)); 
        }

        public void addLevels(string Map, string minMap, Vector2 playerSpawn, Rectangle winZone )
        {
            levels.Add(new Level(Game.Content.Load<Texture2D>(@"World\" + Map),Game.Content.Load<Texture2D>(@"World\" + minMap), playerSpawn , winZone));
        }

        public void addPlayers(int playerIndex, Vector2 spawn)
        {
            toDrawNoMatrix.Add(new DrawData(Game.Content.Load<Texture2D>(@"Sprites\MouseTexture"), Vector2.Multiply(spawn, 0.2f), 1 )); 
            players.Add(new UserControlledSprite(Game.Content.Load<Texture2D>(@"Sprites\soldier_spritesheet"), spawn, new Point(67, 90), 0.5f, new Point(0, 1), new Point(8, 1), new Vector2(6, 6), new Vector2(34, 57), 0, 89.5f));
        }

        public override void Initialize()
        {
            collisionManager = (CollisionManager)Game.Services.GetService(typeof(CollisionManager)); 
            sound = (SoundManager)Game.Services.GetService(typeof(SoundManager));
            input = (InputManager)Game.Services.GetService(typeof(InputManager));
            dot = new DrawData(Game.Content.Load<Texture2D>(@"Sprites\MouseTexture"), Vector2.Zero, 1);
            dot.textureImage = Game.Content.Load<Texture2D>(@"Sprites\MouseTexture");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            line = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            line.SetData(new[] { Color.White });

            // for all player in players
            for (int i = 0; i < players.Count; i++)
            {
                // add player animation 
                players[0].addAnimation("walk", new Point(0, 0), new Point(67, 90), new Point(8, 1));
                players[0].addAnimation("idle", new Point(0, 0), new Point(67, 90), new Point(1, 1)); 
            }
            Console.WriteLine(players.Count);
           

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            PlayerUpdate(gameTime); 

            // if game not paused
            if (!isPaused)
            {
                // get elapsed time
                timer += gameTime.ElapsedGameTime.Milliseconds;

                // Update bullets
                for (int i = 0; i < bullets.Count; i++)
                {
                    // update collison rectangle
                    bullets[i].Update(gameTime, collisionManager.clientRectangle);

                    // Check collision
                    if (collisionManager.IsOutOfBounds(bullets[i].GetPostion, players[i].frameSize) ||
                        collisionManager.pixelPerfectCollision(bullets[i].collisionRect, currentLevel.map))
                            // collision! 
                            bullets.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }

        public void PlayerUpdate(GameTime gameTime)
        {
            // for all player in players
            for (int i = 0; i < players.Count; i++)
            {
                // test collision
                if (collisionManager.pixelPerfectCollision(players[0].collisionRect, currentLevel.map))
                {
                    // Collision! 
                    players[i].Collision();
                }
                
                // Update player
                players[i].setInverseMatrixMouse(inverseMatrixMosue);
                players[i].Update(gameTime, Game.Window.ClientBounds);
                miniplayerposition = Vector2.Multiply(players[i].GetPostion, .2f);

                // Play sounds for player
                PlayerSound();

                // Shoot bullets
                if (input.leftClick)
                {
                    // Pause inbetween shots
                    if (timer < 0)
                    {
                        // Shoot! 
                        bullets.Add(new Projectile(Game.Content.Load<Texture2D>(@"Sprites\projectile"), players[i].GetPostion, new Vector2(2000, 2000), players[i].rotation));
                        timer = 100f;
                    }
                }
            }
        }

        public void PlayerSound()
        {
            // for all player in players
            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].wasWalking)
                {
                    sound.StopSoundLoop("run-loop");
                }

                if (players[i].wasWalking)
                {
                    sound.PlaySoundLoop("run-loop");
                }

                if (input.leftClick)
                {
                    sound.PlaySoundContinuously("shot", 100f);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (!isPaused)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, viewMatrix);

                // Draw player
                // for all player in players

                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Draw(spriteBatch);
                    DrawLine(line, 1, Color.Red, players[i].GetPostion, 1900f, i);
                    //spriteBatch.Draw(block, players[i].collisionRect, Color.White * 0.5f);
                }

                // Draw projectiles
                foreach (Projectile bullet in bullets)
                {
                    bullet.Draw(spriteBatch);
                }

                spriteBatch.Draw(currentLevel.map, Vector2.Zero, Color.White);

                /*foreach (Projectile bullet in bullets)
               {
                   spriteBatch.Draw(block, bullet.collisionRect, Color.White * 0.5f);
               }*/

                spriteBatch.End();
                // NoMatrix
                spriteBatch.Begin();

                

                spriteBatch.Draw(currentLevel.miniMap, Vector2.Zero, Color.White);

                for (int i = 0; i < players.Count; i++)
                {
                    dot.Draw(spriteBatch);
                }

                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        void DrawLine(Texture2D blank,
              float width, Color color, Vector2 point1, float length, int playerIndex)
        {
            float angle = players[playerIndex].rotation;
            spriteBatch.Draw(blank, point1, null, color * 0.2f,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }
    }
}
