using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormDietPlans : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString; private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormDietPlans()
        {
            InitializeComponent();
            this.Text = "Плани раціонів";
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
                Name = "dgvDietPlans",
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
                ("Додати", BtnAdd_Click),
                ("Видалити", BtnDelete_Click),
                ("Переглянути склад", BtnViewDetails_Click)
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
                DataGridView dgv = this.Controls.Find("dgvDietPlans", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT dp.DietPlanID, s.SpeciesName, dp.PlanName, dp.SeasonType, 
                           dp.AgeGroup, dp.Description, dp.DailyCalories, dp.FeedingsPerDay, 
                           dp.IsActive, dp.CreatedDate
                    FROM DietPlans dp
                    LEFT JOIN Species s ON dp.SpeciesID = s.SpeciesID
                    ORDER BY s.SpeciesName, dp.SeasonType";

                dataAdapter = new SqlDataAdapter(query, connection);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["DietPlanID"].HeaderText = "ID";
                    dgv.Columns["SpeciesName"].HeaderText = "Вид тварини";
                    dgv.Columns["PlanName"].HeaderText = "Назва раціону";
                    dgv.Columns["SeasonType"].HeaderText = "Сезон";
                    dgv.Columns["AgeGroup"].HeaderText = "Вікова група";
                    dgv.Columns["Description"].HeaderText = "Опис";
                    dgv.Columns["DailyCalories"].HeaderText = "Калорій/день";
                    dgv.Columns["FeedingsPerDay"].HeaderText = "Годувань/день";
                    dgv.Columns["IsActive"].HeaderText = "Активний";
                    dgv.Columns["CreatedDate"].HeaderText = "Дата створення";

                    dgv.Columns["DietPlanID"].ReadOnly = true;
                    dgv.Columns["SpeciesName"].ReadOnly = true;
                    dgv.Columns["CreatedDate"].ReadOnly = true;
                    dgv.Columns["DietPlanID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["DietPlanID"].Width = 50;
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
            FormAddDietPlan addForm = new FormAddDietPlan();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvDietPlans", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "Ви впевнені, що хочете видалити вибрані раціони?\nУвага: будуть видалені всі деталі раціону!",
                        "Підтвердження",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            foreach (DataGridViewRow row in dgv.SelectedRows)
                            {
                                if (!row.IsNewRow && row.Cells["DietPlanID"].Value != null)
                                {
                                    int dietPlanId = Convert.ToInt32(row.Cells["DietPlanID"].Value);
                                    string deleteQuery = "DELETE FROM DietPlans WHERE DietPlanID = @DietPlanID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@DietPlanID", dietPlanId);
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

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            DataGridView dgv = this.Controls.Find("dgvDietPlans", false)[0] as DataGridView;

            if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Cells["DietPlanID"].Value != null)
            {
                int dietPlanId = Convert.ToInt32(dgv.SelectedRows[0].Cells["DietPlanID"].Value);
                FormDietPlanDetails detailsForm = new FormDietPlanDetails(dietPlanId);
                detailsForm.Show();
            }
            else
            {
                MessageBox.Show("Виберіть раціон для перегляду!", "Увага",
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

    // Форма додавання раціону
    public class FormAddDietPlan : Form
    {
        private ComboBox cmbSpecies, cmbSeason, cmbAgeGroup;
        private TextBox txtPlanName, txtDescription, txtCalories, txtFeedingsPerDay;
        private CheckBox chkIsActive;
        private string connectionString = @"Data Source=localhost;Initial Catalog=ZooMenuDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public FormAddDietPlan()
        {
            this.Text = "Додати план раціону";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeControls();
            LoadSpecies();
        }

        private void InitializeControls()
        {
            int y = 20;

            // Вид тварини
            this.Controls.Add(new Label { Text = "Вид тварини:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbSpecies = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbSpecies);
            y += 35;

            // Назва раціону
            this.Controls.Add(new Label { Text = "Назва раціону:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtPlanName = new TextBox { Location = new Point(180, y), Size = new Size(280, 25) };
            this.Controls.Add(txtPlanName);
            y += 35;

            // Сезон
            this.Controls.Add(new Label { Text = "Сезон:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbSeason = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSeason.Items.AddRange(new[] { "Зима", "Весна", "Літо", "Осінь", "Цілорічний" });
            cmbSeason.SelectedIndex = 4;
            this.Controls.Add(cmbSeason);
            y += 35;

            // Вікова група
            this.Controls.Add(new Label { Text = "Вікова група:", Location = new Point(20, y), Size = new Size(150, 20) });
            cmbAgeGroup = new ComboBox { Location = new Point(180, y), Size = new Size(280, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAgeGroup.Items.AddRange(new[] { "Дитинчата", "Молодняк", "Дорослі", "Літні" });
            cmbAgeGroup.SelectedIndex = 2;
            this.Controls.Add(cmbAgeGroup);
            y += 35;

            // Опис
            this.Controls.Add(new Label { Text = "Опис:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtDescription = new TextBox { Location = new Point(180, y), Size = new Size(280, 60), Multiline = true };
            this.Controls.Add(txtDescription);
            y += 70;

            // Калорії
            this.Controls.Add(new Label { Text = "Калорій на день:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtCalories = new TextBox { Location = new Point(180, y), Size = new Size(280, 25), Text = "2000" };
            this.Controls.Add(txtCalories);
            y += 35;

            // Годувань на день
            this.Controls.Add(new Label { Text = "Годувань на день:", Location = new Point(20, y), Size = new Size(150, 20) });
            txtFeedingsPerDay = new TextBox { Location = new Point(180, y), Size = new Size(280, 25), Text = "2" };
            this.Controls.Add(txtFeedingsPerDay);
            y += 35;

            // Активний
            chkIsActive = new CheckBox { Text = "Активний раціон", Location = new Point(180, y), Checked = true };
            this.Controls.Add(chkIsActive);
            y += 40;

            // Кнопки
            Button btnSave = new Button { Text = "Зберегти", Location = new Point(180, y), Size = new Size(120, 35) };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(310, y), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void LoadSpecies()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT SpeciesID, SpeciesName FROM Species ORDER BY SpeciesName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbSpecies.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                    if (cmbSpecies.Items.Count > 0) cmbSpecies.SelectedIndex = 0;
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
                if (string.IsNullOrWhiteSpace(txtPlanName.Text))
                {
                    MessageBox.Show("Введіть назву раціону!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO DietPlans 
                        (SpeciesID, PlanName, SeasonType, AgeGroup, Description, DailyCalories, FeedingsPerDay, IsActive, CreatedDate)
                        VALUES 
                        (@SpeciesID, @PlanName, @SeasonType, @AgeGroup, @Description, @DailyCalories, @FeedingsPerDay, @IsActive, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SpeciesID", ((ComboItem)cmbSpecies.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@PlanName", txtPlanName.Text);
                        cmd.Parameters.AddWithValue("@SeasonType", cmbSeason.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@AgeGroup", cmbAgeGroup.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(txtDescription.Text) ? (object)DBNull.Value : txtDescription.Text);
                        cmd.Parameters.AddWithValue("@DailyCalories",
                            string.IsNullOrWhiteSpace(txtCalories.Text) ? (object)DBNull.Value : int.Parse(txtCalories.Text));
                        cmd.Parameters.AddWithValue("@FeedingsPerDay", int.Parse(txtFeedingsPerDay.Text));
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Раціон успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
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