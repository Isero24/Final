using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    class Player
    {
        public string playerName;

        public Vector3 position;

        public Player(string playerName)
        {
            this.playerName = playerName;
        }

        public void Initialize()
        {

        }

        internal void Update(GameTime gameTime, bool v)
        {
            throw new NotImplementedException();
        }
    }
}
