using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Reflection.Emit;


namespace mamakr
{
    public partial class Form1 : Form
    {
        private MySqlConnection databaseConnection()
        {
            String connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }
        public Form1()
        {
            InitializeComponent();
            textBox2.PasswordChar = '*';    

        }
        
        
        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("กรุณากรอกชื่อผู้ใช้และรหัสผ่าน");
                return;
            }

            try
            {
                using (MySqlConnection conn = databaseConnection())
                {
                    conn.Open();

                    string query = "SELECT Name, จำนวนครั้งที่เข้าใช้บริการ FROM customersmamakr WHERE username = @username AND password = @password";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader.GetString("Name");
                                int loginCount = reader.GetInt32("จำนวนครั้งที่เข้าใช้บริการ");

                                reader.Close();  // ปิด reader ก่อนทำการอัปเดตฐานข้อมูล

                                // อัปเดตจำนวนครั้งการเข้าใช้บริการในฐานข้อมูล
                                string updateQuery = "UPDATE customersmamakr SET จำนวนครั้งที่เข้าใช้บริการ = จำนวนครั้งที่เข้าใช้บริการ + 1 WHERE username = @username AND password = @password";
                                using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@username", username);
                                    updateCmd.Parameters.AddWithValue("@password", password);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // ส่งข้อมูลไปที่ Form4 และแสดง Form4
                                Form4 form4 = new Form4(username, name);
                                form4.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลผู้ใช้นี้ในระบบ กรุณาสมัครสมาชิกก่อน");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}");
            }

        }



        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 frm3 = new Form3();
            frm3.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();

            Form2 frm2 = new Form2();
            frm2.Show();
        }
        public class Customer
        {
            public string Name { get; set; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่า TextBox ที่เกี่ยวข้องกับ password char ถูกแสดงอยู่หรือไม่
            if (textBox2.PasswordChar == '\0')
            {
                // ถ้าไม่ได้แสดง password char ให้แสดงเป็น '*'
                textBox2.PasswordChar = '*';
            }
            else
            {
                // ถ้าแสดง password char อยู่แล้ว ให้กำหนดให้ไม่แสดง
                textBox2.PasswordChar = '\0';
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
