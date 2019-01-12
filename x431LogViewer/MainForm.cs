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

                // read channel count
                fs.Seek(0x134, SeekOrigin.Begin); 
                fs.Read(buffer, 0, 1);
                int columnCount = buffer[0] / 4;

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

                // read point values
                List<string> pointValues = new List<string>();
                while (fs.Position != fs.Length)
                {
                    // read field lenght
                    fs.Read(buffer, 0, 2);
                    var16 = BitConverter.ToInt16(buffer, 0);

                    // read field data
                    fs.Read(buffer, 0, var16 - 2);
                    string value = Encoding.ASCII.GetString(buffer, 0, var16 - 3);
                    pointValues.Add(value);
                }

                string[] columnNames = new string[columnCount + 1];
                columnNames[0] = "Rec. num";
                fs.Seek(0x138, SeekOrigin.Begin);
                // read column headers
                for(int i = 0; i < columnCount; i++)
                {
                    // read column description
                    fs.Read(buffer, 0, 4);
                    var32 = BitConverter.ToInt16(buffer, 0);

                    if (var32 == 0) continue;

                    columnNames[i + 1] = string.Format("{0}. {1}", i+1, pointValues[var32 - 0x09]);
                }

                // read column headers
                for (int i = 0; i < columnCount; i++)
                {
                    // read column description
                    fs.Read(buffer, 0, 4);
                    var32 = BitConverter.ToInt16(buffer, 0);

                    if (var32 == 0) continue;

                    columnNames[i + 1] = string.Format("{0} ({1})", columnNames[i+1], pointValues[var32 - 0x09]);
                }

                DataTable table = new DataTable();
                foreach (string columnName in columnNames)
                {
                    DataColumn column = new DataColumn(columnName);
                    table.Columns.Add(column);
                }
                table.Columns[0].DataType = Type.GetType("System.Int32");

                // read data 
                fs.Seek(0x11c, SeekOrigin.Begin);
                fs.Read(buffer, 0, 2);
                var16 = BitConverter.ToInt16(buffer, 0);

                // Read data lenght
                fs.Seek(var16 + 8, SeekOrigin.Begin);
                fs.Read(buffer, 0, 8);
                int recordsCount = BitConverter.ToInt32(buffer, 0);

                for(int i = 0; i < ((recordsCount / 4) / columnCount); i++)
                {
                    DataRow row = table.NewRow();
                    row[0] = i + 1;
                    for(int j = 0; j < columnCount; j++)
                    {
                        fs.Read(buffer, 0, 4);
                        var16 = BitConverter.ToInt16(buffer, 0);
                        row[j + 1] = pointValues[var16 - 0x09];
                    }

                    table.Rows.Add(row);
                }

                dataGridView1.DataSource = table;
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(dataGridView1.DataSource is DataTable))
                return;

            saveFileDialog.FileName = _filePath.Text + ".csv";

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
            {
                DataTable table = (DataTable)dataGridView1.DataSource;

                foreach (DataColumn column in table.Columns)
                {
                    sw.Write(string.Format("\"{0}\",", column.ColumnName));
                }
                sw.WriteLine();

                foreach (DataRow row in table.Rows)
                {
                    foreach (object item in row.ItemArray)
                    {
                        sw.Write(string.Format("{0},", Convert.ToString(item)));
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
