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
using Microsoft.Xna.Framework.Net;

namespace Final
{
    public enum MessageType { UpdatePosition, WeaponFired, EndGame, StartGame, RejoinLobby, RestartGame, UpdateRemotePlayer, Kill, Respawn, CreateOrb }

    public enum GameState
    {
        SignIn, FindSession,
        CreateSession, Start, InGame, GameOver
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ModelManager modelManager;

        Model localPlayerModel;
        Model remotePlayerModel;

        SpriteFont font;

        Texture2D heightmap;

        Vector3 lastCameraPosition;
        Vector3 playerStartingLocation;

        float playerModelScale;

        Interface intrface;

        RenderTarget2D renderTarget;

        Player localPlayer;
        Player remotePlayer;

        QuadTree _quadTree;
        RasterizerState _rsDefault = new RasterizerState();
        RasterizerState _rsWire = new RasterizerState();
        bool _isWire;
        KeyboardState _previousKeyboardState;

        GraphicsDevice _device;

        List<Terrain> terrainPieces;

        public Terrain terrain;

        // Initial state to trigger Windows LIVE signin
        GameState currentGameState = GameState.SignIn;
        NetworkSession networkSession;
        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();

        public static int xRes = 1500;
        public static int yRes = 800;

        public Camera camera { get; protected set; }
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1500;
            graphics.PreferredBackBufferHeight = 800;

            playerStartingLocation = new Vector3(40, 40, -3);
            playerModelScale = .07f;

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
            terrainPieces = new List<Terrain>();

            camera = new Camera(this, new Vector3(100, 200, -100),
                new Vector3(0, 0, 0), Vector3.Up);

            modelManager = new ModelManager(this, camera);

            Components.Add(camera);
            Components.Add(modelManager);
            Components.Add(new GamerServicesComponent(this));

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

            lastCameraPosition = camera.cameraPosition;

            localPlayerModel = Content.Load<Model>(@"airplane\ju-87");
            remotePlayerModel = Content.Load<Model>(@"airplane\ju-87");

            heightmap = Content.Load<Texture2D>(@"map\Size128\heightmap4");
            //generateTerrain(heightmap);

            intrface = new Interface();

            font = Content.Load<SpriteFont>("SpriteFont1");

            renderTarget = new RenderTarget2D(_device, 1500, 800, false, _device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);

            base.LoadContent();
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
            //Window.Title = String.Format("GameState: {0}", currentGameState);

            if (this.IsActive)
            {
                // Run different methods based on game state
                switch (currentGameState)
                {
                    case GameState.SignIn:
                        Update_SignIn();
                        break;
                    case GameState.FindSession:
                        Update_FindSession();
                        break;
                    case GameState.CreateSession:
                        Update_CreateSession();
                        break;
                    case GameState.Start:
                        Update_Start(gameTime);
                        break;
                    case GameState.InGame:
                        Update_InGame(gameTime);
                        break;
                    /*case GameState.GameOver:
                        Update_GameOver(gameTime);
                        break;*/
                }
            }

            // Update the network session
            if (networkSession != null)
                networkSession.Update();

            base.Update(gameTime);
        }

        protected void Update_SignIn()
        {
            // If no local gamers are signed in, show sign-in screen
            if (Gamer.SignedInGamers.Count < 1)
            {
                // Guide is a part of GamerServices, this will bring up the 
                // SignIn window, allowing 1 user to Sign in, The false allows
                // users to sign in locally
                Guide.ShowSignIn(1, false);
            }
            else
            {
                // Local gamer signed in, move to find sessions
                currentGameState = GameState.FindSession;
            }
        }

        private void Update_FindSession()
        {
            // Find sessions of the current game over SystemLink, 1 local gamer,
            // no special properties
            AvailableNetworkSessionCollection sessions =
                NetworkSession.Find(NetworkSessionType.SystemLink, 1, null);

            if (sessions.Count == 0)
            {
                // If no sessions exist, move to the CreateSession game state
                currentGameState = GameState.CreateSession;
            }
            else
            {
                // If a session does exist, join it, wire up events,
                // and move to the Start game state
                networkSession = NetworkSession.Join(sessions[0]);
                WireUpEvents();
                currentGameState = GameState.Start;
            }
        }

        private void Update_CreateSession()
        {
            // Create a new session using SystemLink with a max of 1
            // local player and a max of 2 total players
            networkSession = NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);

            // If the host drops, other player becomes host
            networkSession.AllowHostMigration = true;

            // Caonnot join a game in progress
            networkSession.AllowJoinInProgress = false;

            // Wire up events and move to the Start game state
            WireUpEvents();
            currentGameState = GameState.Start;
        }

        protected void WireUpEvents()
        {
            // Wire up events for gamers joining and leaving, defines what to do when a gamer
            // Joins or leaves the session
            networkSession.GamerJoined += GamerJoined;
            networkSession.GamerLeft += GamerLeft;
        }

        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // Gamer joined. Set the tag for the gamer to a new UserControlledObject.
            // These Tags are going to be your local representation of remote players
            if (e.Gamer.IsHost)
            {
                // The Create players will create and return instances of your player class, setting
                // the appropriate values to differentiate between local and remote players
                // Tag is of type Object, which means it can hold any type
                e.Gamer.Tag = CreateLocalPlayer(e.Gamer.Gamertag);

                // Moved to here instead of down below because network is currently being circumvented
                e.Gamer.Tag = CreateRemotePlayer(e.Gamer.Gamertag);
            }
            else
            {
                e.Gamer.Tag = CreateRemotePlayer(e.Gamer.Gamertag);
            }
        }

        void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            // Dispose of the network session, set it to null.
            networkSession.Dispose();
            networkSession = null;

            // Perform any necessary clean up,
            // stop sound track, etc.

            // Go back to looking for another session
            currentGameState = GameState.FindSession;
        }

        private object CreateLocalPlayer(string GamerTag)
        {
            InitializeLevel();

            localPlayer = new Player(GamerTag, playerStartingLocation, playerModelScale, camera, true);
            localPlayer.Initialize(localPlayerModel, modelManager, false);

            return localPlayer;
        }

        private object CreateRemotePlayer(string GamerTag)
        {
            InitializeLevel();

            remotePlayer = new Player("Player2", playerStartingLocation, playerModelScale, camera, false);
            remotePlayer.Initialize(remotePlayerModel, modelManager, true);

            return remotePlayer;
        }

        private void Update_Start(GameTime gameTime)
        {
            // Get local gamer, should be just one
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // Check for game start key or button press
            // only if there are two players
            if (networkSession.AllGamers.Count == 1)
            {
                // If space bar or start button is pressed, begin the game
                //if (Keyboard.GetState().IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                //{
                    // Send message to other player that we're starting
                    packetWriter.Write((int)MessageType.StartGame);
                    localGamer.SendData(packetWriter, SendDataOptions.Reliable);

                    // Call StartGame
                    StartGame();
                //}
            }

            // Process any incoming packets
            ProcessIncomingData(gameTime);
        }

        protected void StartGame()
        {
            // Set game state to InGame
            currentGameState = GameState.InGame;

            // Any other that that need to be set up
            // for beginning a game
            // Starting audio, resetting values, etc
        }

        private void InitializeLevel()
        {
            generateTerrain(heightmap);
        }

        protected void ProcessIncomingData(GameTime gameTime)
        {
            // Process incoming data
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // While there are packets to be read . . .
            while (localGamer.IsDataAvailable)
            {
                // Get the packet and info on sender
                NetworkGamer sender;
                localGamer.ReceiveData(packetReader, out sender);

                // Ignore the packet if you sent it
                if (!sender.IsLocal)
                {
                    // Read messagetype from start of packet and call appropriate method
                    MessageType messageType = (MessageType)packetReader.ReadInt32();
                    switch (messageType)
                    {
                        case MessageType.EndGame:
                            EndGame();
                            break;
                        case MessageType.StartGame:
                            StartGame();
                            break;
                        case MessageType.RejoinLobby:
                            currentGameState = GameState.Start;
                            break;
                        case MessageType.RestartGame:
                            StartGame();
                            break;
                        case MessageType.UpdateRemotePlayer:
                            UpdateRemotePlayer(gameTime);
                            break;

                            // Any other actions for specific messages
                    }
                }
            }
        }

        protected void EndGame()
        {
            // Perform whatever actions are to occur
            // when a game ends. Stop music, play
            // A certain sound effect, etc.
            currentGameState = GameState.GameOver;
        }

        protected void UpdateRemotePlayer(GameTime gameTime)
        {
            // Get the other (non-local) player
            // Think about combining this with GetOtherPlayer() method
            //NetworkGamer theOtherGuy = GetOtherPlayer();

            // Get the PlayerClass represeting the other player
            //Player theOtherPlayer = ((Player)theOtherGuy.Tag);

            // Read in the new position of the other player
            // Set the position
            //remotePlayer.position = packetReader.ReadVector3(); ;

            // Read any other information from the packet and handle it
            //remotePlayer.Update(gameTime, true);
        }

        protected NetworkGamer GetOtherPlayer()
        {
            // Search through the list of players and find the
            // one that's remote
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                if (!gamer.IsLocal)
                    return gamer;
            }
            return null;
        }

        private void Update_InGame(GameTime gameTime)
        {
            // Update the local player
            UpdateLocalPlayer(gameTime);

            // Update Remote player
            UpdateRemotePlayer(gameTime);

            // Read any incoming data
            ProcessIncomingData(gameTime);

            // Only host checks for endgame
            if (networkSession.IsHost)
            {
                // Check for end game conditions, if they are met send a message to other player
                packetWriter.Write((int)MessageType.EndGame);
                networkSession.LocalGamers[0].SendData(packetWriter, SendDataOptions.Reliable);
                //EndGame();
            }
        }

        protected void UpdateLocalPlayer(GameTime gameTime)
        {
            // Get local player
            LocalNetworkGamer localGamer = networkSession.LocalGamers[0];

            // Get the local player's sprite
            //Player local = (Player)localGamer.Tag;

            // Call the local's Update method which will process user input
            // for movement and update animation frame
            // Boolean used to inform the update function that the local player is calling update,
            // therefore update based on local input
            localPlayer.Update(gameTime, true);

            // Send message to other player with message tag and new position of local player
            packetWriter.Write((int)MessageType.UpdateRemotePlayer);
            //packetWriter.Write(localPlayer.position);

            // Send data to other player
            localGamer.SendData(packetWriter, SendDataOptions.InOrder);

            // Package up any other necessary data nad send it to other player
        }

        private void Update_GameOver(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            // If player presses enter or A button, restart game
            if (keyboardState.IsKeyDown(Keys.Enter) || gamePadState.Buttons.A == ButtonState.Pressed)
            {
                // Send restart game message
                packetWriter.Write((int)MessageType.RestartGame);
                networkSession.LocalGamers[0].SendData(packetWriter, SendDataOptions.Reliable);
                RestartGame();
            }

            // If player presses escape or B button, rejoin lobby
            if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.B == ButtonState.Pressed)
            {
                // Send join lobby message
                packetWriter.Write((int)MessageType.RejoinLobby);
                networkSession.LocalGamers[0].SendData(packetWriter, SendDataOptions.Reliable);
                //RejoinLobby();
            }

            // Read any incoming message
            ProcessIncomingData(gameTime);
        }

        public void RestartGame()
        {

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Only draw when game is active
            if (this.IsActive)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.SetRenderTarget(renderTarget);
                GraphicsDevice.Clear(Color.CornflowerBlue);
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.BlendState = BlendState.Opaque;

                // Based on the current game state,
                // call the appropriate method
                switch (currentGameState)
                {
                    case GameState.SignIn:
                    case GameState.FindSession:
                    case GameState.CreateSession:
                        GraphicsDevice.Clear(Color.DarkBlue);
                        break;
                    case GameState.Start:
                        // Wrhite function to draw the start screen
                        //DrawStartScreen();
                        break;
                    case GameState.InGame:
                        // Write function to handle draws during game time (terrain, models, etc)
                        DrawInGameScreen(gameTime);
                        break;
                    case GameState.GameOver:
                        // Write function to draw game over screen
                        //DrawGameOverScreen();
                        break;
                }
            }

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, 1500, 800), Color.White);
            intrface.Draw(spriteBatch, font, currentGameState, localPlayer);
            spriteBatch.End();

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

        }

        void DrawInGameScreen(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int tilesDrawn = 0;
            foreach (Terrain t in terrainPieces)
            {
                if (t.isVisible(new BoundingFrustum(camera.view * camera.projection)))
                {
                    t.Draw(gameTime);
                    tilesDrawn++;
                }
            }

            //Window.Title = String.Format("Terrain Tiles Drawn: {0} - Total Terrain Tiles: {1}", tilesDrawn, terrainPieces.Count);

            base.Draw(gameTime);

            //_quadTree.Draw(gameTime);
        }

        protected void generateTerrain(Texture2D heightmap)
        {

            Texture2D tempData;

            Terrain tempTerrain = new Terrain(this);

            Rectangle sourceRectangle;

            int segmentSize = 256;

            tempData = new Texture2D(GraphicsDevice, segmentSize, segmentSize);

            // Create a color variable array that will hold the pixel color of the new segment
            Color[] data = new Color[segmentSize * segmentSize];

            int hmSize = heightmap.Width;
            int splitNum = 0;
            while (hmSize > segmentSize)
            {
                hmSize /= 2;
                splitNum++;
            }

            splitNum = 7;
            for (int x = 0; x <= splitNum; x++)
            {
                for (int y = 0; y <= splitNum; y++)
                {
                    sourceRectangle = new Rectangle((segmentSize - 1) * x, (segmentSize - 1) * y, segmentSize, segmentSize);
                    heightmap.GetData(0, sourceRectangle, data, 0, data.Length);
                    tempData.SetData(data);
                    terrainPieces.Add(new Terrain(this));
                    terrainPieces.Last().load(tempData, 0, 0, 2.0f, 2.0f, x + (splitNum * 0), splitNum - y + (splitNum * 0), segmentSize - 1);
                }
            }

            /*for (int x = 0; x <= splitNum; x++)
            {
                for (int y = 0; y <= splitNum; y++)
                {
                    sourceRectangle = new Rectangle((segmentSize - 1) * x, (segmentSize - 1) * y, segmentSize, segmentSize);
                    heightmap.GetData(0, sourceRectangle, data, 0, data.Length);
                    tempData.SetData(data);
                    terrainPieces.Add(new Terrain(this));
                    terrainPieces.Last().load(tempData, 0, 0, 2.0f, 3.0f, x + (splitNum * 1), splitNum - y + (splitNum * 0), segmentSize - 1);
                }
            }*/
        }


    }
}
