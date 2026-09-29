namespace SteamSwitcherApp;

public partial class Form1 : Form
{
    private readonly System.Windows.Forms.Timer gamepadTimer;
    private ushort previousButtons;

    private string? filePath;

    private Button browseButton = null!;
    private Button loadButton = null!;
    private Button openSteamButton = null!;
    private Button fillAccountButton = null!;

    private readonly List<Account> accounts = new();
    private bool isBusy;

    private ListView accountList = null!;
    private Label fileStatusLabel = null!;
    private TextBox selectedAccountBox = null!;
    private TextBox statusBox = null!;

    private readonly Color backgroundColor = Color.FromArgb(239, 245, 252);
    private readonly Color primaryColor = Color.FromArgb(21, 111, 232);
    private readonly Color primaryFocusColor = Color.FromArgb(0, 82, 204);
    private readonly Color secondaryColor = Color.FromArgb(248, 251, 254);
    private readonly Color secondaryFocusColor = Color.FromArgb(225, 239, 255);
    private readonly Color focusBorderColor = Color.FromArgb(255, 176, 0);
    private readonly Color disabledColor = Color.FromArgb(224, 229, 236);
    private readonly Color disabledTextColor = Color.FromArgb(116, 127, 141);
    private readonly Color textColor = Color.FromArgb(12, 24, 43);
    private readonly Color readyColor = Color.FromArgb(222, 248, 228);

    public Form1()
    {
        InitializeComponent();

        Text = "Steam Switcher";
        Width = 1080;
        Height = 760;
        MinimumSize = new Size(1000, 720);
        StartPosition = FormStartPosition.Manual;
        BackColor = backgroundColor;
        Font = new Font("Segoe UI", 11F);
        KeyPreview = true;

        BuildLayout();
        ReadSavedFilePath();

        gamepadTimer = new System.Windows.Forms.Timer();
        gamepadTimer.Interval = 50;
        gamepadTimer.Tick += GamepadTimer_Tick;
        gamepadTimer.Start();

        Shown += Form1_Shown;
        KeyDown += Form1_KeyDown;
    }


    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MoveToRightSideOfScreen();
    }


    private void BuildLayout()
    {
        loadButton = CreatePrimaryButton("Load Account File", 48, 32, 270, 54);
        loadButton.Enabled = false;
        loadButton.Click += LoadButton_Click;
        Controls.Add(loadButton);

        openSteamButton = CreateSecondaryButton("Open Steam", 338, 32, 210, 54);
        openSteamButton.Click += OpenSteamButton_Click;
        Controls.Add(openSteamButton);

        fileStatusLabel = new Label();
        fileStatusLabel.Text = "No file";
        fileStatusLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        fileStatusLabel.ForeColor = Color.FromArgb(17, 123, 40);
        fileStatusLabel.BackColor = readyColor;
        fileStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
        fileStatusLabel.Left = 838;
        fileStatusLabel.Top = 32;
        fileStatusLabel.Width = 170;
        fileStatusLabel.Height = 54;
        Controls.Add(fileStatusLabel);

        browseButton = new Button();
        browseButton.Text = "Choose File";
        browseButton.Left = 48;
        browseButton.Top = 32;
        browseButton.Width = 270;
        browseButton.Height = 54;
        browseButton.Visible = false;
        browseButton.Click += BrowseButton_Click;
        Controls.Add(browseButton);

        fillAccountButton = CreatePrimaryButton("Fill Account", 568, 32, 250, 54);
        fillAccountButton.Enabled = false;
        fillAccountButton.Click += FillAccountButton_Click;
        Controls.Add(fillAccountButton);

        selectedAccountBox = new TextBox();
        selectedAccountBox.Left = 48;
        selectedAccountBox.Top = 104;
        selectedAccountBox.Width = 960;
        selectedAccountBox.Height = 42;
        selectedAccountBox.Font = new Font("Segoe UI", 14F);
        selectedAccountBox.ReadOnly = true;
        selectedAccountBox.BackColor = Color.FromArgb(248, 251, 254);
        selectedAccountBox.ForeColor = textColor;
        Controls.Add(selectedAccountBox);

        accountList = new ListView();
        accountList.Left = 48;
        accountList.Top = 170;
        accountList.Width = 960;
        accountList.Height = 500;
        accountList.View = View.Details;
        accountList.FullRowSelect = true;
        accountList.HideSelection = false;
        accountList.GridLines = true;
        accountList.MultiSelect = false;
        accountList.Font = new Font("Segoe UI", 13F);
        accountList.BackColor = Color.White;
        accountList.ForeColor = textColor;
        accountList.Columns.Add("#", 80);
        accountList.Columns.Add("Account", 850);
        accountList.DoubleClick += AccountList_DoubleClick;
        accountList.KeyDown += AccountList_KeyDown;
        accountList.SelectedIndexChanged += AccountList_SelectedIndexChanged;
        Controls.Add(accountList);

        statusBox = new TextBox();
        statusBox.Left = 48;
        statusBox.Top = 690;
        statusBox.Width = 960;
        statusBox.Height = 0;
        statusBox.Multiline = true;
        statusBox.ReadOnly = true;
        statusBox.ScrollBars = ScrollBars.Vertical;
        statusBox.TabStop = false;
        statusBox.Visible = false;
        Controls.Add(statusBox);
    }


    private void MoveToRightSideOfScreen()
    {
        Rectangle workingArea = Screen.FromControl(this).WorkingArea;
        int margin = 24;

        int left = workingArea.Right - Width - margin;
        int top = workingArea.Top + margin;

        if (left < workingArea.Left + margin)
        {
            left = workingArea.Left + margin;
        }

        Location = new Point(left, top);
    }


    private Button CreatePrimaryButton(
        string text,
        int left,
        int top,
        int width,
        int height)
    {
        Button button = CreateBaseButton(text, left, top, width, height);
        button.Tag = "primary";
        UpdateButtonVisual(button);
        return button;
    }


    private Button CreateSecondaryButton(
        string text,
        int left,
        int top,
        int width,
        int height)
    {
        Button button = CreateBaseButton(text, left, top, width, height);
        button.Tag = "secondary";
        UpdateButtonVisual(button);
        return button;
    }


    private Button CreateBaseButton(
        string text,
        int left,
        int top,
        int width,
        int height)
    {
        Button button = new Button();
        button.Text = text;
        button.Left = left;
        button.Top = top;
        button.Width = width;
        button.Height = height;
        button.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.TabStop = false;
        button.GotFocus += Button_FocusChanged;
        button.LostFocus += Button_FocusChanged;
        button.EnabledChanged += Button_EnabledChanged;
        return button;
    }


    private void Button_FocusChanged(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            UpdateButtonVisual(button);
        }
    }


    private void Button_EnabledChanged(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            UpdateButtonVisual(button);
        }
    }


    private void UpdateButtonVisual(Button button)
    {
        bool isPrimary = button.Tag as string == "primary";

        if (!button.Enabled)
        {
            button.BackColor = disabledColor;
            button.ForeColor = disabledTextColor;
            button.FlatAppearance.BorderColor = disabledColor;
            button.FlatAppearance.BorderSize = 1;
            return;
        }

        if (button.Focused)
        {
            button.BackColor =
                isPrimary ? primaryFocusColor : secondaryFocusColor;
            button.ForeColor = isPrimary ? Color.White : textColor;
            button.FlatAppearance.BorderColor = focusBorderColor;
            button.FlatAppearance.BorderSize = 3;
            return;
        }

        button.BackColor = isPrimary ? primaryColor : secondaryColor;
        button.ForeColor = isPrimary ? Color.White : textColor;
        button.FlatAppearance.BorderColor =
            isPrimary
                ? primaryColor
                : Color.FromArgb(184, 199, 214);
        button.FlatAppearance.BorderSize = 1;
    }


    private void ReadSavedFilePath()
    {
        loadButton.Enabled = false;

        if (!File.Exists("config.txt"))
        {
            fileStatusLabel.Text = "No file";
            return;
        }

        filePath = File.ReadAllText("config.txt");

        if (File.Exists(filePath))
        {
            fileStatusLabel.Text = "File ready";
            loadButton.Enabled = true;
            return;
        }

        fileStatusLabel.Text = "File not found";
    }


    private void Form1_Shown(object? sender, EventArgs e)
    {
        loadButton.Focus();
    }


    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
                MoveGamepadFocus(-1);
                e.Handled = true;
                break;

            case Keys.Right:
                MoveGamepadFocus(1);
                e.Handled = true;
                break;

            case Keys.Up:
                MoveAccountOrFocusList(-1);
                e.Handled = true;
                break;

            case Keys.Down:
                MoveAccountOrFocusList(1);
                e.Handled = true;
                break;

            case Keys.Enter:
            case Keys.Space:
                PressFocusedControl();
                e.Handled = true;
                break;

            case Keys.X:
                PressOpenSteamShortcut();
                e.Handled = true;
                break;

            case Keys.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }


    private void AccountList_DoubleClick(object? sender, EventArgs e)
    {
        SelectAccount();
    }


    private void AccountList_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            SelectAccount();
        }
    }


    private void AccountList_SelectedIndexChanged(
        object? sender,
        EventArgs e)
    {
        SelectAccount();
    }


    private void SelectAccount()
    {
        Account? account = GetSelectedAccount();

        if (account == null)
        {
            selectedAccountBox.Clear();
            UpdateFillAccountButton();
            return;
        }

        selectedAccountBox.Text = account.Displaygame;
        fileStatusLabel.Text = "Selected";
        AddStatus("Selected account: " + account.Displaygame);
        UpdateFillAccountButton();
    }


    private async void OpenSteamButton_Click(
        object? sender,
        EventArgs e)
    {
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        SetControlsEnabled(false);
        ShowStatusBox();
        statusBox.Clear();
        AddStatus("Starting Steam...");

        try
        {
            await SteamManager.OpenSteam(AddStatus);
            AddStatus("Steam opened. Wait for the login window, then press Fill Account.");
            fileStatusLabel.Text = "Steam ready";
        }
        catch (Exception ex)
        {
            AddStatus("Open Steam failed: " + ex.Message);

            MessageBox.Show(
                ex.Message,
                "Steam Switcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            fileStatusLabel.Text = "Open failed";
        }
        finally
        {
            isBusy = false;
            SetControlsEnabled(true);
        }
    }


    private async void FillAccountButton_Click(
        object? sender,
        EventArgs e)
    {
        Account? account = GetSelectedAccount();

        if (isBusy || account == null)
        {
            return;
        }

        isBusy = true;
        SetControlsEnabled(false);
        fileStatusLabel.Text = "Filling";
        ShowStatusBox();
        AddStatus("Filling selected account...");

        try
        {
            await SteamManager.FillLogin(account, AddStatus);
            AddStatus("Account submitted.");
            fileStatusLabel.Text = "Submitted";
        }
        catch (Exception ex)
        {
            AddStatus("Fill account failed: " + ex.Message);

            MessageBox.Show(
                ex.Message,
                "Steam Switcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            fileStatusLabel.Text = "Fill failed";
        }
        finally
        {
            isBusy = false;
            SetControlsEnabled(true);
            accountList.Focus();
        }
    }


    private void AddStatus(string message)
    {
        string time = DateTime.Now.ToString("HH:mm:ss");

        statusBox.AppendText(
            $"[{time}] {message}{Environment.NewLine}"
        );
    }


    private void ShowStatusBox()
    {
        statusBox.Visible = true;
        statusBox.Height = 90;
        Height = 900;
    }


    private void SetControlsEnabled(bool enabled)
    {
        loadButton.Enabled =
            enabled &&
            filePath != null &&
            File.Exists(filePath);

        browseButton.Enabled = enabled;
        openSteamButton.Enabled = enabled;
        accountList.Enabled = enabled;

        UpdateFillAccountButton();
    }


    private void UpdateFillAccountButton()
    {
        fillAccountButton.Enabled =
            !isBusy &&
            GetSelectedAccount() != null;
    }


    private Account? GetSelectedAccount()
    {
        if (accountList.SelectedItems.Count == 0)
        {
            return null;
        }

        return accountList.SelectedItems[0].Tag as Account;
    }


    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Filter =
            "Text files (*.txt)|*.txt|All files (*.*)|*.*";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            filePath = openFileDialog.FileName;
            File.WriteAllText("config.txt", filePath);

            loadButton.Enabled = true;
            fileStatusLabel.Text = "File ready";
        }
    }


    private void LoadButton_Click(object? sender, EventArgs e)
    {
        if (filePath != null && File.Exists(filePath))
        {
            LoadAccounts(filePath);
            accountList.Focus();
            return;
        }

        MessageBox.Show(
            "Please choose an account file first.",
            "Steam Switcher"
        );
    }


    private void LoadAccounts(string path)
    {
        accountList.Items.Clear();
        selectedAccountBox.Clear();
        accounts.Clear();

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] parts = line.Split('|');

            if (parts.Length != 3)
            {
                continue;
            }

            Account account = new Account(
                parts[0].Trim(),
                parts[1].Trim(),
                parts[2].Trim()
            );

            accounts.Add(account);

            ListViewItem item = new ListViewItem(accounts.Count.ToString());
            item.SubItems.Add(account.Displaygame ?? "");
            item.Tag = account;
            accountList.Items.Add(item);
        }

        if (accountList.Items.Count > 0)
        {
            accountList.Items[0].Selected = true;
            accountList.Items[0].Focused = true;
            SelectAccount();
        }

        AddStatus($"Loaded {accountList.Items.Count} account(s).");
    }


    private void GamepadTimer_Tick(object? sender, EventArgs e)
    {
        ushort buttons = GamepadManager.GetButtons();

        if (IsNewPress(buttons, GamepadManager.DPadLeft))
        {
            MoveGamepadFocus(-1);
        }

        if (IsNewPress(buttons, GamepadManager.DPadRight))
        {
            MoveGamepadFocus(1);
        }

        if (IsNewPress(buttons, GamepadManager.DPadUp))
        {
            MoveAccountOrFocusList(-1);
        }

        if (IsNewPress(buttons, GamepadManager.DPadDown))
        {
            MoveAccountOrFocusList(1);
        }

        if (IsNewPress(buttons, GamepadManager.A))
        {
            PressFocusedControl();
        }

        if (IsNewPress(buttons, GamepadManager.X))
        {
            PressOpenSteamShortcut();
        }

        if (IsNewPress(buttons, GamepadManager.B))
        {
            Close();
        }

        previousButtons = buttons;
    }


    private void MoveGamepadFocus(int direction)
    {
        Control[] focusTargets =
        {
            loadButton,
            openSteamButton,
            fillAccountButton,
            accountList
        };

        int currentIndex = Array.FindIndex(
            focusTargets,
            control => control.Focused
        );

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        for (int i = 1; i <= focusTargets.Length; i++)
        {
            int nextIndex =
                (currentIndex + direction * i + focusTargets.Length) %
                focusTargets.Length;

            Control nextControl = focusTargets[nextIndex];

            if (nextControl.Enabled)
            {
                nextControl.Focus();
                return;
            }
        }
    }


    private void MoveAccountOrFocusList(int direction)
    {
        if (accountList.Focused)
        {
            MoveSelection(direction);
            return;
        }

        accountList.Focus();
    }


    private void PressOpenSteamShortcut()
    {
        if (openSteamButton.Enabled)
        {
            openSteamButton.PerformClick();
        }
    }


    private void PressFocusedControl()
    {
        if (loadButton.Focused && loadButton.Enabled)
        {
            loadButton.PerformClick();
            return;
        }

        if (openSteamButton.Focused && openSteamButton.Enabled)
        {
            openSteamButton.PerformClick();
            return;
        }

        if (fillAccountButton.Focused && fillAccountButton.Enabled)
        {
            fillAccountButton.PerformClick();
            return;
        }

        if (accountList.Focused && fillAccountButton.Enabled)
        {
            fillAccountButton.PerformClick();
        }
    }


    private void MoveSelection(int direction)
    {
        if (accountList.Items.Count == 0)
        {
            return;
        }

        int selectedIndex =
            accountList.SelectedIndices.Count > 0
                ? accountList.SelectedIndices[0]
                : 0;

        int nextIndex = Math.Clamp(
            selectedIndex + direction,
            0,
            accountList.Items.Count - 1
        );

        accountList.Items[nextIndex].Selected = true;
        accountList.Items[nextIndex].Focused = true;
        accountList.EnsureVisible(nextIndex);
    }


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
