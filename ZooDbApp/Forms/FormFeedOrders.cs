using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormFeedOrders : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormFeedOrders()
        {
            InitializeComponent();
            this.Text = "Замовлення кормів";
            this.Size = new Size(1300, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeControls();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ResumeLayout(false);
        }

        private void InitializeControls()
        {
            DataGridView dgv = new DataGridView
            {
                Name = "dgvOrders",
                Location = new Point(10, 50),
                Size = new Size(1260, 550),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgv);
            var buttons = new[]
        {
            ("Оновити", (EventHandler)((s, e) => LoadData())),
            ("Нове замовлення", BtnAdd_Click),
            ("Видалити", BtnDelete_Click),
            ("Деталі замовлення", BtnDetails_Click)
        };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 150, 10),
                    Size = new Size(140, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 610),
                Size = new Size(1260, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvOrders", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                SELECT fo.OrderID, s.CompanyName AS SupplierName, emp.FullName AS EmployeeName,
                       fo.OrderDate, fo.ExpectedDeliveryDate, fo.ActualDeliveryDate,
                       fo.TotalAmount, fo.OrderStatus, fo.PaymentStatus, fo.Notes
                FROM FeedOrders fo
                LEFT JOIN Suppliers s ON fo.SupplierID = s.SupplierID
                LEFT JOIN Employees emp ON fo.EmployeeID = emp.EmployeeID
                ORDER BY fo.OrderDate DESC";

                dataAdapter = new SqlDataAdapter(query, connection);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["OrderID"].HeaderText = "ID";
                    dgv.Columns["SupplierName"].HeaderText = "Постачальник";
                    dgv.Columns["EmployeeName"].HeaderText = "Відповідальний";
                    dgv.Columns["OrderDate"].HeaderText = "Дата замовлення";
                    dgv.Columns["ExpectedDeliveryDate"].HeaderText = "Очікувана доставка";
                    dgv.Columns["ActualDeliveryDate"].HeaderText = "Фактична доставка";
                    dgv.Columns["TotalAmount"].HeaderText = "Сума";
                    dgv.Columns["OrderStatus"].HeaderText = "Статус замовлення";
                    dgv.Columns["PaymentStatus"].HeaderText = "Статус оплати";
                    dgv.Columns["Notes"].HeaderText = "Примітки";

                    dgv.Columns["OrderID"].ReadOnly = true;
                    dgv.Columns["SupplierName"].ReadOnly = true;
                    dgv.Columns["EmployeeName"].ReadOnly = true;
                    dgv.Columns["OrderID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["OrderID"].Width = 50;

                    // Кольорове виділення
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.Cells["OrderStatus"].Value != null)
                        {
                            string status = row.Cells["OrderStatus"].Value.ToString();
                            if (status == "Доставлено")
                                row.DefaultCellStyle.BackColor = Color.LightGreen;
                            else if (status == "Скасовано")
                                row.DefaultCellStyle.BackColor = Color.LightGray;
                        }
                    }
                }

                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Функція створення замовлення буде реалізована через окрему форму", "Інформація",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvOrders", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані замовлення?",
                        "Підтвердження",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            foreach (DataGridViewRow row in dgv.SelectedRows)
                            {
                                if (!row.IsNewRow && row.Cells["OrderID"].Value != null)
                                {
                                    int orderId = Convert.ToInt32(row.Cells["OrderID"].Value);
                                    string deleteQuery = "DELETE FROM FeedOrders WHERE OrderID = @OrderID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        UpdateStatus("Записи видалено!");
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Виберіть рядки для видалення!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            DataGridView dgv = this.Controls.Find("dgvOrders", false)[0] as DataGridView;

            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Cells["OrderID"].Value != null)
            {
                MessageBox.Show("Функція перегляду деталей замовлення буде реалізована", "Інформація",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Виберіть замовлення для перегляду!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateStatus(string message)
        {
            Label lblStatus = this.Controls.Find("lblStatus", false)[0] as Label;
            if (lblStatus != null)
            {
                lblStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            }
        }
    }
}