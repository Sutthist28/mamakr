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

namespace mamakr
{
    public partial class Form8 : Form
    {
        private MySqlConnection databaseConnection()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";

            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        public Form8()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string searchUsername = textBox3.Text.Trim(); // รับค่า username จาก TextBox1 และทำการ Trim() เพื่อลบช่องว่างที่อาจเกิดขึ้น
            if (!string.IsNullOrEmpty(searchUsername))
            {
                //MessageBox.Show("Searching for: " + searchUsername); // แสดงค่า username ที่ใช้ในการค้นหา
                SearchUserAndDisplayData();
            }
            else
            {
                MessageBox.Show("กรุณากรอก Name&lastname เพื่อทำการค้นหา");
            }
        }
        private void SearchUserAndDisplayData()
        {
            string searchName = textBox3.Text.Trim(); // รับค่า Name จาก TextBox3 และทำการ Trim() เพื่อลบช่องว่างที่อาจเกิดขึ้น

            if (string.IsNullOrEmpty(searchName))
            {
                MessageBox.Show("กรุณากรอก Name เพื่อทำการค้นหา");
                return;
            }

            try
            {
                using (MySqlConnection conn = databaseConnection())
                {
                    conn.Open();

                    // เปลี่ยน SQL query เป็นค้นหาตาม Name
                    MySqlCommand cmd = new MySqlCommand("SELECT username, password, Name, Email, Tel FROM customersmamakr WHERE Name = @Name", conn);
                    cmd.Parameters.AddWithValue("@Name", searchName);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // ตรวจสอบค่าที่ดึงมาได้
                        string foundUsername = reader["username"] != DBNull.Value ? reader["username"].ToString() : "";
                        string foundPassword = reader["password"] != DBNull.Value ? reader["password"].ToString() : "";
                        string foundName = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "";
                        string foundEmail = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                        string foundTel = reader["Tel"] != DBNull.Value ? reader["Tel"].ToString() : "";

                        // แสดงค่าที่ได้ใน TextBox ต่างๆ
                        textBox1.Text = foundUsername; // แสดง username
                        textBox2.Text = foundPassword; // แสดง password
                        textBox3.Text = foundName; // แสดง Name (ที่ค้นหา)
                        textBox4.Text = foundEmail; // แสดง Email
                        textBox5.Text = foundTel; // แสดง Tel
                    }
                    else
                    {
                        MessageBox.Show("ไม่พบผู้ใช้งานที่ค้นหา");
                        // ล้างข้อมูลใน TextBoxes ทุกช่อง
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        textBox4.Clear();
                        textBox5.Clear();
                    }

                    reader.Close();
                } // สิ้นสุด using จะปิดการเชื่อมต่ออัตโนมัติ
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการค้นหา: " + ex.Message);
            }
        }

        //private MySqlConnection conn = null;
        private void button2_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = null;

            try
            {
                conn = databaseConnection();
                conn.Open(); // ตรวจสอบว่าการเชื่อมต่อฐานข้อมูลสำเร็จ

                // SQL query สำหรับการอัปเดตข้อมูล
                string sql = "UPDATE customersmamakr SET Name = @NewName, password = @Password, username = @Username, Email = @Email, Tel = @Tel WHERE Name = @OldName";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // เพิ่มพารามิเตอร์และกำหนดค่าจาก TextBoxes
                string newName = string.IsNullOrEmpty(textBox6.Text.Trim()) ? textBox3.Text : textBox6.Text;
                cmd.Parameters.AddWithValue("@NewName", newName);
                cmd.Parameters.AddWithValue("@OldName", textBox3.Text);
                cmd.Parameters.AddWithValue("@Password", textBox2.Text);
                cmd.Parameters.AddWithValue("@Username", textBox1.Text);
                cmd.Parameters.AddWithValue("@Email", textBox4.Text);
                cmd.Parameters.AddWithValue("@Tel", textBox5.Text);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    // ถ้ามีการอัปเดต textBox6 ให้อัปเดตข้อมูลในตารางอื่น ๆ ด้วย
                    if (!string.IsNullOrEmpty(textBox6.Text.Trim()))
                    {
                        // อัปเดต selectsales คอลัมน์ Customername
                        string updateSelectSales = "UPDATE selectsales SET Namelast = @NewUsername WHERE Namelast = @OldUsername";
                        MySqlCommand cmdSelectSales = new MySqlCommand(updateSelectSales, conn);
                        cmdSelectSales.Parameters.AddWithValue("@NewUsername", textBox6.Text);
                        cmdSelectSales.Parameters.AddWithValue("@OldUsername", textBox1.Text);
                        cmdSelectSales.ExecuteNonQuery();

                        // อัปเดต salesproduct คอลัมน์ Name
                        string updateSalesProduct = "UPDATE salesproduct SET Name = @NewUsername WHERE Name = @OldUsername";
                        MySqlCommand cmdSalesProduct = new MySqlCommand(updateSalesProduct, conn);
                        cmdSalesProduct.Parameters.AddWithValue("@NewUsername", textBox6.Text);
                        cmdSalesProduct.Parameters.AddWithValue("@OldUsername", textBox1.Text);
                        cmdSalesProduct.ExecuteNonQuery();

                        // อัปเดต historycustomer
                        string updateHistoryCustomer = "UPDATE historycustomer SET Name = @NewUsername WHERE Name = @OldUsername";
                        MySqlCommand cmdHistoryCustomer = new MySqlCommand(updateHistoryCustomer, conn);
                        cmdHistoryCustomer.Parameters.AddWithValue("@NewUsername", textBox6.Text);
                        cmdHistoryCustomer.Parameters.AddWithValue("@OldUsername", textBox1.Text);
                        cmdHistoryCustomer.ExecuteNonQuery();
                    }

                    MessageBox.Show("แก้ไขข้อมูลสำเร็จ");
                    // เมื่อแก้ไขข้อมูลสำเร็จ ทำการล้างข้อมูลใน TextBoxes ทุกช่อง
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ต้องการแก้ไข");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการเชื่อมต่อฐานข้อมูล: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการแก้ไขข้อมูล: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
    

        private void button3_Click(object sender, EventArgs e)
        {
            string searchUsername = textBox1.Text; // รับค่า username จาก TextBox1
            SearchUserAndDisplayData();

            // ถ้าหากมีข้อมูลผู้ใช้งานที่ค้นหาเจอ
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                DialogResult result = MessageBox.Show("คุณต้องการลบข้อมูลนี้หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    DeleteUserByUsername(textBox1.Text);
                }
            }
        }

        private void DeleteUserByUsername(string username)
        {
            try
            {
                MySqlConnection conn = databaseConnection();
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("DELETE FROM customersmamakr WHERE username = @Username", conn);
                cmd.Parameters.AddWithValue("@Username", username);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("ลบข้อมูลสำเร็จ");
                    // เมื่อแก้ไขข้อมูลสำเร็จ ทำการล้างข้อมูลใน TextBoxes ทุกช่อง
                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";
                    textBox4.Text = "";
                    textBox5.Text = "";
                     // ล้างข้อมูลใน TextBoxes ทุกช่อง
                                        // โหลดข้อมูลลูกค้าใหม่เข้าสู่ DataGridView หรืออื่นๆ ตามที่ต้องการ
                                        // เช่น showCustomers();
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ต้องการลบ");
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการลบข้อมูล: " + ex.Message);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form8_Load(object sender, EventArgs e)
        {

        }
    }
}
