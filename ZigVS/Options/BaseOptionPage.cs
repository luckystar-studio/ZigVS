namespace ZigVS
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.ComponentModel;
    using ZigVS.Options;

    /// <summary>
    /// A base class for a DialogPage to show in Tools -> Options.
    /// </summary>
    internal class BaseOptionPage<T> : DialogPage where T : BaseOptionModel<T>, new()
    {
        private BaseOptionModel<T> _model;

        GeneralOptions captureOnActivate = new GeneralOptions();

        public BaseOptionPage()
        {
#pragma warning disable VSTHRD104 // Offer async methods
            _model = ThreadHelper.JoinableTaskFactory.Run(BaseOptionModel<T>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
        }

        public override object AutomationObject => _model;

        public override void LoadSettingsFromStorage()
        {
            _model.Load();
        }

        public override void SaveSettingsToStorage()
        {
            _model.Save();
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);

            if (_model is BaseOptionModel<GeneralOptions> _goModel)
            {
                GeneralOptions l_generalOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            return await GeneralOptions.GetLiveInstanceAsync();
                        });

                captureOnActivate.ToolPath = l_generalOptions.ToolPath;
                captureOnActivate.LanguageServerPath = l_generalOptions.LanguageServerPath;
                captureOnActivate.Arguments = l_generalOptions.Arguments;
                captureOnActivate.DebugArguments = l_generalOptions.DebugArguments;
                captureOnActivate.TDebugSwitch = l_generalOptions.TDebugSwitch;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            if (_model is BaseOptionModel<GeneralOptions> _goModel)
            {
                GeneralOptions l_generalOptions = ThreadHelper.JoinableTaskFactory.Run(async () => {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            return await GeneralOptions.GetLiveInstanceAsync();
                        });

                bool configDirty = false;
                bool instanceDirty = false;

                if (!StringComparer.Ordinal.Equals(captureOnActivate.ToolPath, l_generalOptions.ToolPath))
                {
                    configDirty = true;
                }
                if (!StringComparer.Ordinal.Equals(captureOnActivate.LanguageServerPath, l_generalOptions.LanguageServerPath))
                {
                    instanceDirty = true;
                }
                if (!StringComparer.Ordinal.Equals(captureOnActivate.Arguments, l_generalOptions.Arguments))
                {
                    instanceDirty = true;
                }
                if (!StringComparer.Ordinal.Equals(captureOnActivate.DebugArguments, l_generalOptions.DebugArguments))
                {
                    instanceDirty = true;
                }
                if (captureOnActivate.TDebugSwitch != l_generalOptions.TDebugSwitch)
                {
                    instanceDirty = true;
                }

                if (instanceDirty == true)
                {
                    LanguageClient.RestartServer();
                }
                else if (configDirty == true)
                {
                    LanguageClient.UpdateConfiguration();
                }
            }
            if (_model is BaseOptionModel<TextEditorAdvancedOptions>)
            {
                LanguageClient.UpdateConfiguration();
            }
        }
    }
}
