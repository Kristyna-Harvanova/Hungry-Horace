using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HungryHorace
{
    /// <summary>
    /// For objects of application.
    /// </summary>
    class FormObject    
    {
        public Bitmap bitmap; 
        public Graphics bitmapGraphics;

        // Size of the object in pixels.
        public Size size; 
        public Color backgroundColor;

        public FormObject() { }

        public FormObject(int width, int height, Color backgroundColor)
        {
            size = new Size(width, height);
            Initialize(backgroundColor);
        }

        /// <summary>
        /// Initial settings.
        /// </summary>
        /// <param name="backGroundColor"></param>
        protected void Initialize(Color backGroundColor)
        {
            backgroundColor = backGroundColor;

            // Setting map to required size.
            bitmap = new Bitmap(size.Width, size.Height);
            bitmapGraphics = Graphics.FromImage(bitmap);  
        }

        /// <summary>
        /// Redraw when new information.
        /// </summary>
        /// <param name="g"></param>
        public virtual void Draw(Graphics g)
        {
            bitmapGraphics.Clear(backgroundColor);
        }
    }

    /// <summary>
    /// Appears at the start of the game.
    /// </summary>
    class StartScreen : FormObject
    {
        public StartScreen(int width, int height, Color backgroundColor) : base(width, height, backgroundColor) { }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            bitmapGraphics.FillRectangle(GameManager.backgroundBrush, 0, 0, size.Width, size.Height);
            StringFormat sf = new StringFormat();   
            sf.Alignment = StringAlignment.Center;  

            /*
            // For English version:
            bitmapGraphics.DrawString("Hungry Herbert", new Font(FontFamily.GenericMonospace, 3 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("Avoid enemies and", new Font(FontFamily.GenericMonospace, GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 3 * GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("collect as many points as you can!", new Font(FontFamily.GenericMonospace, GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 4 * GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("In case of emergency you can hit space bar to stop the game.", new Font(FontFamily.GenericMonospace, GameManager.tileSize / 2), Brushes.RosyBrown, size.Width / 2, 5 * GameManager.headerHeight * GameManager.tileSize, sf);
            */

            bitmapGraphics.DrawString("Hladovec Herbert", new Font(FontFamily.GenericMonospace, 3 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("Vyhýbejte se nepřátelům", new Font(FontFamily.GenericMonospace, GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 3 * GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("a nasbírejte přitom co nejvíce bodů!", new Font(FontFamily.GenericMonospace, GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 4 * GameManager.headerHeight * GameManager.tileSize, sf);
            bitmapGraphics.DrawString("V případě potřeby můžete hru pozastavit stiskem klávesy mezerníku.", new Font(FontFamily.GenericMonospace, GameManager.tileSize / 2), Brushes.RosyBrown, size.Width / 2, 5 * GameManager.headerHeight * GameManager.tileSize, sf);

            g.DrawImage(bitmap, 0, 0);
        }
    }

    /// <summary>
    /// Displays information all the time when playing.
    /// </summary>
    class Header : FormObject
    {
        public Header(int width, int height, Color backgroundColor) : base(width, height, backgroundColor) { }

        public override void Draw(Graphics g)
        {
            base.Draw(g);

            StringFormat sf = new StringFormat();   
            sf.Alignment = StringAlignment.Center;  
            sf.LineAlignment = StringAlignment.Far;

            /*
            // For English version:
            bitmapGraphics.DrawString($"Score: {GameManager.scoreTotal + GameManager.scoreCurrentLevel}", new Font(FontFamily.GenericMonospace, 2 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, size.Height, sf);
            */

            bitmapGraphics.DrawString($"Skóre: {GameManager.scoreTotal + GameManager.scoreCurrentLevel}", new Font(FontFamily.GenericMonospace, 2 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, size.Height, sf);

            // Centering of header.
            g.DrawImage(bitmap, GameManager.gameOffset.x, 0);   
        }
    }

    /// <summary>
    /// Appears at the end of game.
    /// </summary>
    class EndScreen : FormObject
    {
        public EndScreen(int width, int height, Color backgroundColor) : base(width, height, backgroundColor) { }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            bitmapGraphics.FillRectangle(GameManager.backgroundBrush, 0, 0, size.Width, size.Height);

            StringFormat sf = new StringFormat();  
            sf.Alignment = StringAlignment.Center; 
            sf.LineAlignment = StringAlignment.Center;

            /*
            // For English version:
            bitmapGraphics.DrawString("Your game ended!", new Font(FontFamily.GenericMonospace, 3 * GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 4 * GameManager.tileSize, sf);
            bitmapGraphics.DrawString($"Your score: {GameManager.scoreTotal}", new Font(FontFamily.GenericMonospace, 2 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, 10 * GameManager.tileSize, sf);
            bitmapGraphics.DrawString($"Highest score: {GameManager.highestScore}", new Font(FontFamily.GenericMonospace, 2 * GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 14 * GameManager.tileSize, sf);
            */

            bitmapGraphics.DrawString("Vaše hra skončila!", new Font(FontFamily.GenericMonospace, 3 * GameManager.tileSize), GameManager.stringBrush, size.Width / 2, 4 * GameManager.tileSize, sf);
            bitmapGraphics.DrawString($"Vaše skóre: {GameManager.scoreTotal}", new Font(FontFamily.GenericMonospace, 2 * GameManager.tileSize, FontStyle.Bold), GameManager.stringBrush, size.Width / 2, 10 * GameManager.tileSize, sf);
            bitmapGraphics.DrawString($"Nejvyšší skóre: {GameManager.highestScore}", new Font(FontFamily.GenericMonospace, 3 * GameManager.tileSize / 2), GameManager.stringBrush, size.Width / 2, 14 * GameManager.tileSize, sf);

            g.DrawImage(bitmap, 0, 0);
        }
    }
}
