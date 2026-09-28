namespace SteamSwitcherApp;

public partial class Form1 : Form
{
    private readonly System.Windows.Forms.Timer gamepadTimer;
    private ushort previousButtons;

    private string? filePath;

    private Button browseButton;
    private Button loadButton;

    private readonly List<Account> accounts = new();

    private ListBox accountList;
    private Label fileStatusLabel;

    public Form1()
    {
        InitializeComponent();

        Text = "Steam Switcher";
        Width = 500;
        Height = 400;


        // =========================
        // LOAD ACCOUNT BUTTON
        // =========================

        loadButton = new Button();

        loadButton.Text = "Load Account";
        loadButton.Width = 150;
        loadButton.Height = 40;
        loadButton.Left = 90;
        loadButton.Top = 10;

        // Không cho button tự nhận focus bằng Tab
        loadButton.TabStop = false;

        Controls.Add(loadButton);

        loadButton.Click += LoadButton_Click;


        // =========================
        // ACCOUNT LIST
        // =========================

        accountList = new ListBox();

        accountList.Width = 300;
        accountList.Height = 180;
        accountList.Left = 90;
        accountList.Top = 60;

        accountList.TabIndex = 0;

        Controls.Add(accountList);

        accountList.DoubleClick += AccountList_DoubleClick;
        accountList.KeyDown += AccountList_KeyDown;
        fileStatusLabel = new Label();

        fileStatusLabel.AutoSize = true;
        fileStatusLabel.Left = 300;
        fileStatusLabel.Top = 280;

        fileStatusLabel.Text = "No file selected";

        Controls.Add(fileStatusLabel);

        // =========================
        // FILE ACCOUNT BUTTON
        // =========================

        browseButton = new Button();

        browseButton.Text = "File account";
        browseButton.Width = 200;
        browseButton.Height = 40;
        browseButton.Left = 90;
        browseButton.Top = 270;

        browseButton.TabStop = false;

        Controls.Add(browseButton);

        browseButton.Click += BrowseButton_Click;


        // =========================
        // READ SAVED FILE PATH
        // =========================
        loadButton.Enabled = false;
        if (File.Exists("config.txt"))
        {
            filePath = File.ReadAllText("config.txt");
            if (File.Exists(filePath))
            {
                fileStatusLabel.Text = "File ready";
                loadButton.Enabled = true;
            }
            else
                {
                    fileStatusLabel.Text = "File not found";
                    loadButton.Enabled = false;
                }
            
        }


        // =========================
        // GAMEPAD TIMER
        // =========================

        gamepadTimer = new System.Windows.Forms.Timer();

        gamepadTimer.Interval = 50;
        gamepadTimer.Tick += GamepadTimer_Tick;

        gamepadTimer.Start();


        // Khi form hiện ra thì focus vào ListBox
        Shown += Form1_Shown;
       
    }


    // =========================
    // FORM SHOWN
    // =========================

    private void Form1_Shown(object? sender, EventArgs e)
    {
        loadButton.Focus();
    }


    // =========================
    // DOUBLE CLICK ACCOUNT
    // =========================

    private void AccountList_DoubleClick(object? sender, EventArgs e)
    {
        SelectAccount();
    }


    // =========================
    // KEYBOARD
    // =========================

    private void AccountList_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            SelectAccount();
        }
    }


    // =========================
    // SELECT ACCOUNT
    // =========================

    private async void SelectAccount()
    {
        if (accountList.SelectedItem is Account account)
        {
            await SteamManager.Login(account);
            Application.Exit();
        }
    }


    // =========================
    // CHOOSE TXT FILE
    // =========================

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Filter =
            "Text files (*.txt)|*.txt|All files (*.*)|*.*";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            filePath = openFileDialog.FileName;

            // Chỉ lưu đường dẫn
            File.WriteAllText("config.txt", filePath);

            loadButton.Enabled = true;
            fileStatusLabel.Text = "File ready";
        }
    }


    // =========================
    // LOAD BUTTON
    // =========================

    private void LoadButton_Click(object? sender, EventArgs e)
    {
        if (filePath != null && File.Exists(filePath))
        {
            LoadAccounts(filePath);

            accountList.Focus();
        }
        else
        {
            MessageBox.Show(
                "Please choose an account file first.",
                "Steam Switcher"
            );
        }
    }


    // =========================
    // LOAD ACCOUNTS FROM TXT
    // =========================

    private void LoadAccounts(string path)
    {
        accountList.Items.Clear();
        accounts.Clear();

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            // Bỏ dòng trống
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split('|');

            // Format phải là:
            // DisplayName|Username|Password
            if (parts.Length != 3)
                continue;

            Account account = new Account(
                parts[0].Trim(),
                parts[1].Trim(),
                parts[2].Trim()
            );

            accounts.Add(account);

            accountList.Items.Add(account);
        }


        if (accountList.Items.Count > 0)
        {
            accountList.SelectedIndex = 0;
        }
    }


    // =========================
    // GAMEPAD
    // =========================

    private void GamepadTimer_Tick(object? sender, EventArgs e)
    {
        ushort buttons = GamepadManager.GetButtons();


        // D-PAD UP
        if (IsNewPress(buttons, GamepadManager.DPadUp))
        {
            if (accountList.SelectedIndex > 0)
            {
                accountList.SelectedIndex--;
            }
        }


        // D-PAD DOWN
        if (IsNewPress(buttons, GamepadManager.DPadDown))
        {
            if (accountList.SelectedIndex <
                accountList.Items.Count - 1)
            {
                accountList.SelectedIndex++;
            }
        }


        // A = LOGIN
        if (IsNewPress(buttons, GamepadManager.A))
        {
            if (loadButton.Focused)
            {
                loadButton.PerformClick();
            }
            else if (accountList.Focused)
            {
                SelectAccount();
            }
        }


        // B = CLOSE APP
        if (IsNewPress(buttons, GamepadManager.B))
        {
            Close();
        }


        // Save current state
        previousButtons = buttons;
    }


    // =========================
    // CHECK NEW BUTTON PRESS
    // =========================

    private bool IsNewPress(
        ushort currentButtons,
        ushort button)
    {
        bool pressedNow =
            (currentButtons & button) != 0;

        bool pressedBefore =
            (previousButtons & button) != 0;

        return pressedNow && !pressedBefore;
    }
}