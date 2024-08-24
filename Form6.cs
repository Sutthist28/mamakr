using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mamakr
{
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form12 frm12 = new Form12();
            frm12.Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form13 frm13 = new Form13();
            frm13.Show();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form8 frm8 = new Form8();
            frm8.Show();
            this.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Form7 frm7 = new Form7();
            frm7.Show();
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form9 frm9 = new Form9();
            frm9.Show();
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form11 frm11 = new Form11();
            frm11.Show();
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form10 frm10 = new Form10();
            frm10.Show();
            this.Close();
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            Form14 frm14 = new Form14();
            frm14.Show();
            this.Close();
        }
    }
}
