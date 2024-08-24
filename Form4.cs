using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static mamakr.Form1;


namespace mamakr
{
    public partial class Form4 : Form
    {
        private string loggedInCustomer;
        private string customerName;

        private MySqlConnection databaseConnection()
        {
            String connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        public Form4(string username, string name)
        {
            InitializeComponent();
            loggedInCustomer = username;
            customerName = name;
            UpdateCustomerInfo();
        }

        private void UpdateCustomerInfo()
        {
            // สมมติว่าคุณมี Label ชื่อ label1 บน Form4
            label1.Text = customerName;
        }


        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text) && int.TryParse(textBox2.Text, out int quantity) && quantity >= 0)
            {
                using (MySqlConnection conn = databaseConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("SELECT Quantity, Price FROM manageproducts WHERE Product = @Product", conn);
                    cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int availableQuantity = reader.GetInt32(0);
                        double price = reader.GetDouble(1);
                        reader.Close();

                        if (quantity <= availableQuantity)
                        {
                            double totalPrice = quantity * price;

                            cmd = new MySqlCommand("SELECT Quantity, Price FROM salesproduct WHERE Name = @CustomerName AND Namelast = @Namelast AND Product = @Product", conn);
                            cmd.Parameters.AddWithValue("@CustomerName", loggedInCustomer);
                            cmd.Parameters.AddWithValue("@Namelast", customerName);
                            cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                            reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                int existingQuantity = reader.GetInt32(0);
                                double existingPrice = reader.GetDouble(1);
                                reader.Close();

                                int newQuantity = existingQuantity + quantity;
                                double newPrice = existingPrice + totalPrice;

                                cmd = new MySqlCommand("UPDATE salesproduct SET Quantity = @Quantity, Price = @Price WHERE Name = @CustomerName AND Namelast = @Namelast AND Product = @Product", conn);
                                cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                                cmd.Parameters.AddWithValue("@Price", newPrice);
                                cmd.Parameters.AddWithValue("@CustomerName", loggedInCustomer);
                                cmd.Parameters.AddWithValue("@Namelast", customerName);
                                cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                                cmd.ExecuteNonQuery();
                            }
                            else
                            {
                                reader.Close();
                                cmd = new MySqlCommand("INSERT INTO salesproduct (Name, Namelast, Product, Quantity, Price) VALUES (@CustomerName, @Namelast, @Product, @Quantity, @Price)", conn);
                                cmd.Parameters.AddWithValue("@CustomerName", loggedInCustomer);
                                cmd.Parameters.AddWithValue("@Namelast", customerName);
                                cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                                cmd.Parameters.AddWithValue("@Quantity", quantity);
                                cmd.Parameters.AddWithValue("@Price", totalPrice);
                                cmd.ExecuteNonQuery();
                            }

                            cmd = new MySqlCommand("UPDATE manageproducts SET Quantity = Quantity - @Quantity WHERE Product = @Product", conn);
                            cmd.Parameters.AddWithValue("@Quantity", quantity);
                            cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                            cmd.ExecuteNonQuery();

                            //MessageBox.Show("ทำรายการสำเร็จ");
                            showSaleProduct();
                            ShowReceiptFromDatabase();
                            UpdateTotalPrice();
                            textBox1.Clear();
                            textBox2.Clear();
                        }
                        else
                        {
                            MessageBox.Show($"Stock ไม่พอ สินค้าที่เหลือเพียง {availableQuantity} ชิ้น, ขออภัยด้วยค่ะ");

                        }
                    }
                    else
                    {
                        MessageBox.Show("ไม่พบสินค้าที่ต้องการ");
                    }
                }
            }
            else
            {
                MessageBox.Show("กรุณากรอกข้อมูลที่ถูกต้อง");
            }

        }


        private void Form4_Load(object sender, EventArgs e)
        {
            //label1.Visible = false;
            ShowReceiptFromDatabase(); // เรียกใช้เมทอดเพื่อแสดงข้อมูลใบเสร็จจากฐานข้อมูล
            showSaleProduct();
            
        }
        private void showSaleProduct()
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
            // ซ่อนคอลัมน์ไอดี
            //dataGridView1.Columns["ID"].Visible = false;
        }
        private void ShowReceiptFromDatabase()
        {
            string connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            try
            {
                if (loggedInCustomer != null)
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        string selectReceiptQuery = "SELECT Name, Product, Quantity, Price, Date FROM salesproduct WHERE Name = @CustomerName";
                        MySqlCommand receiptCommand = new MySqlCommand(selectReceiptQuery, connection);
                        receiptCommand.Parameters.AddWithValue("@CustomerName", loggedInCustomer);

                        DataTable dataTable = new DataTable();
                        dataTable.Load(receiptCommand.ExecuteReader());

                        if (dataTable.Rows.Count > 0)
                        {
                            dataGridView2.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {   
            // แสดงกล่องข้อความยืนยันการลบรายการสินค้า
            if (!string.IsNullOrEmpty(textBox2.Text) && int.TryParse(textBox2.Text, out int quantity) && quantity > 0)
            {
                DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการยกเลิกรายการสินค้านี้?", "ยืนยันการลบรายการ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // สร้างการเชื่อมต่อกับฐานข้อมูล
                    using (MySqlConnection conn = databaseConnection())
                    {
                        conn.Open();

                        // สร้างคำสั่ง SQL เพื่อดึงข้อมูล Quantity, ID และ Price จากตาราง salesproduct
                        MySqlCommand cmd = new MySqlCommand("SELECT Quantity, ID, Price FROM salesproduct WHERE Name = @CustomerName AND Namelast = @Namelast AND Product = @Product", conn);
                        cmd.Parameters.AddWithValue("@CustomerName", loggedInCustomer);
                        cmd.Parameters.AddWithValue("@Namelast", customerName);
                        cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                        MySqlDataReader reader = cmd.ExecuteReader();


                        if (reader.Read())
                        {
                            // ดึงข้อมูลจำนวนสินค้าที่มีอยู่, ID และราคาต่อชิ้น
                            int existingQuantity = reader.GetInt32(0); // ดึงจำนวนสินค้าที่มีอยู่
                            int productId = reader.GetInt32(1); // ดึงไอดี
                            double pricePerUnit = reader.GetDouble(2); // ดึงราคาต่อชิ้น
                            reader.Close();

                            if (quantity <= existingQuantity)
                            {
                                // ดึง the price per ชิ้น from manageproducts
                                double priceFromManage = GetPriceFromManageProducts(textBox1.Text);
                                double priceToSubtract = priceFromManage * quantity; // Calculate the total price to subtract

                                // Update Quantity and Price in salesproduct table
                                if (existingQuantity - quantity > 0)
                                {
                                    cmd = new MySqlCommand("UPDATE salesproduct SET Quantity = Quantity - @Quantity, Price = Price - @Price WHERE ID = @ProductID", conn);
                                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.Parameters.AddWithValue("@Price", priceToSubtract);
                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    // Remove the product if quantity becomes 0
                                    cmd = new MySqlCommand("DELETE FROM salesproduct WHERE ID = @ProductID", conn);
                                    cmd.Parameters.AddWithValue("@ProductID", productId);
                                    cmd.ExecuteNonQuery();
                                }

                                // Add cancelled quantity back to manageproducts table (though this should logically not affect manageproducts, it's here for consistency)
                                cmd = new MySqlCommand("UPDATE manageproducts SET Quantity = Quantity + @Quantity WHERE Product = @Product", conn);
                                cmd.Parameters.AddWithValue("@Quantity", quantity);
                                cmd.Parameters.AddWithValue("@Product", textBox1.Text);
                                cmd.ExecuteNonQuery();

                                // ลบราคาของสินค้าที่ถูกยกเลิกออกจากราคาทั้งหมด
                                double totalPriceToSubtract = quantity * pricePerUnit; // Total price including VAT
                                UpdateTotalPrice(); // Update total price displayed

                                //MessageBox.Show("ยกเลิกรายการสินค้าสำเร็จ");
                                ShowReceiptFromDatabase(); // Refresh the DataGridView
                                textBox1.Clear();
                                textBox2.Clear();
                            }
                            else
                            {
                                MessageBox.Show("จำนวนที่ยกเลิกมากกว่าจำนวนที่มีอยู่");
                            }
                        }
                        else
                        {
                            reader.Close();
                            MessageBox.Show("ไม่พบรายการสินค้าที่ต้องการยกเลิก");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("โปรดป้อนจำนวนที่ถูกต้องในรูปแบบของตัวเลขบวก");
            }
        }

        private double GetPriceFromManageProducts(string productName)
        {
            MySqlConnection conn = databaseConnection();
            conn.Open();
            // สร้างคำสั่ง SQL เพื่อดึงราคา (Price) จากตาราง manageproducts โดยใช้ชื่อผลิตภัณฑ์ (Product) เป็นพารามิเตอร์
            MySqlCommand cmd = new MySqlCommand("SELECT Price FROM manageproducts WHERE Product = @Product", conn);
            cmd.Parameters.AddWithValue("@Product", productName);
            object result = cmd.ExecuteScalar();// ดำเนินการคำสั่ง SQL และดึงค่าราคา (Price) ที่ส่งกลับมา

            conn.Close();
            // ตรวจสอบผลลัพธ์ที่ได้รับ
            if (result != null && result != DBNull.Value)
            {   // แปลงผลลัพธ์เป็น double และส่งคืน
                return Convert.ToDouble(result);
            }
            else
            {
                // จัดการกรณีที่ไม่พบราคาสำหรับผลิตภัณฑ์ (ควรเป็นกรณีที่ไม่น่าจะเกิดขึ้นได้)
                return 0.0; // or throw an exception, depending on your error handling strategy
            }
        }

        private void UpdateTotalPrice()
        {
            double totalPrice = 0;
            MySqlConnection conn = databaseConnection();
            conn.Open();

            MySqlCommand cmd = new MySqlCommand("SELECT SUM(Price) FROM salesproduct WHERE Name = @CustomerName", conn);
            cmd.Parameters.AddWithValue("@CustomerName", loggedInCustomer);
            object total = cmd.ExecuteScalar();
            if (total != DBNull.Value)
            {
                totalPrice = Convert.ToDouble(total);
            }

            conn.Close();

            // Display the total price in TextBox3
            textBox3.Text = totalPrice.ToString("F2");
        }





        private void datagridview_cellclick1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                // เลือกแถวที่คลิก
                dataGridView1.Rows[e.RowIndex].Selected = true;

                // ตั้งค่าค่าของ textBox2 เป็นคอลัมน์ "Product" ของแถวที่คลิก
                if (dataGridView1.Rows[e.RowIndex].Cells["Product"].Value != null)
                {
                    textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["Product"].Value.ToString();
                }

                // ดึงชื่อไฟล์รูปภาพจากฐานข้อมูล
                string productName = dataGridView1.Rows[e.RowIndex].Cells["Product"].Value.ToString();
                string query = "SELECT Picproduct FROM manageproducts WHERE Product = @ProductName";

                MySqlConnection conn = databaseConnection();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductName", productName);

                try
                {
                    conn.Open();
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // อ่านข้อมูลรูปภาพ
                        byte[] imgData = (byte[])reader["Picproduct"];

                        // แสดงรูปภาพใน picture1
                        MemoryStream ms = new MemoryStream(imgData);
                        pictureBox1.Image = Image.FromStream(ms);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // ดึงข้อความจาก label1 และ textBox3
            string label1Text = label1.Text.Trim();
            string textFromForm5 = textBox3.Text;

            // คำนวณราคาสินค้าและ VAT
            decimal totalPrice = 0;
            decimal totalVat = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows) // วนลูปผ่านทุกแถวใน DataGridView
            {
                // ตรวจสอบว่าเซลล์ที่ต้องการใช้ในการคำนวณมีค่าหรือไม่
                if (row.Cells["Product"].Value != null &&
                    row.Cells["Quantity"].Value != null &&
                    row.Cells["Price"].Value != null)
                {
                    string productName = row.Cells["Product"].Value.ToString(); // ดึงชื่อของผลิตภัณฑ์
                    decimal itemPrice = Convert.ToDecimal(row.Cells["Price"].Value); // ดึงราคาของผลิตภัณฑ์และแปลงเป็น decimal
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);// ดึงจำนวนของผลิตภัณฑ์และแปลงเป็น int

                    decimal itemTotalPrice = itemPrice;
                    totalPrice += itemTotalPrice;  // เพิ่มราคารวมของรายการเข้าไปใน totalPrice

                    decimal itemVat = itemTotalPrice * 0.07m; // คำนวณ VAT ที่ 7%
                    totalVat += itemVat;
                }
            }

            decimal totalWithVat = totalPrice + totalVat;
            // คำนวณราคารวมหลังจากรวม VAT โดยการบวก totalPrice (ราคารวมทั้งหมด) กับ totalVat (VAT รวมทั้งหมด)


            // ถ้าราคาสินค้ารวมมากกว่า 3000 บาท ให้ส่วนลด 5%
            if (totalPrice >= 3000)
            {
                decimal discountAmount = totalWithVat * 0.05m;
                totalWithVat -= discountAmount;
                // เพิ่มส่วนลดในเอกสารถ้าจำเป็น
            }

            // สร้างและแสดง Form5 พร้อมกับส่งข้อมูลที่คำนวณแล้ว
            Form5 form5 = new Form5(loggedInCustomer, textFromForm5, totalWithVat.ToString("F2"));
            form5.Show();

            // ซ่อนฟอร์มปัจจุบัน
            this.Hide();

            // อัปเดตราคาทั้งหมด
            UpdateTotalPrice();

            HashSet<string> customerNames = new HashSet<string>();

            // รวบรวมชื่อจาก DataGridView2
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Name"].Value != null)
                {
                    customerNames.Add(row.Cells["Name"].Value.ToString());
                }
            }

            // นำชื่อแรกมาใช้
            string usercus = customerNames.FirstOrDefault() ?? "";

            // ดึงชื่อจากฐานข้อมูล customersmamakr ตาม username
            string customerName = "";
            string username = textBox1.Text.Trim(); // ดึง username จาก textBox1

            using (MySqlConnection conn = databaseConnection())
            {
                conn.Open();

                // ดึงชื่อของลูกค้าตาม username
                string getNameQuery = "SELECT Name FROM customersmamakr WHERE username = @username";
                using (MySqlCommand getNameCommand = new MySqlCommand(getNameQuery, conn))
                {
                    getNameCommand.Parameters.AddWithValue("@username", username);
                    using (MySqlDataReader reader = getNameCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customerName = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "";
                        }
                    }
                }
            }

            // ดึงชื่อของลูกค้าที่ล่าสุดจาก salesproduct
            using (MySqlConnection conn = databaseConnection())
            {
                conn.Open();
                string query = "SELECT Name FROM salesproduct ORDER BY id DESC LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usercus = reader.GetString("Name");
                        }
                    }
                }
            }

            // รวบรวมผลิตภัณฑ์และปริมาณจาก DataGridView2
            string iproduct = "";
            string iquantity = "";

            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Product"].Value != null && row.Cells["Quantity"].Value != null)
                {
                    iproduct += row.Cells["Product"].Value.ToString() + ",";
                    iquantity += row.Cells["Quantity"].Value.ToString() + ",";
                }
            }

            // ลบคอมม่าสุดท้าย
            iproduct = iproduct.TrimEnd(',');
            iquantity = iquantity.TrimEnd(',');

            // บันทึกข้อมูลลงใน selectsales
            using (MySqlConnection conn = databaseConnection())
            {
                conn.Open();
                string insertQuery = "INSERT INTO selectsales (Customername, Productname, Amount, Namelast) VALUES (@username, @iproduct, @iquantity, @Namelast)";

                using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@username", usercus);
                    cmd.Parameters.AddWithValue("@iproduct", iproduct);
                    cmd.Parameters.AddWithValue("@iquantity", iquantity);
                    cmd.Parameters.AddWithValue("@Namelast", label1Text); // ใช้ข้อมูลจาก label1
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void datagridview_cellclick2(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewCell cell = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (cell != null && cell.Value != null && !cell.Value.Equals(""))
                {
                    string productName = dataGridView2.Rows[e.RowIndex].Cells["Product"].FormattedValue.ToString();
                    string quantity = dataGridView2.Rows[e.RowIndex].Cells["Quantity"].FormattedValue.ToString();

                    textBox1.Text = productName;
                    textBox2.Text = quantity;

                    // เรียกเมทอด ShowReceiptFromDatabase() ซึ่งจะแสดงข้อมูลจากตาราง salesproduct โดยใช้ชื่อลูกค้าที่ล็อกอินอยู่
                    ShowReceiptFromDatabase();
                }
            }


        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text) && decimal.TryParse(textBox3.Text, out decimal value))
            {
                textBox3.Text = value.ToString("#,##0"); // แสดงเลขในรูปแบบหลักพัน
                textBox3.SelectionStart = textBox3.Text.Length; // ตั้งตำแหน่งตัวเลือกที่สิ้นสุดของข้อความ
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
