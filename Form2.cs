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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;
            // ตรวจสอบ
            if (username.ToLower() == "admin" && password == "2567")
            {
                Form6 frm6 = new Form6();
                frm6.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // จบการทำงานของฟังก์ชันที่นี่
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Hide();
        }
    }
}
