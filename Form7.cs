using MySql.Data.MySqlClient;
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
    public partial class Form7 : Form
    {
        private MySqlConnection databaseConnection()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";

            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        public Form7()
        {
            InitializeComponent();
        }
        private void showCustomers()
        {
            MySqlConnection conn = databaseConnection();
            DataSet ds = new DataSet();
            conn.Open();

            MySqlCommand cmd;

            cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM customersmamakr";

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(ds);

            conn.Close();
            dataGridView1.DataSource = ds.Tables[0].DefaultView;
        }
        private void Form11_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

            Form6 frm6 = new Form6();
            frm6.Show();
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            showCustomers();
        }
        private void searchCustomer(string searchTerm)
        {
            MySqlConnection conn = databaseConnection();
            DataSet ds = new DataSet();

            try
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Customersmamakr WHERE Name LIKE @SearchTerm OR Tel LIKE @SearchTerm OR Email LIKE @SearchTerm";
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

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
            string searchTerm = textBox1.Text.Trim();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchCustomer(searchTerm);
            }
            else
            {
                showCustomers(); // แสดงทั้งหมดหาก TextBox ไม่มีข้อความ
            }
        }
    }
}
