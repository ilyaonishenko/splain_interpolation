using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication7
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                Commons.SyncWay = Sync.Semaphore;
            else if (radioButton2.Checked)
                Commons.SyncWay = Sync.Mutex;
            else
                Commons.SyncWay = Sync.Event;
            this.Hide();
        }
    }
}
