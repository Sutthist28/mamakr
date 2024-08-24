using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace mamakr
{
    public partial class Form10 : Form
    {

        public Form10()
        {
            InitializeComponent();

        }



        private void Form10_Load(object sender, EventArgs e)
        {
            ShowSelectedDateHistory();
        }


        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();

            Form6 frm6 = new Form6();
            frm6.Show();
        }

        private void ShowSelectedDateHistory() //ดึงข้อมูลประวัติจากฐานข้อมูลโดยค้นหาด้วยวันที่ที่ผู้ใช้เลือกจาก DateTimePicker และแสดงผลใน DataGridView สำหรับการแสดงประวัติที่มีวันที่ตรงกับที่เลือกไว้
        {
            string selectedDate = dateTimePicker1.Value.Date.ToString("yyyy-MM-dd");
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            string query = "SELECT * FROM selectsales WHERE DATE(date) = @selectedDate";

            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@selectedDate", selectedDate);
                    connection.Open();

                    DataTable dataTable = new DataTable();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;
                }
            }
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ShowSelectedDateHistory();
            UpdateSalesTotals();
        }
        private void UpdateSalesTotals()
        {
            string selectedDate = dateTimePicker1.Value.Date.ToString("yyyy-MM-dd");
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";

            // คำนวณยอดขายรายวัน
            string dailyQuery = "SELECT SUM(totalprice) FROM selectsales WHERE DATE(date) = @selectedDate";

            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(dailyQuery, connection))
                {
                    command.Parameters.AddWithValue("@selectedDate", selectedDate);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    textBox1.Text = result != DBNull.Value ? result.ToString() : "0";
                }
            }

            // คำนวณยอดขายรายเดือน
            string monthlyQuery = "SELECT SUM(totalprice) FROM selectsales WHERE MONTH(date) = MONTH(@selectedDate) AND YEAR(date) = YEAR(@selectedDate)";

            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = new MySqlCommand(monthlyQuery, connection))
                {
                    command.Parameters.AddWithValue("@selectedDate", selectedDate);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    textBox2.Text = result != DBNull.Value ? result.ToString() : "0";
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && decimal.TryParse(textBox1.Text, out decimal value))
            {
                textBox1.Text = value.ToString("#,##0"); // แสดงเลขในรูปแบบหลักพัน
                textBox1.SelectionStart = textBox1.Text.Length; // ตั้งตำแหน่งตัวเลือกที่สิ้นสุดของข้อความ
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text) && decimal.TryParse(textBox2.Text, out decimal value))
            {
                textBox2.Text = value.ToString("#,##0"); // แสดงเลขในรูปแบบหลักพัน
                textBox2.SelectionStart = textBox2.Text.Length; // ตั้งตำแหน่งตัวเลือกที่สิ้นสุดของข้อความ
            }
        }
    }
    
}
