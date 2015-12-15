using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final
{
    class Interface
    {
        public Interface() { }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch, SpriteFont spriteFont, GameState currentGameState, Player player)
        {
            Vector2 fontPosition;

            switch (currentGameState)
            {
                case GameState.SignIn:
                    fontPosition = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - spriteFont.MeasureString("Signing in . . .") / 2;
                    spriteBatch.DrawString(spriteFont, "Signing in . . .", fontPosition, Color.White);
                    break;

                case GameState.FindSession:
                    fontPosition = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - spriteFont.MeasureString("Searching for game . . .") / 2;
                    spriteBatch.DrawString(spriteFont, "Searching for game . . .", fontPosition, Color.White);
                    break;

                case GameState.CreateSession:
                    fontPosition = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - spriteFont.MeasureString("Creating game . . .") / 2;
                    spriteBatch.DrawString(spriteFont, "Creating game . . .", fontPosition, Color.White);
                    break;

                case GameState.Start:
                    fontPosition = new Vector2(Game1.xRes / 2, Game1.yRes / 2) - spriteFont.MeasureString("Starting game . . .") / 2;
                    spriteBatch.DrawString(spriteFont, "Starting game . . .", fontPosition, Color.White);
                    break;

                case GameState.InGame:
                    break;
            }
        }
    }
}
