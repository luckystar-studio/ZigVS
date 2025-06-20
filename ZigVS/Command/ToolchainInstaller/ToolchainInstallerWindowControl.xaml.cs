namespace ZigVS
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

#nullable enable
    /// <summary>
    /// Interaction logic for ToolWindowControl.
    /// </summary>
    public partial class ToolchainInstallerWindowControl : System.Windows.Controls.UserControl
    {
        ToolchainInstaller m_ToolchainInstaller = new ToolchainInstaller();

        ToolchainInstallerWindowViewModel m_ToolchainInstallerWindowViewModel = new ToolchainInstallerWindowViewModel();

        bool m_ZigBool = false;
        bool m_ZLSBool = false;
        bool m_CPUBool = false;
        bool m_DirectoryBool = false;
        bool m_EnvBool = false;

        System.Windows.Media.Brush m_greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16,0x84,0x00));

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolchainInstallerWindowControl"/> class.
        /// </summary>
        public ToolchainInstallerWindowControl()
        {
            InitializeComponent();

            DataContext = m_ToolchainInstallerWindowViewModel;

            m_ToolchainInstallerWindowControl = this;

            m_ZigVersion_ComboBox.Items.Add("0.14.1");
            m_ZigVersion_ComboBox.Items.Add("0.14.0");
            m_ZigVersion_ComboBox.Items.Add("master");

//            m_ZLSVersion_ComboBox.Items.Add("0.14.1");
            m_ZLSVersion_ComboBox.Items.Add("0.14.0");

            m_CPU_ComboBox.Items.Add("x86_64");
            m_CPU_ComboBox.Items.Add("aarch64");

            m_DoNotSet_Radio_Button.Foreground = Title.Foreground;
            m_DoNotSet_Radio_Button.Background = Title.Background;
            m_DoSet_Path_Radio_Button.Foreground = Title.Foreground;
            m_DoSet_Path_Radio_Button.Background = Title.Background;

            Reset();
        }

        static ToolchainInstallerWindowControl? m_ToolchainInstallerWindowControl = null;
        static public ToolchainInstallerWindowControl? GetInstance()
        {
            return m_ToolchainInstallerWindowControl;
        }

        public void Reset()
        {
            UpdateInstallerButtonText();
        }

        void UpdateInstallerButtonText()
        {
            if (m_ToolchainInstaller.GetState() == InstallerBase.State.Installing)
            {
                m_ToolchainInstallerWindowViewModel.InstallButtonTextString = "Cancel";
            }
            else
            {
                m_ToolchainInstallerWindowViewModel.InstallButtonTextString = "Install";
            }
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        void directory_button_Click(object sender, RoutedEventArgs e)
        {
            var l_FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (l_FolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                m_DirecotryPath_TextBox.Text = l_FolderBrowserDialog.SelectedPath;
                m_DirectoryBool = Common.File.HasWritePermissionOnDir(m_DirecotryPath_TextBox.Text);
                m_Directory_TextBlock.Foreground = m_DirectoryBool ? m_greenBrush : Title.Foreground;
                Check_Status();
            }
        }

        void DirectoryPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            bool l_ifPathIsValid = false;
            try
            {
                if(System.IO.Path.IsPathRooted(m_DirecotryPath_TextBox.Text))
                {
                    var l_path = System.IO.Path.GetFullPath(m_DirecotryPath_TextBox.Text);

                    l_ifPathIsValid = Common.File.HasWritePermissionOnDir(l_path);
                }
            
            }
            catch {}

            m_DirectoryBool = l_ifPathIsValid;
            m_Directory_TextBlock.Foreground = m_DirectoryBool ? m_greenBrush : Title.Foreground;
            Check_Status();
        }

        bool? GetIsChecked()
        {
            return m_DoSet_Path_Radio_Button.IsChecked;
        }

        void ZigVersion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            m_ZigBool = true;
            m_Zig_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void ZLSVersion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            m_ZLSBool = true;
            m_LS_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void CPU_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            m_CPUBool = true;
            m_CPU_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void DoNotSet_Click(object sender, RoutedEventArgs e)
        {
            m_EnvBool = true;
            m_Env_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void DoSet_Path_Click(object sender, RoutedEventArgs e)
        {
            m_EnvBool = true;
            m_Env_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void DoSet_Click(object sender, RoutedEventArgs e)
        {
            m_EnvBool = true;
            m_Env_TextBlock.Foreground = m_greenBrush;
            Check_Status();
        }

        void Check_Status()
        {
            m_ToolchainInstallerWindowViewModel.InstallButtonEnabled = m_ZigBool && m_ZLSBool && m_CPUBool & m_DirectoryBool && m_EnvBool;
        }

        void install_button_Click(object sender, RoutedEventArgs e)
        {
            if (m_ToolchainInstaller.GetState() == InstallerBase.State.None)
            {
                bool l_checked = (m_DoSet_Path_Radio_Button.IsChecked != null) && m_DoSet_Path_Radio_Button.IsChecked.Value;

#pragma warning disable VSTHRD010
                m_ToolchainInstaller.Start(m_ZigVersion_ComboBox.Text, m_ZLSVersion_ComboBox.Text, m_CPU_ComboBox.Text, m_DirecotryPath_TextBox.Text, l_checked);
#pragma warning restore VSTHRD010
            }
            else if (m_ToolchainInstaller.GetState() == InstallerBase.State.Installing)
            {
                m_ToolchainInstaller.CancelInstallation();
            }
            UpdateInstallerButtonText();
        }
    }

    public class ToolchainInstallerWindowViewModel : INotifyPropertyChanged
    {
        private string m_InstallButtonTextString="";

        public string InstallButtonTextString
        {
            get => m_InstallButtonTextString;
            set
            {
                if (m_InstallButtonTextString != value)
                {
                    m_InstallButtonTextString = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool m_InstallButtonEnabled = false;
        public bool InstallButtonEnabled
        {
            get => m_InstallButtonEnabled;
            set
            {
                if (m_InstallButtonEnabled != value)
                {
                    m_InstallButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string i_propertyNameString = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(i_propertyNameString));
        }
    }
}