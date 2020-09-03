using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace PandaMonogame.UI
{
    public class PUITooltip
    {
        public string Name;
        public Texture2D Texture;
    }

    public static class PUITooltipManager
    {
        public static GraphicsDevice Graphics;
        public static DynamicSpriteFont DefaultFont;
        public static Color DefaultColor = new Color(0, 0, 0, 120);
        public static Color DefaultTextColor = new Color(255, 255, 255, 255);
        public static PUITooltip ActiveTooltip;
        public static Dictionary<string, PUITooltip> Tooltips = new Dictionary<string, PUITooltip>();

        public static void Setup(GraphicsDevice graphics, DynamicSpriteFont font)
        {
            Graphics = graphics;
            DefaultFont = font;
        }

        public static void Update(GameTime gameTime)
        {
            if (ActiveTooltip == null)
                return;
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (ActiveTooltip == null)
                return;

            var mousePos = MouseManager.GetMousePosition();

            spriteBatch.Draw(ActiveTooltip.Texture, mousePos, Color.White);
        }

        public static void AddTooltipFromTexture(string name, Texture2D texture)
        {
            if (Tooltips.ContainsKey(name))
                return;
        }

        public static void AddTextTooltip(string name, string text, int size = 26, int padding = 15)
        {
            AddTextTooltip(name, text, DefaultColor, DefaultTextColor, size, padding);
        }

        public static void AddTextTooltip(string name, string text, Color backgroundColor, Color textColor, int size = 26, int padding = 15)
        {
            AddTextTooltip(name, text, backgroundColor, textColor, DefaultFont, size, padding);
        }

        public static void AddTextTooltip(string name, string text, Color backgroundColor, Color textColor, DynamicSpriteFont font, int size = 26, int padding = 15)
        {
            if (Tooltips.ContainsKey(name))
                return;

            font.Size = size;

            var textSize = font.MeasureString(text);
            var newTexture = new RenderTarget2D(Graphics, (int)textSize.X + padding * 2, (int)textSize.Y + padding * 2);

            using (var spriteBatch = new SpriteBatch(Graphics))
            {
                Graphics.SetRenderTarget(newTexture);
                Graphics.Clear(backgroundColor);
                
                spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
                spriteBatch.DrawString(font, text, new Vector2(padding), textColor);
                spriteBatch.End();

                Graphics.SetRenderTarget(null);
            }

            Tooltips.Add(name, new PUITooltip()
            {
                Name = name,
                Texture = newTexture,
            });
        }

        public static void SetActiveTooltip(string name = "")
        {
            if (!Tooltips.ContainsKey(name))
            {
                ActiveTooltip = null;
                return;
            }

            ActiveTooltip = Tooltips[name];
        }

        public static void SetActiveTooltip(PUITooltip tooltip = null)
        {
            ActiveTooltip = tooltip;
        }

        public static void Clear()
        {
            foreach (var kvp in Tooltips)
                kvp.Value.Texture.Dispose();
        }
    }
}
