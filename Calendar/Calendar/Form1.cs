using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
//using System.Runtime.InteropServices;

namespace Calendar
{
    public partial class Form1 : Form
    {

        Calendar cal;
        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //Display calendar in notepad
            string s = textBox2.Text.ToString();
            if (s == string.Empty)
            {
                MessageBox.Show("Please enter a year", "Year missing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox2.Focus();
                return;
            }
            Int32 number;
            bool success = Int32.TryParse(s, out number);
            if (!success)
            {
                MessageBox.Show("Please enter a year", "Year missing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textBox2.Focus();
                return;

            }
            calendarToNotepad(s);

        }

        private void calendarToNotepad(string year)
        {
            cal = new Calendar(year);
            this.Text = "Calendar " + year.ToString(); 
            textBox1.Text = cal.GetNotepadText();

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            int year = DateTime.Now.Year;
                       
            string s = textBox2.Text.ToString();
            if (s == string.Empty)
            {
                s = year.ToString();
                textBox2.Text = s;
            }
            this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2,
                          (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);
            calendarToNotepad(s);
            textBox1.Select(0, 0);

            
        }

        // This is the event handler that does the actual printing.
        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            //Font
            Font f = new System.Drawing.Font("Consolas", 9, FontStyle.Regular);

            //Brush
            Brush b = new SolidBrush(Color.Black);

            //Where to draw the string
            PointF p = new PointF(16, 10);

            //Draw some strings into the graphics
            e.Graphics.DrawString(textBox1.Text, f, b, p);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            PrintDocument printDoc = new PrintDocument();

            //PrintPage event to draw the textbox contents on page
            printDoc.PrintPage += new PrintPageEventHandler(printDoc_PrintPage);

            printDoc.DocumentName = "Print";

            printDialog.Document = printDoc;
            printDialog.AllowSelection = true;

            printDialog.AllowSomePages = true;

            if (printDialog.ShowDialog() == DialogResult.OK)
                printDoc.Print();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            textBox2.Focus();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char) Keys.Enter)
            {
                button1_Click(this, new EventArgs());
                e.Handled = true;
            }

        }
    }
}
