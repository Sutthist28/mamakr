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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=127.0.0.1;Port=3306;Database=ramenmamakr;Uid=root;Pwd=;";

            // ตรวจสอบว่ากรอกข้อมูลครบทุกช่อง
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text))
               
            {
                MessageBox.Show("กรุณากรอกให้ครบทุกช่องค่ะ");
                return;
            }
            if (textBox1.Text.Any(c => !char.IsLetter(c) && !char.IsDigit(c) && !char.IsPunctuation(c) && !char.IsWhiteSpace(c)))
            {
                MessageBox.Show("username สามารถมีภาษาอังกฤษ ตัวเลข และสัญลักษณ์บางอย่างได้");
                return;
            }
            // ตรวจสอบว่าเบอร์โทรศัพท์มีความยาว 10 หลักและเป็นตัวเลขเท่านั้น
            if (textBox5.Text.Length != 10 || !textBox5.Text.All(char.IsDigit))
            {
                MessageBox.Show("กรุณากรอกเบอร์มือถือให้ครบ 10 ตัว และต้องเป็นตัวเลขเท่านั้นค่ะ");
                return;
            }

            // ตรวจสอบรหัสผ่าน
            if (textBox2.Text.Length < 8 || !textBox2.Text.All(c => char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                MessageBox.Show("รหัสผ่านต้องมีความยาวอย่างน้อย 8 ตัวและต้องเป็นภาษาอังกฤษ ตัวเลข หรือสัญลักษณ์เท่านั้น");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // ตรวจสอบว่า username มีอยู่ในฐานข้อมูลหรือไม่
                    string checkUsernameQuery = "SELECT COUNT(*) FROM customersmamakr WHERE username = @username";
                    MySqlCommand checkUsernameCommand = new MySqlCommand(checkUsernameQuery, connection);
                    checkUsernameCommand.Parameters.AddWithValue("@username", textBox1.Text);

                    int usernameCount = Convert.ToInt32(checkUsernameCommand.ExecuteScalar());

                    if (usernameCount > 0)
                    {
                        MessageBox.Show("มี username นี้อยู่แล้วกรุณาเปลี่ยน username ด้วยค่ะ");
                        return;
                    }

                    // ตรวจสอบว่าเบอร์โทรศัพท์มีอยู่ในฐานข้อมูลหรือไม่
                    string checkTelQuery = "SELECT COUNT(*) FROM customersmamakr WHERE tel = @Tel";
                    MySqlCommand checkTelCommand = new MySqlCommand(checkTelQuery, connection);
                    checkTelCommand.Parameters.AddWithValue("@Tel", textBox5.Text);

                    int telCount = Convert.ToInt32(checkTelCommand.ExecuteScalar());

                    if (telCount > 0)
                    {
                        MessageBox.Show("หมายเลขโทรศัพท์นี้มีอยู่ในระบบแล้ว");
                        return;
                    }

                    // SQL query สำหรับการ insert ข้อมูล
                    string query = "INSERT INTO customersmamakr (username, password, Name, Email, tel) VALUES (@username, @password, @Name, @Email, @Tel)";

                    // สร้าง MySqlCommand object
                    MySqlCommand command = new MySqlCommand(query, connection);

                    // เพิ่ม parameters พร้อมค่าจาก text boxes
                    command.Parameters.AddWithValue("@username", textBox1.Text);
                    command.Parameters.AddWithValue("@password", textBox2.Text);
                    command.Parameters.AddWithValue("@Name", textBox3.Text);
                    command.Parameters.AddWithValue("@Email", textBox4.Text);
                    command.Parameters.AddWithValue("@Tel", textBox5.Text);  // ตรงนี้เป็นการส่งค่าโทรศัพท์ที่มีเลข 0 นำหน้า
                    

                    // Execute query
                    int rowsAffected = command.ExecuteNonQuery();

                    

                      // กลับไปที่ Form1
                      Form1 form1 = new Form1();
                    form1.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    // Handle ข้อผิดพลาดที่อาจเกิดขึ้น
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1 frm1 = new Form1();
            frm1.Show();
            this.Close();
        }
    }
    
}
