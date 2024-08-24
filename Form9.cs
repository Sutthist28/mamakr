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
using System.IO;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace mamakr
{
    public partial class Form9 : Form
    {
        //private byte[] imageData;
        private MySqlConnection databaseConnection()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        public Form9()
        {
            InitializeComponent();
        }
        private void showProduct()
        {
            MySqlConnection conn = databaseConnection();
            DataSet ds = new DataSet();
            conn.Open();

            MySqlCommand cmd;
            cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM manageproducts";

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(ds);

            conn.Close();
            dataGridView1.DataSource = ds.Tables[0].DefaultView;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Close();
        }

        private void Form9_Load(object sender, EventArgs e)
        {
            showProduct();
        }
        //private string selectedImagePath;

        private void button4_Click(object sender, EventArgs e)
        {   // สร้างอ็อบเจ็กต์ OpenFileDialog เพื่อให้ผู้ใช้เลือกไฟล์
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // ตั้งค่าฟิลเตอร์เพื่อให้แสดงเฉพาะไฟล์ภาพที่มีนามสกุล .jpg, .jpeg, .png, หรือ .bmp
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp";
            // แสดงกล่องโต้ตอบให้ผู้ใช้เลือกไฟล์
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;// ดึงเส้นทางของไฟล์ที่เลือก
                    Image selectedImage = Image.FromFile(imagePath);// โหลดภาพจากไฟล์ที่เลือก

                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Image = selectedImage;// แสดงภาพใน PictureBox
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // อ่านรูปภาพจาก PictureBox เป็น byte array
                byte[] imageData;
                using (MemoryStream ms = new MemoryStream())
                {
                    pictureBox1.Image.Save(ms, ImageFormat.Jpeg); // หรือใช้รูปแบบภาพอื่นตามต้องการ
                    imageData = ms.ToArray();
                }

                // เชื่อมต่อฐานข้อมูล
                MySqlConnection conn = databaseConnection();
                conn.Open();

                // เตรียมคำสั่ง SQL สำหรับเพิ่มข้อมูลสินค้า
                string insertSql = "INSERT INTO manageproducts(Product, Quantity, Price, picproduct) VALUES(@product, @quantity, @price, @pic)";
                MySqlCommand insertCmd = new MySqlCommand(insertSql, conn);

                // กำหนดค่าพารามิเตอร์
                insertCmd.Parameters.AddWithValue("@product", textBox1.Text);
                insertCmd.Parameters.AddWithValue("@quantity", textBox2.Text);
                insertCmd.Parameters.AddWithValue("@price", textBox3.Text);
                insertCmd.Parameters.AddWithValue("@pic", imageData); // ใส่ข้อมูลรูปภาพในพารามิเตอร์

                // ประมวลผลคำสั่ง SQL
                int rowsAffected = insertCmd.ExecuteNonQuery();

                // ปิดการเชื่อมต่อฐานข้อมูล
                conn.Close();

                // ตรวจสอบการบันทึกข้อมูล
                if (rowsAffected > 0)
                {
                    MessageBox.Show("เพิ่มรายการสำเร็จ");
                    showProduct();
                }
                else
                {
                    MessageBox.Show("ไม่สามารถบันทึกข้อมูลได้");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {   // ดึงหมายเลขแถวที่เลือกจาก DataGridView
                int selectRow = dataGridView1.CurrentCell.RowIndex;
                int deleteId = Convert.ToInt32(dataGridView1.Rows[selectRow].Cells["ID"].Value);// ดึงค่า ID จากเซลล์ของแถวที่เลือก

                MySqlConnection conn = databaseConnection();
                string sql = "DELETE FROM manageproducts WHERE ID = @deleteId"; // ใช้พารามิเตอร์ในการระบุ ID ที่ต้องการลบ
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@deleteId", deleteId); // กำหนดค่าให้กับพารามิเตอร์

                conn.Open();

                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                if (rows > 0)
                {
                    MessageBox.Show("ลบข้อมูลสำเร็จ");
                    showProduct();
                }
                else
                {
                    MessageBox.Show("ไม่พบข้อมูลที่ต้องการลบ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedRow = dataGridView1.CurrentCell.RowIndex; // ดึงหมายเลขแถวที่เลือกใน DataGridView
                int editId = Convert.ToInt32(dataGridView1.Rows[selectedRow].Cells["ID"].Value);// ดึงค่า ID จากเซลล์ของแถวที่เลือก

                MySqlConnection conn = databaseConnection();
                conn.Open();

                // สร้างคำสั่ง SQL เพื่อตรวจสอบชื่อสินค้าที่ซ้ำกัน
                MySqlCommand checkDuplicateCmd = conn.CreateCommand();
                checkDuplicateCmd.CommandText = "SELECT COUNT(*) FROM manageproducts WHERE Product = @product AND ID != @id";
                checkDuplicateCmd.Parameters.AddWithValue("@product", textBox2.Text); // กำหนดค่าพารามิเตอร์สำหรับคำสั่ง SQL
                checkDuplicateCmd.Parameters.AddWithValue("@id", editId);
                // ดำเนินการคำสั่ง SQL และรับผลลัพธ์
                int count = Convert.ToInt32(checkDuplicateCmd.ExecuteScalar());
                // ตรวจสอบว่ามีชื่อสินค้าซ้ำกันหรือไม่
                if (count > 0)
                {   // แสดงข้อความหากชื่อสินค้าซ้ำ
                    MessageBox.Show("ชื่อสินค้าซ้ำกับที่มีอยู่ในระบบแล้ว");
                    conn.Close();
                    return;
                }

                // Update product name
                MySqlCommand updateNameCmd = conn.CreateCommand();
                updateNameCmd.CommandText = "UPDATE manageproducts SET Product = @name WHERE ID = @id";
                updateNameCmd.Parameters.AddWithValue("@name", textBox1.Text);
                updateNameCmd.Parameters.AddWithValue("@id", editId);
                int nameRowsAffected = updateNameCmd.ExecuteNonQuery();

                // Update price
                MySqlCommand updatePriceCmd = conn.CreateCommand();
                updatePriceCmd.CommandText = "UPDATE manageproducts SET Price = @price WHERE ID = @id";
                updatePriceCmd.Parameters.AddWithValue("@price", textBox3.Text);
                updatePriceCmd.Parameters.AddWithValue("@id", editId);
                int priceRowsAffected = updatePriceCmd.ExecuteNonQuery();

                // Update quantity
                MySqlCommand updateQuantityCmd = conn.CreateCommand();
                updateQuantityCmd.CommandText = "UPDATE manageproducts SET Quantity = @quantity WHERE ID = @id";
                updateQuantityCmd.Parameters.AddWithValue("@quantity", textBox2.Text);
                updateQuantityCmd.Parameters.AddWithValue("@id", editId);
                int quantityRowsAffected = updateQuantityCmd.ExecuteNonQuery();

                // Update image
                if (pictureBox1.Image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Create a temporary Bitmap to avoid GDI+ error
                        using (Bitmap tempBitmap = new Bitmap(pictureBox1.Image))
                        {
                            tempBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        byte[] imageData = ms.ToArray();

                        MySqlCommand updateImageCmd = conn.CreateCommand();
                        updateImageCmd.CommandText = "UPDATE manageproducts SET Picproduct = @imageData WHERE ID = @id";
                        updateImageCmd.Parameters.AddWithValue("@imageData", imageData);
                        updateImageCmd.Parameters.AddWithValue("@id", editId);
                        int imageRowsAffected = updateImageCmd.ExecuteNonQuery();
                    }
                }

                conn.Close();

                MessageBox.Show("อัปเดตข้อมูลสำเร็จ");
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }

        private void dataGridview1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // กำหนดค่า TextBox จากคอลัมน์ที่ถูกเลือกใน DataGridView
                textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["Product"].FormattedValue.ToString();
                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["Quantity"].FormattedValue.ToString();
                textBox3.Text = dataGridView1.Rows[e.RowIndex].Cells["Price"].FormattedValue.ToString();

                // ตรวจสอบว่ามีการเลือกคอลัมน์ที่มีรูปภาพหรือไม่
                string columnName = "Picproduct"; // Column name storing the image
                if (dataGridView1.Columns.Contains(columnName))
                {
                    // ตรวจสอบว่าเซลล์ที่เลือกมีค่ารูปภาพหรือไม่
                    if (dataGridView1.Rows[e.RowIndex].Cells[columnName].Value != null)
                    {
                        byte[] imageData = (byte[])dataGridView1.Rows[e.RowIndex].Cells[columnName].Value;
                        if (imageData.Length > 0)
                        {
                            try
                            {
                                using (MemoryStream ms = new MemoryStream(imageData))
                                {
                                    Image img = Image.FromStream(ms);
                                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                                    pictureBox1.Image = img;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error displaying image: " + ex.Message);
                            }
                        }
                    }
                }
            }

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

