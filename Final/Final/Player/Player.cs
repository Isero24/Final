using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    class Player
    {
        public string name;

        Vector3 startingLocation;
        float modelScale;
        public FlightShip vehicle;
        Camera camera;
        bool isLocal;

        public Player(string playerName, Vector3 playerStartingLocation, float playerModelScale, Camera c, bool local)
        {
            name = playerName;
            startingLocation = playerStartingLocation;
            modelScale = playerModelScale;
            camera = c;
            isLocal = local;
        }

        public void Initialize(Model playerModel, ModelManager mm, bool isStationary)
        {
            if (isLocal)
            {
                vehicle = new FlightShip(playerModel, startingLocation, modelScale, isStationary, name);
                mm.playerVehicles.Add(vehicle);
                camera.UpdateCamera(vehicle);
            }
            else
            {
                vehicle = new FlightShip(playerModel, startingLocation, modelScale, isStationary, name);
                mm.playerVehicles.Add(vehicle);
            }
        }

        public void Update(GameTime gameTime, bool v)
        {
            if (isLocal)
            {
                camera.UpdateCamera(vehicle);
            }
            else
            {
                // Remote bounds checking
            }
        }
    }
}
