namespace ZigVS
{
    using Microsoft.VisualStudio.Shell;
    using Microsoft.Web.WebView2.Core;
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

#nullable enable
#pragma warning disable VSTHRD002,VSTHRD010
    /// <summary>
    /// Interaction logic for PackageBrowserWindowControl.xaml
    /// </summary>
    public partial class PackageInstallerWindowControl : UserControl
    {
        PackageInstaller m_PackageInstaller = new PackageInstaller();

        PackageInstallerWindowViewModel m_PackageInstallerWindowViewModel = new PackageInstallerWindowViewModel();

        System.Windows.Media.Brush m_greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x16, 0x84, 0x00));

        bool m_repogitoryBool = false;
        bool m_methodBool = false;

        string m_zipUrlString = "";
        string m_targzUrlString = "";
        string m_gitUrlString = "";

        string m_workingDirectoryPathString = "";

        const string c_targzExtensionString = ".tar.gz";
        const string c_zipExtensionString = ".zip";
        const string c_methodGit = "git";
        const string c_methodUnzip = "unzip";
        const string c_methodZigFetch = "zig fetch   (Open Folder Mode)";
        const string c_methodAdd = "add package (Project File Mode)";

        public PackageInstallerWindowControl()
        {
            m_PackageInstallerWindowControl = this;
            InitializeComponent();

            DataContext = m_PackageInstallerWindowViewModel;

#pragma warning disable CS1998, CS4014
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                InitializeWebViewAsync();
            });
#pragma warning restore CS1998, CS4014

            m_Method_ComboBox.Items.Add(c_methodZigFetch);
            m_Method_ComboBox.Items.Add(c_methodGit);
            m_Method_ComboBox.Items.Add(c_methodUnzip);
            m_Method_ComboBox.Items.Add(c_methodAdd);

            Reset();
        }

        static PackageInstallerWindowControl? m_PackageInstallerWindowControl = null;
        static public PackageInstallerWindowControl? GetInstance()
        {
            return m_PackageInstallerWindowControl;
        }

        public void Reset()
        {
            UpdateInstallerButtonText();
        }

        void UpdateInstallerButtonText()
        {
            if (m_PackageInstaller.GetState() == InstallerBase.State.Installing)
            {
                m_PackageInstallerWindowViewModel.InstallButtonTextString = "Cancel";
            }
            else
            {
                m_PackageInstallerWindowViewModel.InstallButtonTextString = "Install";
            }
        }

        async Task InitializeWebViewAsync()
        {
            // create webview2 environment and load the webview
            string l_webViewDirectoryString = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            l_webViewDirectoryString = System.IO.Path.Combine(l_webViewDirectoryString, "LuckyStarStudio","ZigVS");
            System.IO.Directory.CreateDirectory(l_webViewDirectoryString);

            var l_CoreWebView2Environment = await CoreWebView2Environment.CreateAsync(null, l_webViewDirectoryString);

            // load the webview2
            await m_WebView2.EnsureCoreWebView2Async(l_CoreWebView2Environment);

            GoHome();
        }

        void Home_Click(object sender, RoutedEventArgs e)
        {
            GoHome();
        }
        
        void GoHome()
        {
            var l_GeneralOptions = GeneralOptions.GetLiveInstanceAsync().Result;
            m_WebView2.Source = new Uri(l_GeneralOptions.HomeUrl);
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            if(m_WebView2.CanGoBack)
            {
                m_WebView2.GoBack();
            }
        }

        void Forword_Click(object sender, RoutedEventArgs e)
        {
            if(m_WebView2.CanGoForward)
            {
                m_WebView2.GoForward();
            }
        }

        void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckStatus();
        }

        void webView_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            m_URL_TextBlock.Text = m_WebView2.Source.ToString();
            CheckStatus();
        }

        void Method_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckStatus();
        }

        void Install_button_Click(object sender, RoutedEventArgs e)
        {
            CheckStatus();

            if (m_PackageInstallerWindowViewModel.InstallButtonEnabled)
            {
                if (m_PackageInstaller.GetState() == InstallerBase.State.None)
                {
                    var l_GeneralOptions = GeneralOptions.GetLiveInstanceAsync().Result;
                    var l_FolderModeOptions = FolderModeOptions.GetLiveInstanceAsync().Result;

                    if (string.IsNullOrEmpty(m_Method_ComboBox.Text))
                    {
                    }
                    else if (m_Method_ComboBox.Text == c_methodGit)
                    {
                        m_PackageInstaller.StartCommand(m_workingDirectoryPathString, l_GeneralOptions.GitPath, l_GeneralOptions.GitOption + ' ' + m_gitUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodUnzip)
                    {
                        m_PackageInstaller.StartUnzip(m_workingDirectoryPathString, m_zipUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodZigFetch && Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
                    {
                        var l_toolPathString = System.IO.Path.Combine(Utilities.GetToolPathFromEnvironmentValue(), l_FolderModeOptions.ToolPath);
                        m_PackageInstaller.StartCommand(m_workingDirectoryPathString, l_toolPathString, l_GeneralOptions.ZigFetchOption + ' ' + m_targzUrlString);
                    }
                    else if (m_Method_ComboBox.Text == c_methodAdd && Utilities.GetSolutionMode() == Utilities.SolutionMode.ProjectMode)
                    {
                    }
                    UpdateInstallerButtonText();
                }
                else if (m_PackageInstaller.GetState() == InstallerBase.State.Installing)
                {
                    m_PackageInstaller.CancelInstallation();
                    Reset();
                }
            }
        }

        void CheckStatus()
        {
            try
            {
                GetDirectoryPath();
                CheckRepositoryStatus();
                CheckMethodStatus();
                CheckInstallationStatus();
            }
            catch { }
        }

        void GetDirectoryPath()
        {
            if (Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
            {
                var l_FolderModelOption = FolderModeOptions.GetLiveInstanceAsync().Result;
                m_workingDirectoryPathString = System.IO.Path.Combine(Utilities.GetOpenFolderPath(), l_FolderModelOption.PackageDirectoryName);
            }
            else if (Utilities.GetSolutionMode() == Utilities.SolutionMode.ProjectMode)
            {
                m_workingDirectoryPathString = System.IO.Path.Combine(Utilities.GetCurrentProjectPath(), "package");
            }
            else
            {
                m_workingDirectoryPathString = "";
            }
        }

        void CheckRepositoryStatus()
        {
            var l_repoTopRegex =    new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)$");
            var l_tagRegex =        new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)/tree/(?<tree>[^/]+)$");
            var l_commitRegex =     new Regex(@"^https://github\.com/(?<username>[^/]+)/(?<repo>[^/]+)/commit/(?<commit>[^/]+)$");
            var l_repoTopMatch = l_repoTopRegex.Match(m_URL_TextBlock.Text);
            var l_tagMatch = l_tagRegex.Match(m_URL_TextBlock.Text);
            var l_commitMatch = l_commitRegex.Match(m_URL_TextBlock.Text);
            m_repogitoryBool = l_repoTopMatch.Success || l_tagMatch.Success || l_commitMatch.Success;

            m_zipUrlString = "";
            m_targzUrlString = "";
            string l_tempUrlString = "";
            if (l_repoTopMatch.Success)
            {
                l_tempUrlString = $"https://github.com/{l_repoTopMatch.Groups["username"]}/{l_repoTopMatch.Groups["repo"]}/archive/refs/heads/master";
            } 
            else if(l_tagMatch.Success)
            {
                var l_branchString = l_tagMatch.Groups["tree"].ToString();
                if (Regex.IsMatch(l_branchString, @"^[0-9.]*$"))
                {
                    l_tempUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}/archive/refs/tags/{l_tagMatch.Groups["tree"]}";
                }
                else
                {
                    l_tempUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}/archive/refs/heads/{l_tagMatch.Groups["tree"]}";
                }
            }
            else if(l_commitMatch.Success)
            {
                l_tempUrlString = $"https://github.com/{l_commitMatch.Groups["username"]}/{l_commitMatch.Groups["repo"]}/archive/{l_commitMatch.Groups["commit"]}";
            }
            m_zipUrlString = l_tempUrlString + c_zipExtensionString;
            m_targzUrlString = l_tempUrlString + c_targzExtensionString;


            m_gitUrlString = "";
            if (l_repoTopMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_repoTopMatch.Groups["username"]}/{l_repoTopMatch.Groups["repo"]}.git";
            }
            else if (l_tagMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_tagMatch.Groups["username"]}/{l_tagMatch.Groups["repo"]}.git --branch {l_tagMatch.Groups["tree"]}";
            }
            else if (l_commitMatch.Success)
            {
                m_gitUrlString = $"https://github.com/{l_commitMatch.Groups["username"]}/{l_commitMatch.Groups["repo"]}.git";
            }

            m_One_TextBlock.Foreground = m_repogitoryBool ? m_greenBrush : Text.Foreground;
        }

        void CheckMethodStatus()
        {
            m_methodBool = false;
            string l_displayString = "";

            var l_GeneralOptions = GeneralOptions.GetLiveInstanceAsync().Result;
            var l_FolderModeOptions = FolderModeOptions.GetLiveInstanceAsync().Result;

            if (string.IsNullOrEmpty(m_Method_ComboBox.Text))
            {
            }
            else if (m_Method_ComboBox.Text == c_methodGit)
            {
                m_methodBool = true;
                l_displayString = l_GeneralOptions.GitPath + ' ' + l_GeneralOptions.GitOption + ' ' + m_gitUrlString;
            }
            else if(m_Method_ComboBox.Text== c_methodUnzip)
            {
                m_methodBool = true;
                l_displayString = "Unzip(internal command) " + m_zipUrlString;
            }
            else if (m_Method_ComboBox.Text == c_methodZigFetch && Utilities.GetSolutionMode() == Utilities.SolutionMode.OpenFolderMode)
            {
                m_methodBool = true;
                var l_toolPathString = System.IO.Path.Combine(Utilities.GetToolPathFromEnvironmentValue(), l_FolderModeOptions.ToolPath);
                l_displayString = l_toolPathString + ' ' + l_GeneralOptions.ZigFetchOption + ' ' + m_targzUrlString;
            }
            else if (m_Method_ComboBox.Text == c_methodAdd && Utilities.GetSolutionMode() == Utilities.SolutionMode.ProjectMode)
            {
                m_methodBool = false;
                l_displayString = "Currently, this method is not supported.";
            }

            m_Two_TextBlock.Foreground = m_methodBool ? m_greenBrush : Text.Foreground;
            m_Command_TextBlock.Text = m_workingDirectoryPathString + " > " + l_displayString;
        }

        void CheckInstallationStatus()
        {
            if (Utilities.GetSolutionMode() == Utilities.SolutionMode.None)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Your folder or project file is not open. Not ready to install.";
            }
            else if (!m_repogitoryBool)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Repository is not selected. Not ready to install.";
            }
            else if (!m_methodBool)
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = false;
                m_Warning_TextBlock.Text = "Method is not selected. Not ready to install.";
            }
            else
            {
                m_PackageInstallerWindowViewModel.InstallButtonEnabled = true;
                m_Warning_TextBlock.Text = "";
            }
        }
    }

    public class PackageInstallerWindowViewModel : INotifyPropertyChanged
    {
        private string m_InstallButtonTextString = "";

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
#pragma warning restore VSTHRD002,VSTHRD010
}
