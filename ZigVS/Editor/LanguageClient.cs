/********************************************************************************************
Copyright(c) 2023 LuckyStar Studio LLC
All rights reserved.

Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you
accept this license. If you do not accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free copyright license to reproduce its contribution, prepare derivative works of
its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions
and limitations in section 3, each contributor grants you a non-exclusive, worldwide,
royalty-free license under its licensed patents to make, have made, use, sell, offer for
sale, import, and/or otherwise dispose of its contribution in the software or derivative
works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors'
name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are
infringed by the software, your patent license from such contributor to the software ends
automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent,
trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only
under this license by including a complete copy of this license with your distribution.
If you distribute any portion of the software in compiled or object code form, you may only
do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it.The contributors give
no express warranties, guarantees, or conditions. You may have additional consumer rights
under your local laws which this license cannot change. To the extent permitted under your
local laws, the contributors exclude the implied warranties of merchantability, fitness for
a particular purpose and non-infringement.

********************************************************************************************/

namespace ZigVS
{
#nullable enable
    using Microsoft.Build.Framework.XamlTypes;
    using Microsoft.VisualStudio.LanguageServer.Client;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;
    using Microsoft.VisualStudio.Utilities;
    using StreamJsonRpc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LSP = Microsoft.VisualStudio.LanguageServer.Protocol;

    // Read https://learn.microsoft.com/en-us/visualstudio/extensibility/workspace-language-services?view=vs-2022

    [ContentType(Parameter.c_languageName)]
    [Export(typeof(ILanguageClient))]
    [RunOnContext(RunningContext.RunOnHost)]
    public class LanguageClient : ILanguageClient, ILanguageClientCustomMessage2
    {
        static LanguageClient? s_Instance;

        public string Name => Parameter.c_languageName + " language extension";
        protected JsonRpc? m_JsonRpc = null;

        public IEnumerable<string>? ConfigurationSections => null;
        public object? InitializationOptions => null;

        public IEnumerable<string>? FilesToWatch => null;
        public bool ShowNotificationOnInitializeFailed => true;

        public LanguageClient()
        {
            s_Instance = this!;
        }

        public async Task<Connection?> ActivateAsync(CancellationToken i_CancellationToken)
        {
            Connection? r_Connection = null;
            try
            {
                await Task.Yield();

                var l_GeneralOptions = await GeneralOptions.GetLiveInstanceAsync();
                if (l_GeneralOptions != null)
                {
                    var l_toolPathString = Utilities.GetToolPathFromEnvironmentValue();

                    ProcessStartInfo l_ProcessStartInfo = new ProcessStartInfo();
                    l_ProcessStartInfo.FileName = Path.Combine(l_toolPathString, l_GeneralOptions.LanguageServerPath);
                    l_ProcessStartInfo.Arguments =
                        (l_GeneralOptions.TDebugSwitch == Switch.off) ? l_GeneralOptions.Arguments : l_GeneralOptions.DebugArguments;
                    l_ProcessStartInfo.RedirectStandardInput = true;
                    l_ProcessStartInfo.RedirectStandardOutput = true;
                    l_ProcessStartInfo.UseShellExecute = false;
                    l_ProcessStartInfo.CreateNoWindow = (l_GeneralOptions.TDebugSwitch == Switch.off);          // false -> shows zls process windows

                    Process l_Process = new Process();
                    l_Process.StartInfo = l_ProcessStartInfo;

                    if (l_Process.Start())
                    {
                        var l_Connection = new Connection(l_Process.StandardOutput.BaseStream, l_Process.StandardInput.BaseStream);
                        r_Connection = await Task.FromResult(l_Connection);
                    }
                }
            }
            catch (Exception l_Exception)
            {
                Common.OutputWindowPane.OutputString("Failed to start the language server. Please check PATH to Zig Tool Chain." + Environment.NewLine + l_Exception.Message);
            }

            return r_Connection;
        }

        public Task AttachForCustomMessageAsync(JsonRpc i_JsonRpc)
        {
            m_JsonRpc = i_JsonRpc;
            return Task.CompletedTask;
        }

        public static void ToggleInlyHints()
        {
            ZLS.ToggleInlyHint();
            s_Instance?.SendConfiguration();
        }

        protected void SendConfiguration()
        {
#pragma warning disable VSTHRD110
            NotifyDidChangeConfigurationAsync(ZLS.GetSettings());
#pragma warning restore VSTHRD110
        }
#pragma warning disable CS8603, VSTHRD114
        protected Task NotifyDidChangeConfigurationAsync(object i_Settings)
        {
            var l_methodString = "workspace/didChangeConfiguration";

            var l_DidChangeConfigurationParams = new LSP.DidChangeConfigurationParams();
            l_DidChangeConfigurationParams.Settings = i_Settings;

            if (m_JsonRpc != null)
            {
                return m_JsonRpc.NotifyWithParameterObjectAsync(l_methodString, l_DidChangeConfigurationParams);
            }
            return null;
        }
#pragma warning restore CS8603                                                                                                                             
        public Task OnServerInitializedAsync()
        {
            SendConfiguration();
            return Task.CompletedTask;
        }

        public Task OnServerInitializeFailedAsync(Exception e)
        {
            return Task.CompletedTask;
        }

        public Task<InitializationFailureContext?> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo i_ILanguageClientInitializationInfo)
        {
            return new Task<InitializationFailureContext?>(() => { return null; });
        }

        public event AsyncEventHandler<EventArgs>? StartAsync;
        public event AsyncEventHandler<EventArgs>? StopAsync;

        public async Task OnLoadedAsync()
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty).ConfigureAwait(false);
        }

        public async Task OnStopedAsync()
        {
            await StopAsync.InvokeAsync(this, EventArgs.Empty);
        }

        // For Custom Message 
#pragma warning disable CS8603
        public object MiddleLayer => null; //LanguageClientMiddleLayer.Instance;
        public object? CustomMessageTarget => null;//throw new NotImplementedException();
#pragma warning restore CS8603
    }
}