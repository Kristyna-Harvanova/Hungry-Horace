using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.AxHost;
using System.Threading;
using System.Runtime.InteropServices;

namespace HungryHorace
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Adjusting tileSize according to size of display.
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MaximizeBox = false;
            WindowState = FormWindowState.Maximized;
            GameManager.g = CreateGraphics();
            GameManager.Initialize(ClientSize, GameManager.g);
            Controls.Add(GameManager.btnStart);
            Controls.Add(GameManager.btnEnd);
            KeyPreview = true;
            timer1.Enabled = true;
            MessageBox.Show(GameManager.messageLoad, GameManager.titleHelp);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!GameManager.pause)
                GameManager.Update();
        }

        // Controlling: arrow keys, space bar 
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (37 <= e.KeyValue && e.KeyValue <= 40)
                Input.Pressed[e.KeyValue - 37] = true;

            if (e.KeyCode == Keys.Space)
                GameManager.pause = !GameManager.pause;

            if (e.Control && e.KeyCode == Keys.S)
                StateOfGame.Save();

            // Allowed only when EndScreen or StartScreen is dislayed
            if ((GameManager.enemyWin || !GameManager.started) && e.Control && e.KeyCode == Keys.L)
                StateOfGame.Restore();
        }                                    

        private void Form1_KeyUp(object sender, KeyEventArgs e) 
        {
            if (37 <= e.KeyValue && e.KeyValue <= 40)
                Input.Pressed[e.KeyValue - 37] = false;                                            
        }
    }
}
