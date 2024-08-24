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
    public partial class Form13 : Form
    {
        private MySqlConnection databaseConnection()
        {
            String connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;charset=utf8;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        public Form13()
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
            cmd.CommandText = "SELECT * FROM historycustomer";

            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            adapter.Fill(ds);

            conn.Clone();

            dataGridView1.DataSource = ds.Tables[0].DefaultView;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Close();
        }

        private void Form13_Load(object sender, EventArgs e)
        {
            showhistoryCus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Prompt user for confirmation before deleting
                DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ที่จะลบข้อมูลนี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Get the ID of the selected row
                    string selectedID = dataGridView1.SelectedRows[0].Cells["ID"].Value.ToString();

                    // Create connection
                    MySqlConnection conn = databaseConnection();
                    conn.Open();

                    // Create command for deletion
                    MySqlCommand deleteCmd = conn.CreateCommand();
                    deleteCmd.CommandText = "DELETE FROM historycustomer WHERE ID = @ID";
                    deleteCmd.Parameters.AddWithValue("@ID", selectedID);

                    // Execute the DELETE query
                    int rowsAffected = deleteCmd.ExecuteNonQuery();

                    // Close connection
                    conn.Close();

                    // Display message based on the result
                    if (rowsAffected > 0)
                    {

                        // Get the quantity of the deleted product
                        int deletedQuantity = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Quantity"].Value);

                        // Get the product name of the deleted product
                        string productName = dataGridView1.SelectedRows[0].Cells["Product"].Value.ToString();

                        // Update the quantity of the product in manageproducts table
                        MySqlConnection conn2 = databaseConnection();
                        conn2.Open();

                        MySqlCommand updateCmd = conn2.CreateCommand();
                        updateCmd.CommandText = "UPDATE manageproducts SET Quantity = Quantity + @Quantity WHERE Product = @Product";
                        updateCmd.Parameters.AddWithValue("@Quantity", deletedQuantity);
                        updateCmd.Parameters.AddWithValue("@Product", productName);

                        int updateRowsAffected = updateCmd.ExecuteNonQuery();
                        if (updateRowsAffected > 0)
                        {

                        }
                        else
                        {
                            MessageBox.Show("ไม่สามารถเพิ่มจำนวนสินค้าได้");
                        }

                        // Refresh the DataGridView
                        showhistoryCus();
                    }
                    else
                    {
                        MessageBox.Show("ไม่สามารถลบข้อมูลได้");
                    }
                }
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim(); // รับชื่อผู้ใช้จาก TextBox3

            if (!string.IsNullOrEmpty(username))
            {
                MySqlConnection conn = databaseConnection();
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM historycustomer WHERE Name = @Username"; // เลือกการสั่งสินค้าของผู้ใช้ที่ระบุ
                cmd.Parameters.AddWithValue("@Username", username);

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridView1.DataSource = dt; // แสดงข้อมูลในตาราง

                conn.Close();
            }
            else
            {
               
            }
        

    }

    private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // เมื่อคลิกที่แถวใน DataGridView
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // ดึงข้อมูลที่เกี่ยวข้องจากแถวที่คลิก
                string username = row.Cells["Name"].Value.ToString();
                string product = row.Cells["Product"].Value.ToString();
                string quantity = row.Cells["Quantity"].Value.ToString();
                string price = row.Cells["Price"].Value.ToString();// ตั้งชื่อ column ตามที่ต้องการ

                // แสดงข้อมูล username ที่คลิกใน TextBox (หรือตามที่ต้องการแสดง)
                textBox1.Text = username;
                textBox2.Text = product;
                textBox3.Text = quantity;
                textBox4.Text = price;
            }
        }
    }
}
