using System.Diagnostics;

namespace KeyboardHookForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Process.Start("notepad.exe", "keystrokes.txt");

        }
    }
}