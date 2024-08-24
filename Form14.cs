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
    public partial class Form14 : Form
    {
        public Form14()
        {
            InitializeComponent();
            dateTimePicker1.ValueChanged += new EventHandler(dateTimePicker1_ValueChanged);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // ล้างข้อความที่แสดงก่อนหน้านี้
            label1.Text = "";

            // รับค่าวันที่ที่เลือก
            DateTime selectedDate = dateTimePicker1.Value.Date;

            // กำหนดสตริงการเชื่อมต่อ (แทนที่ด้วยสตริงการเชื่อมต่อจริงของคุณ)
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";

            // กำหนดคำสั่ง SQL เพื่อดึงสินค้าที่ขายดีที่สุด 3 อันดับในวันที่เลือก
            string query = @"
                SELECT Productname, SUM(Amount) AS TotalAmount
                FROM selectsales
                WHERE DATE(Date) = @SelectedDate
                GROUP BY Productname
                ORDER BY TotalAmount DESC
                LIMIT 3";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@SelectedDate", selectedDate);

                try
                {
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    int rank = 1;
                    while (reader.Read())
                    {
                        string productName = reader["Productname"].ToString();
                        int totalAmount = Convert.ToInt32(reader["TotalAmount"]);

                        label1.Text += $"{rank}. {productName}: {totalAmount} ชิ้น \n\n";
                        rank++;
                    }

                    reader.Close();

                    // แสดง Label1 ถ้ามีข้อมูล
                    label1.Visible = !string.IsNullOrEmpty(label1.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Close();
        }
    }
    
}

