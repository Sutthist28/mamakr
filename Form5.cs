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
using static mamakr.Form1;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Drawing.Printing;
using System.IO;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Net.Mail;
using System.Net;
using System.Reflection.Emit;
using System.Diagnostics;


namespace mamakr
{
    public partial class Form5 : Form
    {
        private string loggedInCustomer;
        private string textFromForm5;
        private string totalWithVat;

        public Form5(string loggedInCustomer, string textFromForm5, string totalWithVat)
        {
            InitializeComponent();
            this.loggedInCustomer = loggedInCustomer;
            this.textFromForm5 = textFromForm5;
            this.totalWithVat = totalWithVat; // กำหนดค่าจากพารามิเตอร์
        }

        public Form5()
        {
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            // กำหนดค่าให้กับ TextBox1 
            textBox1.Text = textFromForm5;
            ShowReceiptFromDatabase(); // แสดงข้อมูลในตาราง
            textBox2.Text = totalWithVat;

        }
        private void ShowReceiptFromDatabase()//โชว์ข้อมูลตารางดาต้ากริดวิว
        {
            string server = "127.0.0.1";
            string database = "ramenmamakr";
            string username = "root";
            string password = "";
            string connectionString = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // คิวรี่ เพื่อดึงข้อมูลใบเสร็จจากฐานข้อมูล
                    string selectReceiptQuery = "SELECT Name, Product, Quantity, Price FROM salesproduct WHERE Name = @CustomerName";
                    MySqlCommand receiptCommand = new MySqlCommand(selectReceiptQuery, connection);
                    receiptCommand.Parameters.AddWithValue("@CustomerName", loggedInCustomer);

                    DataTable dataTable = new DataTable();
                    dataTable.Load(receiptCommand.ExecuteReader());

                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pdfFilePath = CreatePDF();
            ShowReceiptFromDatabase(); // แสดงข้อมูลใน DataGridView ใหม่
            UpdateSelectSales(); //อัพราคารวมลงฐานข้อมูล
            SendEmailToCustomer();//ส่งอีเมลให้ลูกค้า
            if (!string.IsNullOrEmpty(pdfFilePath) && File.Exists(pdfFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pdfFilePath,
                    UseShellExecute = true // เปิดไฟล์ด้วยโปรแกรมที่เชื่อมโยงกับประเภทไฟล์นั้น
                });
            }

        }
        private string GetCustomerEmailFromDatabase()
        {
            string customerEmail = string.Empty;
            string server = "127.0.0.1";
            string database = "ramenmamakr";
            string username = "root";
            string password = "";
            string connectionString = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string selectCustomerEmailQuery = "SELECT Email FROM customersmamakr WHERE username = @CustomerName";
                    MySqlCommand emailCommand = new MySqlCommand(selectCustomerEmailQuery, connection);
                    emailCommand.Parameters.AddWithValue("@CustomerName", loggedInCustomer);

                    object result = emailCommand.ExecuteScalar();
                    if (result != null)
                    {
                        customerEmail = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching customer email: " + ex.Message);
            }

            return customerEmail;
        }

        private void SendEmailToCustomer()
        {
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "sutthida.th@kkumail.com";
            string smtpPassword = "ogmd onmz pqxh jarr";
            string senderEmail = "sutthida.th@kkumail.com";
            string emailSubject = "ใบเสร็จการสั่งซื้อสินค้า ร้าน KR ramyoen";
            string emailBody = "ขอบคุณที่ใช้บริการค่ะ Have a nice day";

            try
            {
                // การเชื่อมต่อ SMTP Server
                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                // ดึงอีเมลลูกค้าจากฐานข้อมูล
                string customerEmail = GetCustomerEmailFromDatabase();
                if (string.IsNullOrEmpty(customerEmail))
                {
                    MessageBox.Show("Customer email address is empty or null. Cannot send email.");
                    return;
                }

                // สร้างข้อความอีเมล
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail);
                mailMessage.To.Add(customerEmail);
                mailMessage.Subject = emailSubject;
                mailMessage.Body = emailBody;

                // ตรวจสอบการสร้างไฟล์ PDF
                string pdfFilePath = CreatePDF(); // แก้ไขฟังก์ชันเพื่อส่ง path ของไฟล์ PDF

                // ตรวจสอบว่ามีไฟล์ PDF อยู่หรือไม่
                if (File.Exists(pdfFilePath))
                {
                    Attachment attachment = new Attachment(pdfFilePath);
                    mailMessage.Attachments.Add(attachment);
                }
                else
                {
                    MessageBox.Show("PDF file not found: " + pdfFilePath);
                    return;
                }

                // ส่งอีเมล
                smtpClient.Send(mailMessage);

                //MessageBox.Show("อีเมลถูกส่งไปยังลูกค้าเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการส่งอีเมล: " + ex.Message);
            }
        }

        // ฟังก์ชันนี้ใช้สร้างไฟล์ PDF สำหรับใบเสร็จลูกค้า
        private string CreatePDF()
        {
            string pdfFilePath = string.Empty; // ตัวแปรสำหรับเก็บเส้นทางของไฟล์ PDF
            Document doc = new Document(PageSize.A4);
            try
            {   // ตรวจสอบว่าโฟลเดอร์นั้นมีอยู่แล้วหรือไม่ ถ้าไม่มีก็ให้สร้างใหม่
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Reciept KRramyoen store");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string customerName = loggedInCustomer.Replace(" ", "_");// ดึงชื่อของลูกค้าที่เข้าสู่ระบบมา และแทนที่ช่องว่างด้วยเครื่องหมายขีดล่าง (_)
                int customerNumber = GetCustomerNumberFromDatabase();// เรียกใช้ฟังก์ชันเพื่อดึงหมายเลขลูกค้าจากฐานข้อมูล
                int orderNumber = GetOrderNumber(); // ฟังก์ชันใหม่เพื่อดึงหมายเลขคำสั่งซื้อ
                string fileName = $"{customerName}_{customerNumber}_{orderNumber}.pdf";// สร้างชื่อไฟล์ PDF โดยรวมชื่อของลูกค้า หมายเลขลูกค้า และหมายเลขคำสั่งซื้อ
                pdfFilePath = Path.Combine(folderPath, fileName);// สร้างที่อยู่ไฟล์ PDF โดยการรวมที่อยู่ของโฟลเดอร์และชื่อไฟล์ที่สร้างขึ้น

                PdfWriter.GetInstance(doc, new FileStream(pdfFilePath, FileMode.Create));
                doc.Open();

                // เพิ่มข้อความ "Ramyoen KR Store" ลงในเอกสาร
                Paragraph storeLocation = new Paragraph("Ramyoen KR Store", FontFactory.GetFont(FontFactory.HELVETICA, 30f));
                storeLocation.Alignment = Element.ALIGN_CENTER;
                doc.Add(storeLocation);
                doc.Add(new Paragraph(" ")); // เพิ่มช่องว่าง
                doc.Add(new Paragraph("Location : KKU wora resident"));
                doc.Add(new Paragraph("Tel.0944522600"));
                doc.Add(new Paragraph($"Customer: {loggedInCustomer}")); // เพิ่มชื่อของลูกค้า

                // เพิ่มวันที่และเวลา
                DateTime now = DateTime.Now;
                doc.Add(new Paragraph($"Date: {now.ToString("dd/MM/yyyy HH:mm:ss")}"));
                doc.Add(new Paragraph(" ")); // เพิ่มช่องว่าง

                // สร้างตารางใน PDF ที่มีจำนวนคอลัมน์เท่ากับจำนวนคอลัมน์ใน DataGridView (ยกเว้นคอลัมน์ "Name")
                PdfPTable table = new PdfPTable(dataGridView1.Columns.Count - 1);
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.HeaderText != "Name")
                    {
                        table.AddCell(new Phrase(column.HeaderText)); // เพิ่มคอลัมน์ในตาราง
                    }
                }

                // เพิ่มแถวและเซลล์ในตารางจาก DataGridView
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {   // สร้างอ็อบเจ็กต์ PdfPTable ที่มีจำนวนคอลัมน์เท่ากับจำนวนคอลัมน์ใน DataGridView ลบคอลัมน์ "Name" ออก
                        if (cell.OwningColumn.HeaderText != "Name")
                        {
                            if (cell.Value != null && cell.Value != DBNull.Value)
                            {
                                table.AddCell(new Phrase(cell.Value.ToString())); // เพิ่มคอลัมน์ในตาราง PDF โดยใช้ชื่อหัวคอลัมน์จาก DataGridView ที่ไม่ใช่ "Name"
                            }
                            else
                            {
                                table.AddCell(new Phrase(string.Empty)); // เพิ่มเซลล์ว่างถ้าไม่มีค่า
                            }
                        }
                    }
                }

                doc.Add(table); // เพิ่มตารางในเอกสาร
                doc.Add(new Paragraph(" ")); // เพิ่มช่องว่าง

                // คำนวณราคาสินค้าและ VAT
                decimal totalPrice = 0;
                decimal totalVat = 0;
                foreach (DataGridViewRow row in dataGridView1.Rows) // วนลูปผ่านทุกแถวใน DataGridView
                {   // ตรวจสอบว่าเซลล์ที่ต้องการใช้ในการคำนวณมีค่าหรือไม่
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
                    doc.Add(new Paragraph($"Discount: {discountAmount:F2} Baht")); // เพิ่มส่วนลดในเอกสาร
                }

                // เพิ่มราคาสินค้า, VAT และราคารวมหลังหักส่วนลดในเอกสาร
                doc.Add(new Paragraph($"Price: {totalPrice:F2} Baht"));
                doc.Add(new Paragraph($"VAT 7%: {totalVat:F2} Baht"));
                doc.Add(new Paragraph($"Totalprice: {totalWithVat:F2} Baht"));


                doc.Close(); // ปิดไฟล์ PDF

                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating PDF: " + ex.Message);
            }

            return pdfFilePath; // ส่งคืน path ของไฟล์ PDF
        }

        // ฟังก์ชันใหม่เพื่อดึงหมายเลขคำสั่งซื้อ
        private int GetOrderNumber()
        {
            // สมมติว่าเรามีวิธีการดึงหมายเลขคำสั่งซื้อที่ไม่ซ้ำกัน เช่น เพิ่มหมายเลขคำสั่งซื้อที่เพิ่มขึ้นจากฐานข้อมูล
            // หรือใช้วิธีการอื่นที่เหมาะสมกับระบบของคุณ
            return new Random().Next(1000, 9999); // ใช้เลขสุ่มเป็นตัวอย่าง
        }

        // ฟังก์ชันนี้ใช้ดึงลำดับลูกค้าจากฐานข้อมูล
        private int GetCustomerNumberFromDatabase()
        {
            int customerNumber = 0;
            string server = "127.0.0.1";
            string database = "ramenmamakr";
            string username = "root";
            string password = "";
            string connectionString = $"SERVER={server};DATABASE={database};UID={username};PASSWORD={password};";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    // ดึงจำนวนลูกค้าทั้งหมดจากตาราง customersmamakr
                    string selectCustomerNumberQuery = "SELECT COUNT(*) FROM customersmamakr";
                    MySqlCommand numberCommand = new MySqlCommand(selectCustomerNumberQuery, connection);
                    customerNumber = Convert.ToInt32(numberCommand.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching customer number: " + ex.Message); // แสดงข้อความ error ถ้ามีข้อผิดพลาด
            }

            return customerNumber + 1; // คืนค่าลำดับลูกค้าที่เพิ่มขึ้น
        }
        private void UpdateSelectSales()
        {   // กำหนดที่อยู่ของโฟลเดอร์ที่เก็บไฟล์ PDF
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Reciept KRramyoen store");

            // ค้นหาไฟล์ PDF ที่แก้ไขล่าสุดในโฟลเดอร์
            string latestFilePath = GetLatestPdfFile(folderPath);
            // ตรวจสอบว่าไฟล์ PDF ล่าสุดมีอยู่หรือไม่
            if (string.IsNullOrEmpty(latestFilePath))
            {
                MessageBox.Show("No PDF files found in the specified folder.");
                return;
            }

            try
            {   // สร้างการเชื่อมต่อกับฐานข้อมูล
                using (MySqlConnection conn = databaseConnection())
                {
                    conn.Open();

                    // อัปเดต Receipt  
                    string updateReceiptQuery = "UPDATE selectsales SET Receipt = @Receipt WHERE ID = (SELECT MAX(ID) FROM selectsales)";
                    using (MySqlCommand receiptCmd = new MySqlCommand(updateReceiptQuery, conn))
                    {
                        receiptCmd.Parameters.AddWithValue("@Receipt", latestFilePath);
                        receiptCmd.ExecuteNonQuery();
                    }

                    // อัปเดต Totalprice
                    double totalPrice;
                    if (double.TryParse(textBox2.Text, out totalPrice))
                    {
                        string updateTotalpriceQuery = "UPDATE selectsales SET Totalprice = @Totalprice WHERE ID = (SELECT MAX(ID) FROM selectsales)";
                        using (MySqlCommand totalpriceCmd = new MySqlCommand(updateTotalpriceQuery, conn))
                        {
                            totalpriceCmd.Parameters.AddWithValue("@Totalprice", totalPrice.ToString("F2"));
                            totalpriceCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating selectsales database: " + ex.Message);
            }
        }

        private string GetLatestPdfFile(string folderPath)
        {   // สร้างอ็อบเจ็กต์ DirectoryInfo สำหรับโฟลเดอร์ที่กำหนด
            DirectoryInfo directory = new DirectoryInfo(folderPath);// ค้นหาไฟล์ PDF ทั้งหมดในโฟลเดอร์
            FileInfo latestFile = directory.GetFiles("*.pdf")
                                           .OrderByDescending(f => f.LastWriteTime)
                                           .FirstOrDefault();
            // คืนค่าที่อยู่ของไฟล์ PDF ล่าสุด ถ้าไม่พบไฟล์ PDF จะคืนค่าว่าง
            return latestFile != null ? latestFile.FullName : string.Empty;
        }

        private void DeleteAllSalesProducts()
        {
            try
            {   // เชื่อมต่อฐานข้อมูลและลบข้อมูลสินค้าที่ขายออกทั้งหมด
                MySqlConnection conn = databaseConnection();
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("DELETE FROM salesproduct", conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting sales products: " + ex.Message);
            }
        }
        private MySqlConnection databaseConnection()
        {
            // แก้ไขค่าการเชื่อมต่อฐานข้อมูล MySQL ให้ตรงกับการกำหนดค่าของคุณ
            String connectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=ramenmamakr;";
            MySqlConnection conn = new MySqlConnection(connectionString);
            return conn;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DeleteAllSalesProducts(); // ลบข้อมูลการขายทั้งหมด
            Application.Exit();
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
