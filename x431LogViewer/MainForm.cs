using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace x431LogViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _filePath.Text = openFileDialog.FileName;

            using (FileStream fs = new FileStream(_filePath.Text, FileMode.Open))
            {
                byte[] buffer = new byte[128];

                byte var8 = 0;
                short var16 = 0;
                int var32 = 0;

                fs.Seek(0x134, SeekOrigin.Begin); // read channel count
                fs.Read(buffer, 0, 1);
                int columnsCount = buffer[0] / 4;

                fs.Seek(0x0c, SeekOrigin.Begin); // seek to the first lenght field

                fs.Read(buffer, 0, 4);
                var32 = BitConverter.ToInt32(buffer, 0);
                fs.Seek(var32, SeekOrigin.Current);

                for (int i = 0; i < 8; i++) // skip several headers
                {
                    fs.Read(buffer, 0, 2);
                    var16 = BitConverter.ToInt16(buffer, 0);
                    fs.Seek(var16 - 2, SeekOrigin.Current);
                }

                // Read measurment headers

            }
        }
    }
}
