using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormFeedStock : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormFeedStock()
        {
            InitializeComponent();
            this.Text = "Склад кормів";
            this.Size = new Size(1400, 750);
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
            // DataGridView
            DataGridView dgv = new DataGridView
            {
                Name = "dgvStock",
                Location = new Point(10, 50),
                Size = new Size(1360, 590),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgv);

            // Кнопки
            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Додати партію", BtnAdd_Click),
                ("Видалити", BtnDelete_Click),
                ("Прострочені корми", BtnExpired_Click),
                ("Низькі залишки", BtnLowStock_Click),
                ("Статистика", BtnStatistics_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 140, 10),
                    Size = new Size(130, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            // Статус панель
            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 650),
                Size = new Size(1360, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);

            // Інфо панель
            Panel panelInfo = new Panel
            {
                Name = "panelInfo",
                Location = new Point(10, 680),
                Size = new Size(1360, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };

            Label lblInfo = new Label
            {
                Name = "lblInfo",
                Text = "Завантаження...",
                Location = new Point(10, 10),
                Size = new Size(1340, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            panelInfo.Controls.Add(lblInfo);
            this.Controls.Add(panelInfo);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvStock", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT fs.StockID, f.FeedName, f.FeedCategory, fs.Quantity, 
                           f.MeasurementUnit, s.CompanyName AS SupplierName,
                           fs.DeliveryDate, fs.ExpiryDate, fs.BatchNumber, 
                           fs.TotalCost, fs.StorageLocation,
                           DATEDIFF(day, GETDATE(), fs.ExpiryDate) AS DaysToExpiry,
                           CASE 
                               WHEN DATEDIFF(day, GETDATE(), fs.ExpiryDate) < 0 THEN 'Прострочено'
                               WHEN DATEDIFF(day, GETDATE(), fs.ExpiryDate) <= 7 THEN 'Термінова увага!'
                               WHEN DATEDIFF(day, GETDATE(), fs.ExpiryDate) <= 30 THEN 'Скоро закінчується'
                               ELSE 'Нормально'
                           END AS Status
                    FROM FeedStock fs
                    LEFT JOIN FeedTypes f ON fs.FeedID = f.FeedID
                    LEFT JOIN Suppliers s ON fs.SupplierID = s.SupplierID
                    WHERE fs.Quantity > 0
                    ORDER BY fs.ExpiryDate, f.FeedName";

                dataAdapter = new SqlDataAdapter(query, connection);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["StockID"].HeaderText = "ID";
                    dgv.Columns["FeedName"].HeaderText = "Назва корму";
                    dgv.Columns["FeedCategory"].HeaderText = "Категорія";
                    dgv.Columns["Quantity"].HeaderText = "Кількість";
                    dgv.Columns["MeasurementUnit"].HeaderText = "Од. вим.";
                    dgv.Columns["SupplierName"].HeaderText = "Постачальник";
                    dgv.Columns["DeliveryDate"].HeaderText = "Дата поставки";
                    dgv.Columns["ExpiryDate"].HeaderText = "Термін придатності";
                    dgv.Columns["BatchNumber"].HeaderText = "№ партії";
                    dgv.Columns["TotalCost"].HeaderText = "Вартість";
                    dgv.Columns["StorageLocation"].HeaderText = "Місце зберігання";
                    dgv.Columns["DaysToExpiry"].HeaderText = "Днів до закінчення";
                    dgv.Columns["Status"].HeaderText = "Статус";

                    dgv.Columns["StockID"].ReadOnly = true;
                    dgv.Columns["FeedName"].ReadOnly = true;
                    dgv.Columns["FeedCategory"].ReadOnly = true;
                    dgv.Columns["MeasurementUnit"].ReadOnly = true;
                    dgv.Columns["SupplierName"].ReadOnly = true;
                    dgv.Columns["DaysToExpiry"].ReadOnly = true;
                    dgv.Columns["Status"].ReadOnly = true;
                    dgv.Columns["StockID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                    dgv.Columns["StockID"].Width = 50;
                    dgv.Columns["MeasurementUnit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["MeasurementUnit"].Width = 70;
                    dgv.Columns["DaysToExpiry"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["DaysToExpiry"].Width = 100;

                    // Кольорове виділення
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.Cells["Status"].Value != null)
                        {
                            string status = row.Cells["Status"].Value.ToString();
                            switch (status)
                            {
                                case "Прострочено":
                                    row.DefaultCellStyle.BackColor = Color.Red;
                                    row.DefaultCellStyle.ForeColor = Color.White;
                                    break;
                                case "Термінова увага!":
                                    row.DefaultCellStyle.BackColor = Color.Orange;
                                    break;
                                case "Скоро закінчується":
                                    row.DefaultCellStyle.BackColor = Color.Yellow;
                                    break;
                            }
                        }
                    }
                }

                UpdateInfoPanel();
                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateInfoPanel()
        {
            try
            {
                Label lblInfo = this.Controls.Find("lblInfo", true)[0] as Label;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    int expired = dataTable.Select("Status = 'Прострочено'").Length;
                    int urgent = dataTable.Select("Status = 'Термінова увага!'").Length;
                    int expiring = dataTable.Select("Status = 'Скоро закінчується'").Length;

                    decimal totalValue = 0;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["TotalCost"] != DBNull.Value)
                            totalValue += Convert.ToDecimal(row["TotalCost"]);
                    }

                    string warningText = "";
                    if (expired > 0)
                        warningText += $"⚠️ ПРОСТРОЧЕНО: {expired} партій! ";
                    if (urgent > 0)
                        warningText += $"⚠️ ТЕРМІНОВА УВАГА: {urgent} партій (менше 7 днів)! ";
                    if (expiring > 0)
                        warningText += $"ℹ️ Скоро закінчується: {expiring} партій ";

                    if (string.IsNullOrEmpty(warningText))
                        warningText = "✓ Всі корми в нормальному стані";

                    lblInfo.Text = $"{warningText} | Загальна вартість запасів: {totalValue:F2} грн";
                }
                else
                {
                    lblInfo.Text = "Склад порожній";
                }
            }
            catch { }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            FormAddFeedStock addForm = new FormAddFeedStock();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvStock", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані партії?",
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
                                if (!row.IsNewRow && row.Cells["StockID"].Value != null)
                                {
                                    int stockId = Convert.ToInt32(row.Cells["StockID"].Value);
                                    string deleteQuery = "DELETE FROM FeedStock WHERE StockID = @StockID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@StockID", stockId);
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

        private void BtnExpired_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataTable != null)
                {
                    DataView dv = dataTable.DefaultView;
                    dv.RowFilter = "Status = 'Прострочено' OR Status = 'Термінова увага!'";
                    UpdateStatus($"Показано критичних записів: {dv.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка фільтрації: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLowStock_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT f.FeedName, f.FeedCategory, 
                               ISNULL(SUM(fs.Quantity), 0) AS TotalStock,
                               f.MeasurementUnit
                        FROM FeedTypes f
                        LEFT JOIN FeedStock fs ON f.FeedID = fs.FeedID AND fs.Quantity > 0
                        GROUP BY f.FeedName, f.FeedCategory, f.MeasurementUnit
                        HAVING ISNULL(SUM(fs.Quantity), 0) < 50
                        ORDER BY TotalStock";

                    DataTable dtLowStock = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dtLowStock);
                    }

                    if (dtLowStock.Rows.Count > 0)
                    {
                        Form reportForm = new Form
                        {
                            Text = "Корми з низькими залишками",
                            Size = new Size(800, 600),
                            StartPosition = FormStartPosition.CenterParent
                        };

                        DataGridView dgvReport = new DataGridView
                        {
                            DataSource = dtLowStock,
                            Location = new Point(10, 10),
                            Size = new Size(760, 530),
                            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                            ReadOnly = true
                        };

                        if (dgvReport.Columns.Count > 0)
                        {
                            dgvReport.Columns["FeedName"].HeaderText = "Назва корму";
                            dgvReport.Columns["FeedCategory"].HeaderText = "Категорія";
                            dgvReport.Columns["TotalStock"].HeaderText = "Залишок";
                            dgvReport.Columns["MeasurementUnit"].HeaderText = "Од. вим.";
                        }

                        reportForm.Controls.Add(dgvReport);
                        reportForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Всі корми мають достатні залишки!", "Інформація",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка перевірки залишків: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnStatistics_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            COUNT(*) AS TotalBatches,
                            COUNT(DISTINCT fs.FeedID) AS UniqueFeedTypes,
                            SUM(fs.TotalCost) AS TotalValue,
                            SUM(CASE WHEN DATEDIFF(day, GETDATE(), fs.ExpiryDate) < 0 THEN 1 ELSE 0 END) AS ExpiredCount,
                            SUM(CASE WHEN DATEDIFF(day, GETDATE(), fs.ExpiryDate) <= 7 AND DATEDIFF(day, GETDATE(), fs.ExpiryDate) >= 0 THEN 1 ELSE 0 END) AS ExpiringCount
                        FROM FeedStock fs
                        WHERE fs.Quantity > 0";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalBatches = reader.GetInt32(0);
                                int uniqueFeeds = reader.GetInt32(1);
                                decimal totalValue = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                                int expiredCount = reader.GetInt32(3);
                                int expiringCount = reader.GetInt32(4);

                                string message = $"Статистика складу:\n\n" +
                                               $"Всього партій: {totalBatches}\n" +
                                               $"Унікальних видів кормів: {uniqueFeeds}\n" +
                                               $"Загальна вартість: {totalValue:F2} грн\n" +
                                               $"Прострочених партій: {expiredCount}\n" +
                                               $"Партій з терміном менше 7 днів: {expiringCount}";

                                MessageBox.Show(message, "Статистика складу",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка розрахунку статистики: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    // Форма додавання партії кормів
    public class FormAddFeedStock : Form
    {
        private ComboBox cmbFeed, cmbSupplier;
        private TextBox txtQuantity, txtBatchNumber, txtTotalCost, txtStorageLocation;
        private DateTimePicker dtpDelivery, dtpExpiry;
        private string connectionString = @"Data Source=localhost;Initial Catalog=ZooMenuDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public FormAddFeedStock()
        {
            this.Text = "Додати партію кормів";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeControls();
            LoadData();
        }

        private void InitializeControls()
        {
            int y = 20;

            // Корм
            this.Controls.Add(new Label { Text = "Корм:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbFeed = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbFeed);
            y += 35;

            // Постачальник
            this.Controls.Add(new Label { Text = "Постачальник:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbSupplier = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbSupplier);
            y += 35;

            // Кількість
            this.Controls.Add(new Label { Text = "Кількість:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtQuantity = new TextBox { Location = new Point(180, y), Size = new Size(280, 25), Text = "100" };
            this.Controls.Add(txtQuantity);
            y += 35;

            // Дата поставки
            this.Controls.Add(new Label { Text = "Дата поставки:", Location = new Point(20, y), Size = new Size(150, 20) });
            dtpDelivery = new DateTimePicker { Location = new Point(180, y), Size = new Size(280, 25), Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDelivery);
            y += 35;

            // Термін придатності
            this.Controls.Add(new Label { Text = "Термін придатності:", Location = new Point(20, y), Size = new Size(150, 20) });
            dtpExpiry = new DateTimePicker { Location = new Point(180, y), Size = new Size(280, 25), Format = DateTimePickerFormat.Short };
            dtpExpiry.Value = DateTime.Today.AddMonths(3);
            this.Controls.Add(dtpExpiry);
            y += 35;

            // Номер партії
            this.Controls.Add(new Label { Text = "Номер партії:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtBatchNumber = new TextBox { Location = new Point(180, y), Size = new Size(280, 25) };
            this.Controls.Add(txtBatchNumber);
            y += 35;

            // Вартість
            this.Controls.Add(new Label { Text = "Загальна вартість:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtTotalCost = new TextBox { Location = new Point(180, y), Size = new Size(280, 25), Text = "1000" };
            this.Controls.Add(txtTotalCost);
            y += 35;

            // Місце зберігання
            this.Controls.Add(new Label { Text = "Місце зберігання:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtStorageLocation = new TextBox { Location = new Point(180, y), Size = new Size(280, 25) };
            this.Controls.Add(txtStorageLocation);
            y += 40;

            // Кнопки
            Button btnSave = new Button { Text = "Зберегти", Location = new Point(180, y), Size = new Size(120, 35) };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(310, y), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Корми
                    string feedQuery = "SELECT FeedID, FeedName + ' (' + FeedCategory + ')' AS DisplayName FROM FeedTypes ORDER BY FeedCategory, FeedName";
                    using (SqlCommand cmd = new SqlCommand(feedQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbFeed.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                    if (cmbFeed.Items.Count > 0) cmbFeed.SelectedIndex = 0;

                    // Постачальники
                    string supplierQuery = "SELECT SupplierID, CompanyName FROM Suppliers WHERE IsActive = 1 ORDER BY CompanyName";
                    using (SqlCommand cmd = new SqlCommand(supplierQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbSupplier.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                    if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbFeed.SelectedItem == null || cmbSupplier.SelectedItem == null)
                {
                    MessageBox.Show("Виберіть корм та постачальника!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введіть коректну кількість!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtTotalCost.Text, out decimal cost) || cost < 0)
                {
                    MessageBox.Show("Введіть коректну вартість!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dtpExpiry.Value <= dtpDelivery.Value)
                {
                    MessageBox.Show("Термін придатності має бути пізніше дати поставки!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO FeedStock 
                        (FeedID, SupplierID, Quantity, DeliveryDate, ExpiryDate, BatchNumber, TotalCost, StorageLocation, CreatedDate)
                        VALUES 
                        (@FeedID, @SupplierID, @Quantity, @DeliveryDate, @ExpiryDate, @BatchNumber, @TotalCost, @StorageLocation, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FeedID", ((ComboItem)cmbFeed.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@SupplierID", ((ComboItem)cmbSupplier.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@DeliveryDate", dtpDelivery.Value);
                        cmd.Parameters.AddWithValue("@ExpiryDate", dtpExpiry.Value);
                        cmd.Parameters.AddWithValue("@BatchNumber",
                            string.IsNullOrWhiteSpace(txtBatchNumber.Text) ? (object)DBNull.Value : txtBatchNumber.Text);
                        cmd.Parameters.AddWithValue("@TotalCost", cost);
                        cmd.Parameters.AddWithValue("@StorageLocation",
                            string.IsNullOrWhiteSpace(txtStorageLocation.Text) ? (object)DBNull.Value : txtStorageLocation.Text);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Партію кормів додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}