using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mamakr
{
    public partial class Form11 : Form
    {
        private MySqlConnection databaseConnection()
        {
            String connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;charset=utf8;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }
            public Form11()
        {
            InitializeComponent();
        }
        private void showhistoryCus()
        {
            MySqlConnection conn = databaseConnection();
            DataSet ds = new DataSet();
            conn.Open();

            MySqlCommand cmd;

            cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM selectsales";

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(ds);

            conn.Clone();

            dataGridView1.DataSource = ds.Tables[0].DefaultView;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Close();
        }

        private void Form11_Load(object sender, EventArgs e)
        {
            showhistoryCus();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Receipt"].Index && e.RowIndex >= 0)
            {
                // ดึง URL ของไฟล์ PDF จาก DataGridView
                string pdfFilePath = dataGridView1.Rows[e.RowIndex].Cells["Receipt"].Value.ToString();

                // เปิดไฟล์ PDF ด้วยโปรแกรมที่เปิดไฟล์ PDF ในระบบ
                try
                {
                    Process.Start(pdfFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถเปิดไฟล์ PDF: " + ex.Message);
                }
            }
        }
        private void searchCustomer(string customerName)
        {
            MySqlConnection conn = databaseConnection();
            DataSet ds = new DataSet();

            try
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM selectsales WHERE Namelast LIKE @CustomerName";
                cmd.Parameters.AddWithValue("@CustomerName", "%" + customerName + "%");

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(ds);

                dataGridView1.DataSource = ds.Tables[0].DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการค้นหา: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string customerName = textBox1.Text.Trim();

            if (!string.IsNullOrEmpty(customerName))
            {
                searchCustomer(customerName);
            }
            else
            {
                showhistoryCus(); // แสดงทั้งหมดหาก TextBox ไม่มีข้อความ
            }
        }
    }
}
