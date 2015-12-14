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

namespace Final
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ModelManager modelManager;

        SpriteFont font;

        QuadTree _quadTree;
        RasterizerState _rsDefault = new RasterizerState();
        RasterizerState _rsWire = new RasterizerState();
        bool _isWire;
        KeyboardState _previousKeyboardState;

        GraphicsDevice _device;

        public string testText = "null";

        public Terrain terrain;

        public FirstPersonCamera fpCamera { get; protected set; }
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 1500;
            graphics.PreferredBackBufferHeight = 800;

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            modelManager = new ModelManager(this);
            Components.Add(modelManager);

            fpCamera = new FirstPersonCamera(this, new Vector3(300, 50, 300),
                new Vector3(300, 0, 100), Vector3.Up);
            Components.Add(fpCamera);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _rsDefault.CullMode = CullMode.CullCounterClockwiseFace;
            _rsDefault.FillMode = FillMode.Solid;

            _rsWire.CullMode = CullMode.CullCounterClockwiseFace;
            _rsWire.FillMode = FillMode.WireFrame;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _device = graphics.GraphicsDevice;

            terrain = new Terrain(this);
            terrain.load(@"map\Size256\hmBlack", 0, 0, 1.0f, 1.0f);

            font = Content.Load<SpriteFont>(@"SpriteFont1");


            //Texture2D heightMap = Content.Load<Texture2D>(@"map\hmLarge");

            //_quadTree = new QuadTree(Vector3.Zero, heightMap, fpCamera.view, fpCamera.projection, _device, 1);
            //_quadTree.Effect.Texture = Content.Load<Texture2D>(@"map\Bitmap1");

            base.LoadContent();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            if (_previousKeyboardState == null)
                _previousKeyboardState = currentKeyboardState;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (currentKeyboardState[Keys.Q] == KeyState.Up && _previousKeyboardState.IsKeyDown(Keys.Q))
            {
                if (_isWire)
                {
                    _device.RasterizerState = _rsDefault;
                    _isWire = false;
                }
                else
                {
                    _device.RasterizerState = _rsWire;
                    _isWire = true;
                }
            }

            _previousKeyboardState = currentKeyboardState;

            terrain.Update(gameTime);

            /*_quadTree.View = fpCamera.view;
            _quadTree.Projection = fpCamera.projection;
            _quadTree.CameraPosition = fpCamera.cameraPosition;
            _quadTree.Update(gameTime);*/

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            terrain.Draw(gameTime);

            //spriteBatch.Begin();
            //spriteBatch.DrawString(font, testText, Vector2.Zero, Color.Red);
            //spriteBatch.End();

            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //_quadTree.Draw(gameTime);

            base.Draw(gameTime);

        }
    }
}
