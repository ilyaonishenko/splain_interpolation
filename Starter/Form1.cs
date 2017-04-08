using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Starter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateProcessHelper.CreateProcess("C:\\Users\\veryoldbarny\\Documents\\WindowsFormsApplication7.exe", String.Empty);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
