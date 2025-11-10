using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ZooMenuApp.Forms
{
    public partial class FormAnimals : Form
    {
        private string connectionString =
                    ConfigurationManager.ConnectionStrings["ZooMenuDB"].ConnectionString;
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;

        public FormAnimals()
        {
            InitializeComponent();
            this.Text = "Тварини";
            this.Size = new Size(1400, 700);
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
                Name = "dgvAnimals",
                Location = new Point(10, 50),
                Size = new Size(1360, 550),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                MultiSelect = false
            };
            this.Controls.Add(dgv);

            var buttons = new[]
            {
                ("Оновити", (EventHandler)((s, e) => LoadData())),
                ("Додати", BtnAdd_Click),
                ("Редагувати", BtnEdit_Click),
                ("Видалити", BtnDelete_Click),
                ("Пошук", BtnSearch_Click)
            };

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = new Button
                {
                    Text = buttons[i].Item1,
                    Location = new Point(10 + i * 110, 10),
                    Size = new Size(100, 30)
                };
                btn.Click += buttons[i].Item2;
                this.Controls.Add(btn);
            }

            Label lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Готово",
                Location = new Point(10, 610),
                Size = new Size(1360, 20),
                AutoSize = false
            };
            this.Controls.Add(lblStatus);
        }

        private void LoadData()
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvAnimals", false)[0] as DataGridView;

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                string query = @"
                    SELECT a.AnimalID, a.AnimalName, s.SpeciesName, a.BirthDate, 
                           a.Gender, a.Weight, e.EnclosureName, a.HealthStatus, 
                           a.SpecialDiet, a.SpecialDietNotes, a.ArrivalDate, a.IsActive
                    FROM Animals a
                    LEFT JOIN Species s ON a.SpeciesID = s.SpeciesID
                    LEFT JOIN Enclosures e ON a.EnclosureID = e.EnclosureID
                    ORDER BY a.AnimalName";

                dataAdapter = new SqlDataAdapter(query, connection);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dgv.DataSource = dataTable;

                if (dgv.Columns.Count > 0)
                {
                    dgv.Columns["AnimalID"].HeaderText = "ID";
                    dgv.Columns["AnimalName"].HeaderText = "Ім'я";
                    dgv.Columns["SpeciesName"].HeaderText = "Вид";
                    dgv.Columns["BirthDate"].HeaderText = "Дата народження";
                    dgv.Columns["Gender"].HeaderText = "Стать";
                    dgv.Columns["Weight"].HeaderText = "Вага (кг)";
                    dgv.Columns["EnclosureName"].HeaderText = "Вольєр";
                    dgv.Columns["HealthStatus"].HeaderText = "Стан здоров'я";
                    dgv.Columns["SpecialDiet"].HeaderText = "Спец. дієта";
                    dgv.Columns["SpecialDietNotes"].HeaderText = "Примітки";
                    dgv.Columns["ArrivalDate"].HeaderText = "Дата прибуття";
                    dgv.Columns["IsActive"].HeaderText = "Активний";

                    dgv.Columns["AnimalID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgv.Columns["AnimalID"].Width = 50;
                }

                UpdateStatus($"Завантажено записів: {dataTable.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvAnimals", false)[0] as DataGridView;

                if (dgv.SelectedRows.Count > 0 && dgv.SelectedRows[0].Cells["AnimalID"].Value != null)
                {
                    int animalId = Convert.ToInt32(dgv.SelectedRows[0].Cells["AnimalID"].Value);
                    FormEditAnimal editForm = new FormEditAnimal(animalId);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Виберіть тварину для редагування!", "Увага",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            FormAddAnimal addForm = new FormAddAnimal();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = this.Controls.Find("dgvAnimals", false)[0] as DataGridView;

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
                                if (!row.IsNewRow && row.Cells["AnimalID"].Value != null)
                                {
                                    int animalId = Convert.ToInt32(row.Cells["AnimalID"].Value);
                                    string deleteQuery = "DELETE FROM Animals WHERE AnimalID = @AnimalID";
                                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                                    {
                                        cmd.Parameters.AddWithValue("@AnimalID", animalId);
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

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = Microsoft.VisualBasic.Interaction.InputBox(
                "Введіть ім'я тварини або вид для пошуку:",
                "Пошук тварин", "");

            if (!string.IsNullOrEmpty(searchTerm))
            {
                try
                {
                    DataView dv = dataTable.DefaultView;
                    dv.RowFilter = $"AnimalName LIKE '%{searchTerm}%' OR SpeciesName LIKE '%{searchTerm}%'";
                    UpdateStatus($"Знайдено записів: {dv.Count}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка пошуку: {ex.Message}", "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

    // Форма редагування тварини
    public class FormEditAnimal : Form
    {
        private int animalId;
        private TextBox txtName, txtWeight, txtSpecialNotes;
        private ComboBox cmbSpecies, cmbEnclosure, cmbGender, cmbHealthStatus;
        private DateTimePicker dtpBirthDate, dtpArrivalDate;
        private CheckBox chkSpecialDiet, chkIsActive;
        private string connectionString = @"Data Source=localhost;Initial Catalog=ZooMenuDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public FormEditAnimal(int animalId)
        {
            this.animalId = animalId;
            this.Text = "Редагувати тварину";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeControls();
            LoadComboBoxData();
            LoadAnimalData();
        }

        private void InitializeControls()
        {
            int y = 20;
            int labelWidth = 150;
            int controlWidth = 300;

            // Ім'я
            this.Controls.Add(new Label { Text = "Ім'я тварини:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtName = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(txtName);
            y += 35;

            // Вид
            this.Controls.Add(new Label { Text = "Вид:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbSpecies = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbSpecies);
            y += 35;

            // Дата народження
            this.Controls.Add(new Label { Text = "Дата народження:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            dtpBirthDate = new DateTimePicker { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(dtpBirthDate);
            y += 35;

            // Стать
            this.Controls.Add(new Label { Text = "Стать:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbGender = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "M", "F", "U" });
            this.Controls.Add(cmbGender);
            y += 35;

            // Вага
            this.Controls.Add(new Label { Text = "Вага (кг):", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtWeight = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(txtWeight);
            y += 35;

            // Вольєр
            this.Controls.Add(new Label { Text = "Вольєр:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbEnclosure = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbEnclosure);
            y += 35;

            // Стан здоров'я
            this.Controls.Add(new Label { Text = "Стан здоров'я:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbHealthStatus = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHealthStatus.Items.AddRange(new[] { "Здоровий", "Хворий", "На карантині", "Під спостереженням" });
            this.Controls.Add(cmbHealthStatus);
            y += 35;

            // Спеціальна дієта
            chkSpecialDiet = new CheckBox { Text = "Потрібна спеціальна дієта", Location = new Point(20, y), Size = new Size(200, 25) };
            this.Controls.Add(chkSpecialDiet);
            y += 35;

            // Примітки про дієту
            this.Controls.Add(new Label { Text = "Примітки про дієту:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtSpecialNotes = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 60), Multiline = true };
            this.Controls.Add(txtSpecialNotes);
            y += 70;

            // Дата прибуття
            this.Controls.Add(new Label { Text = "Дата прибуття:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            dtpArrivalDate = new DateTimePicker { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(dtpArrivalDate);
            y += 35;

            // Активний
            chkIsActive = new CheckBox { Text = "Активний", Location = new Point(20, y), Size = new Size(200, 25), Checked = true };
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

        private void LoadComboBoxData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Завантаження видів
                    string speciesQuery = "SELECT SpeciesID, SpeciesName FROM Species ORDER BY SpeciesName";
                    using (SqlCommand cmd = new SqlCommand(speciesQuery, conn))
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

                    // Завантаження вольєрів
                    string enclosuresQuery = "SELECT EnclosureID, EnclosureName FROM Enclosures ORDER BY EnclosureName";
                    using (SqlCommand cmd = new SqlCommand(enclosuresQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbEnclosure.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAnimalData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM Animals WHERE AnimalID = @AnimalID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AnimalID", animalId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtName.Text = reader["AnimalName"].ToString();

                                // Встановлення виду
                                int speciesId = reader.GetInt32(reader.GetOrdinal("SpeciesID"));
                                for (int i = 0; i < cmbSpecies.Items.Count; i++)
                                {
                                    if (((ComboItem)cmbSpecies.Items[i]).Value == speciesId)
                                    {
                                        cmbSpecies.SelectedIndex = i;
                                        break;
                                    }
                                }

                                dtpBirthDate.Value = reader.GetDateTime(reader.GetOrdinal("BirthDate"));
                                cmbGender.SelectedItem = reader["Gender"].ToString();
                                txtWeight.Text = reader["Weight"].ToString();

                                // Встановлення вольєру
                                if (reader["EnclosureID"] != DBNull.Value)
                                {
                                    int enclosureId = reader.GetInt32(reader.GetOrdinal("EnclosureID"));
                                    for (int i = 0; i < cmbEnclosure.Items.Count; i++)
                                    {
                                        if (((ComboItem)cmbEnclosure.Items[i]).Value == enclosureId)
                                        {
                                            cmbEnclosure.SelectedIndex = i;
                                            break;
                                        }
                                    }
                                }

                                cmbHealthStatus.SelectedItem = reader["HealthStatus"].ToString();
                                chkSpecialDiet.Checked = reader.GetBoolean(reader.GetOrdinal("SpecialDiet"));
                                txtSpecialNotes.Text = reader["SpecialDietNotes"]?.ToString() ?? "";
                                dtpArrivalDate.Value = reader.GetDateTime(reader.GetOrdinal("ArrivalDate"));
                                chkIsActive.Checked = reader.GetBoolean(reader.GetOrdinal("IsActive"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних тварини: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введіть ім'я тварини!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE Animals SET 
                        AnimalName = @AnimalName,
                        SpeciesID = @SpeciesID,
                        BirthDate = @BirthDate,
                        Gender = @Gender,
                        Weight = @Weight,
                        EnclosureID = @EnclosureID,
                        HealthStatus = @HealthStatus,
                        SpecialDiet = @SpecialDiet,
                        SpecialDietNotes = @SpecialDietNotes,
                        ArrivalDate = @ArrivalDate,
                        IsActive = @IsActive
                        WHERE AnimalID = @AnimalID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AnimalID", animalId);
                        cmd.Parameters.AddWithValue("@AnimalName", txtName.Text);
                        cmd.Parameters.AddWithValue("@SpeciesID", ((ComboItem)cmbSpecies.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value);
                        cmd.Parameters.AddWithValue("@Gender", cmbGender.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Weight", decimal.Parse(txtWeight.Text));
                        cmd.Parameters.AddWithValue("@EnclosureID", ((ComboItem)cmbEnclosure.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@HealthStatus", cmbHealthStatus.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@SpecialDiet", chkSpecialDiet.Checked);
                        cmd.Parameters.AddWithValue("@SpecialDietNotes",
                            string.IsNullOrWhiteSpace(txtSpecialNotes.Text) ? (object)DBNull.Value : txtSpecialNotes.Text);
                        cmd.Parameters.AddWithValue("@ArrivalDate", dtpArrivalDate.Value);
                        cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Дані успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Допоміжна форма для додавання тварин
    public class FormAddAnimal : Form
    {
        private TextBox txtName, txtWeight, txtSpecialNotes;
        private ComboBox cmbSpecies, cmbEnclosure, cmbGender, cmbHealthStatus;
        private DateTimePicker dtpBirthDate, dtpArrivalDate;
        private CheckBox chkSpecialDiet;
        private string connectionString = @"Data Source=localhost;Initial Catalog=ZooMenuDB;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        public FormAddAnimal()
        {
            this.Text = "Додати нову тварину";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            InitializeControls();
            LoadComboBoxData();
        }

        private void InitializeControls()
        {
            int y = 20;
            int labelWidth = 150;
            int controlWidth = 300;

            // Ім'я
            this.Controls.Add(new Label { Text = "Ім'я тварини:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtName = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(txtName);
            y += 35;

            // Вид
            this.Controls.Add(new Label { Text = "Вид:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbSpecies = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbSpecies);
            y += 35;

            // Дата народження
            this.Controls.Add(new Label { Text = "Дата народження:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            dtpBirthDate = new DateTimePicker { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            dtpBirthDate.Value = DateTime.Now.AddYears(-2);
            this.Controls.Add(dtpBirthDate);
            y += 35;

            // Стать
            this.Controls.Add(new Label { Text = "Стать:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbGender = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "M", "F", "U" });
            cmbGender.SelectedIndex = 2;
            this.Controls.Add(cmbGender);
            y += 35;

            // Вага
            this.Controls.Add(new Label { Text = "Вага (кг):", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtWeight = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), Text = "50" };
            this.Controls.Add(txtWeight);
            y += 35;

            // Вольєр
            this.Controls.Add(new Label { Text = "Вольєр:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbEnclosure = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            this.Controls.Add(cmbEnclosure);
            y += 35;

            // Стан здоров'я
            this.Controls.Add(new Label { Text = "Стан здоров'я:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            cmbHealthStatus = new ComboBox { Location = new Point(180, y), Size = new Size(controlWidth, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbHealthStatus.Items.AddRange(new[] { "Здоровий", "Хворий", "На карантині", "Під спостереженням" });
            cmbHealthStatus.SelectedIndex = 0;
            this.Controls.Add(cmbHealthStatus);
            y += 35;

            // Спеціальна дієта
            chkSpecialDiet = new CheckBox { Text = "Потрібна спеціальна дієта", Location = new Point(20, y), Size = new Size(200, 25) };
            this.Controls.Add(chkSpecialDiet);
            y += 35;

            // Примітки про дієту
            this.Controls.Add(new Label { Text = "Примітки про дієту:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            txtSpecialNotes = new TextBox { Location = new Point(180, y), Size = new Size(controlWidth, 60), Multiline = true };
            this.Controls.Add(txtSpecialNotes);
            y += 70;

            // Дата прибуття
            this.Controls.Add(new Label { Text = "Дата прибуття:", Location = new Point(20, y), Size = new Size(labelWidth, 20) });
            dtpArrivalDate = new DateTimePicker { Location = new Point(180, y), Size = new Size(controlWidth, 25) };
            this.Controls.Add(dtpArrivalDate);
            y += 40;

            // Кнопки
            Button btnSave = new Button { Text = "Зберегти", Location = new Point(180, y), Size = new Size(120, 35) };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnCancel = new Button { Text = "Скасувати", Location = new Point(310, y), Size = new Size(120, 35) };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Завантаження видів
                    string speciesQuery = "SELECT SpeciesID, SpeciesName FROM Species ORDER BY SpeciesName";
                    using (SqlCommand cmd = new SqlCommand(speciesQuery, conn))
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

                    // Завантаження вольєрів
                    string enclosuresQuery = "SELECT EnclosureID, EnclosureName FROM Enclosures ORDER BY EnclosureName";
                    using (SqlCommand cmd = new SqlCommand(enclosuresQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbEnclosure.Items.Add(new ComboItem
                                {
                                    Value = reader.GetInt32(0),
                                    Text = reader.GetString(1)
                                });
                            }
                        }
                    }
                    if (cmbEnclosure.Items.Count > 0) cmbEnclosure.SelectedIndex = 0;
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
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введіть ім'я тварини!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Animals 
                        (AnimalName, SpeciesID, BirthDate, Gender, Weight, EnclosureID, HealthStatus, 
                         SpecialDiet, SpecialDietNotes, ArrivalDate, IsActive, CreatedDate)
                        VALUES 
                        (@AnimalName, @SpeciesID, @BirthDate, @Gender, @Weight, @EnclosureID, @HealthStatus,
                         @SpecialDiet, @SpecialDietNotes, @ArrivalDate, @IsActive, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AnimalName", txtName.Text);
                        cmd.Parameters.AddWithValue("@SpeciesID", ((ComboItem)cmbSpecies.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value);
                        cmd.Parameters.AddWithValue("@Gender", cmbGender.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Weight", decimal.Parse(txtWeight.Text));
                        cmd.Parameters.AddWithValue("@EnclosureID", ((ComboItem)cmbEnclosure.SelectedItem).Value);
                        cmd.Parameters.AddWithValue("@HealthStatus", cmbHealthStatus.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@SpecialDiet", chkSpecialDiet.Checked);
                        cmd.Parameters.AddWithValue("@SpecialDietNotes",
                            string.IsNullOrWhiteSpace(txtSpecialNotes.Text) ? (object)DBNull.Value : txtSpecialNotes.Text);
                        cmd.Parameters.AddWithValue("@ArrivalDate", dtpArrivalDate.Value);
                        cmd.Parameters.AddWithValue("@IsActive", true);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Тварину успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Допоміжний клас для ComboBox
    public class ComboItem
    {
        public int Value { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}