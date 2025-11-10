using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormDietPlanDetails : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;
        private int dietPlanId;

        public FormDietPlanDetails(int dietPlanId = 0)
        {
            InitializeComponent();
            this.dietPlanId = dietPlanId;
            this.Text = "Склад раціону";
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
            // Панель вибору раціону
            Panel panelTop = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1260, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblDietPlan = new Label
            {
                Text = "Раціон:",
                Location = new Point(10, 18),
                Size = new Size(80, 20)
            };
            panelTop.Controls.Add(lblDietPlan);

            ComboBox cmbDietPlans = new ComboBox
            {
                Name = "cmbDietPlans",
                Location = new Point(100, 15),
                Size = new Size(400, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDietPlans.SelectedIndexChanged += (s, e) =>
            {
                if (cmbDietPlans.SelectedItem != null)
                {
                    this.dietPlanId = ((ComboItem)cmbDietPlans.SelectedItem).Value;
                    LoadData();
                }
            };
            panelTop.Controls.Add(cmbDietPlans);

            Button btnRefreshPlans = new Button
            {
                Text = "🔄",
                Location = new Point(510, 15),
                Size = new Size(40, 25)
            };
            btnRefreshPlans.Click += (s, e) => LoadDietPlans();
            panelTop.Controls.Add(btnRefreshPlans);

            this.Controls.Add(panelTop);
            LoadDietPlans();

            // DataGridView для деталей
            DataGridView dgv = new DataGridView
            {
                Name = "dgvDetails",
                Location = new Point(10, 120),
                Size = new Size(1260, 480),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            this.Controls.Add(dgv);

            // Кнопки управління
            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Додати корм", BtnAdd_Click),
                ("Видалити", BtnDelete_Click),
                ("Розрахувати калорії", BtnCalculateCalories_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 150, 80),
                    Size = new Size(140, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            // Статус панель
            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 610),
                Size = new Size(1260, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);

            // Інформаційна панель
            Label lblInfo = new Label
            {
                Name = "lblInfo",
                Text = "Виберіть раціон",
                Location = new Point(620, 80),
                Size = new Size(640, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };
            this.Controls.Add(lblInfo);
        }

        private void LoadDietPlans()
        {
            try
            {
                ComboBox cmbDietPlans = this.Controls.Find("cmbDietPlans", true)[0] as ComboBox;
                cmbDietPlans.Items.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT dp.DietPlanID, s.SpeciesName + ' - ' + dp.PlanName + ' (' + dp.SeasonType + ')' AS DisplayName
                        FROM DietPlans dp
                        LEFT JOIN Species s ON dp.SpeciesID = s.SpeciesID
                        WHERE dp.IsActive = 1
                        ORDER BY s.SpeciesName, dp.PlanName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbDietPlans.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                if (dietPlanId > 0)
                {
                    for (int i = 0; i < cmbDietPlans.Items.Count; i++)
                    {
                        if (((ComboItem)cmbDietPlans.Items[i]).Value == dietPlanId)
                        {
                            cmbDietPlans.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else if (cmbDietPlans.Items.Count > 0)
                {
                    cmbDietPlans.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження раціонів: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            if (dietPlanId == 0)
            {
                UpdateStatus("Виберіть раціон");
                return;
            }

            try
            {
                DataGridView dgv = this.Controls.Find("dgvDetails", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT dpd.DetailID, f.FeedName, f.FeedCategory, dpd.QuantityGrams, 
                           dpd.FeedingTime, dpd.FeedingOrder, dpd.Notes,
                           f.CaloriesPer100g, 
                           CAST((dpd.QuantityGrams / 100.0 * f.CaloriesPer100g) AS INT) AS TotalCalories
                    FROM DietPlanDetails dpd
                    LEFT JOIN FeedTypes f ON dpd.FeedID = f.FeedID
                    WHERE dpd.DietPlanID = @DietPlanID
                    ORDER BY dpd.FeedingOrder, dpd.FeedingTime";

                dataAdapter = new SqlDataAdapter(query, connection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@DietPlanID", dietPlanId);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["DetailID"].HeaderText = "ID";
                    dgv.Columns["FeedName"].HeaderText = "Назва корму";
                    dgv.Columns["FeedCategory"].HeaderText = "Категорія";
                    dgv.Columns["QuantityGrams"].HeaderText = "Кількість (г)";
                    dgv.Columns["FeedingTime"].HeaderText = "Час годування";
                    dgv.Columns["FeedingOrder"].HeaderText = "Порядок";
                    dgv.Columns["Notes"].HeaderText = "Примітки";
                    dgv.Columns["CaloriesPer100g"].HeaderText = "Калорії/100г";
                    dgv.Columns["TotalCalories"].HeaderText = "Всього калорій";

                    dgv.Columns["DetailID"].ReadOnly = true;
                    dgv.Columns["FeedName"].ReadOnly = true;
                    dgv.Columns["FeedCategory"].ReadOnly = true;
                    dgv.Columns["CaloriesPer100g"].ReadOnly = true;
                    dgv.Columns["TotalCalories"].ReadOnly = true;

                    dgv.Columns["DetailID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["DetailID"].Width = 50;
                    dgv.Columns["FeedingOrder"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["FeedingOrder"].Width = 80;
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
                Label lblInfo = this.Controls.Find("lblInfo", false)[0] as Label;

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    decimal totalCalories = 0;
                    decimal totalWeight = 0;

                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["TotalCalories"] != DBNull.Value)
                            totalCalories += Convert.ToDecimal(row["TotalCalories"]);
                        if (row["QuantityGrams"] != DBNull.Value)
                            totalWeight += Convert.ToDecimal(row["QuantityGrams"]);
                    }

                    lblInfo.Text = $"Загальна калорійність: {totalCalories} ккал | Загальна вага: {totalWeight} г ({totalWeight / 1000:F2} кг)";
                }
            }
            catch { }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (dietPlanId == 0)
            {
                MessageBox.Show("Спочатку виберіть раціон!", "Увага",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FormAddDietDetail addForm = new FormAddDietDetail(dietPlanId);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvDetails", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані записи?",
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
                                if (!row.IsNewRow && row.Cells["DetailID"].Value != null)
                                {
                                    int detailId = Convert.ToInt32(row.Cells["DetailID"].Value);
                                    string deleteQuery = "DELETE FROM DietPlanDetails WHERE DetailID = @DetailID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@DetailID", detailId);
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

        private void BtnCalculateCalories_Click(object sender, EventArgs e)
        {
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                decimal totalCalories = 0;
                decimal totalProtein = 0;
                decimal totalFat = 0;
                decimal totalCarbs = 0;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            SUM(dpd.QuantityGrams / 100.0 * f.CaloriesPer100g) AS TotalCalories,
                            SUM(dpd.QuantityGrams / 100.0 * f.ProteinPercent) AS TotalProtein,
                            SUM(dpd.QuantityGrams / 100.0 * f.FatPercent) AS TotalFat,
                            SUM(dpd.QuantityGrams / 100.0 * f.CarbohydratesPercent) AS TotalCarbs
                        FROM DietPlanDetails dpd
                        LEFT JOIN FeedTypes f ON dpd.FeedID = f.FeedID
                        WHERE dpd.DietPlanID = @DietPlanID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DietPlanID", dietPlanId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                totalCalories = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader.GetValue(0));
                                totalProtein = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1));
                                totalFat = reader.IsDBNull(2) ? 0 : Convert.ToDecimal(reader.GetValue(2));
                                totalCarbs = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3));
                            }
                        }
                    }
                }

                string message = $"Поживна цінність раціону:\n\n" +
                               $"Калорії: {totalCalories:F0} ккал\n" +
                               $"Білки: {totalProtein:F1} г\n" +
                               $"Жири: {totalFat:F1} г\n" +
                               $"Вуглеводи: {totalCarbs:F1} г";

                MessageBox.Show(message, "Розрахунок поживної цінності",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    // Форма додавання корму до раціону
    public class FormAddDietDetail : Form
    {
        private ComboBox cmbFeed;
        private TextBox txtQuantity, txtNotes;
        private DateTimePicker dtpFeedingTime;
        private NumericUpDown nudOrder;
        private int dietPlanId;
        private string connectionString = @"Data Source=localhost;Initial Catalog=ZooMenuDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public FormAddDietDetail(int dietPlanId)
        {
            this.dietPlanId = dietPlanId;
            this.Text = "Додати корм до раціону";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeControls();
            LoadFeeds();
        }

        private void InitializeControls()
        {
            int y = 20;

            // Корм
            this.Controls.Add(new Label { Text = "Корм:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbFeed = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbFeed);
            y += 35;

            // Кількість
            this.Controls.Add(new Label { Text = "Кількість (грамів):", Location = new Point(20, y), Size = new Size(150, 20) });
            txtQuantity = new TextBox { Location = new Point(180, y), Size = new Size(280, 25), Text = "500" };
            this.Controls.Add(txtQuantity);
            y += 35;

            // Час годування
            this.Controls.Add(new Label { Text = "Час годування:", Location = new Point(20, y), Size = new Size(150, 20) });
            dtpFeedingTime = new DateTimePicker
            {
                Location = new Point(180, y),
                Size = new Size(280, 25),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Value = DateTime.Today.AddHours(9)
            };
            this.Controls.Add(dtpFeedingTime);
            y += 35;

            // Порядок годування
            this.Controls.Add(new Label { Text = "Порядок годування:", Location = new Point(20, y), Size = new Size(150, 20) });
            nudOrder = new NumericUpDown
            {
                Location = new Point(180, y),
                Size = new Size(280, 25),
                Minimum = 1,
                Maximum = 10,
                Value = 1
            };
            this.Controls.Add(nudOrder);
            y += 35;

            // Примітки
            this.Controls.Add(new Label { Text = "Примітки:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtNotes = new TextBox { Location = new Point(180, y), Size = new Size(280, 60), Multiline = true };
            this.Controls.Add(txtNotes);
            y += 70;

            // Кнопки
            Button btnSave = new Button { Text = "Зберегти", Location = new Point(180, y), Size = new Size(120, 35) };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(310, y), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void LoadFeeds()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT FeedID, FeedName + ' (' + FeedCategory + ')' AS DisplayName FROM FeedTypes ORDER BY FeedCategory, FeedName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження кормів: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbFeed.SelectedItem == null)
                {
                    MessageBox.Show("Виберіть корм!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    MessageBox.Show("Введіть коректну кількість!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO DietPlanDetails 
                        (DietPlanID, FeedID, QuantityGrams, FeedingTime, FeedingOrder, Notes, CreatedDate)
                        VALUES 
                        (@DietPlanID, @FeedID, @QuantityGrams, @FeedingTime, @FeedingOrder, @Notes, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DietPlanID", dietPlanId);
                        cmd.Parameters.AddWithValue("@FeedID", ((ComboItem)cmbFeed.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@QuantityGrams", quantity);
                        cmd.Parameters.AddWithValue("@FeedingTime", dtpFeedingTime.Value.TimeOfDay);
                        cmd.Parameters.AddWithValue("@FeedingOrder", (int)nudOrder.Value);
                        cmd.Parameters.AddWithValue("@Notes",
                            string.IsNullOrWhiteSpace(txtNotes.Text) ? (object)DBNull.Value : txtNotes.Text);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Корм додано до раціону!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
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