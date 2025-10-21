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

using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ZigVS.Editor
{
    [Export(typeof(IKeyProcessorProvider))]
    [Name("ZigVS AltF1 InlayHints Refresh KeyProcessor")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [ContentType("zig")]
    public sealed class AltF1InlayHintsRefreshKeyProcessorProvider : IKeyProcessorProvider
    {
        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
            => new AltF1InlayHintsRefreshKeyProcessor(wpfTextView);
    }

    internal sealed class AltF1InlayHintsRefreshKeyProcessor : KeyProcessor
    {
        private readonly IWpfTextView _view;

        public AltF1InlayHintsRefreshKeyProcessor(IWpfTextView view)
        {
            _view = view;
        }

        public override void KeyDown(KeyEventArgs args)
        {
            if (IsAltF1(args))
            {
                ForceRelayout(_view);
            }
            base.KeyDown(args);
        }

        public override void KeyUp(KeyEventArgs args)
        {
            if (IsAltF1(args))
            {
                ForceRelayout(_view);
            }
            base.KeyUp(args);
        }

        private static bool IsAltF1(KeyEventArgs e)
            => (e.Key == Key.F1 || e.SystemKey == Key.F1) &&
               (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));

        /// <summary>
        /// Force a full layout/redraw without touching the buffer.
        /// The tiny zoom nudge is visually imperceptible but triggers a layout pass.
        /// </summary>
        private static void ForceRelayout(IWpfTextView view)
        {
            if (view.IsClosed) return;
            var old = view.ZoomLevel;
            // Use a minimal delta that won’t visibly flicker.
            var delta = 0.1; // percentage points
            view.ZoomLevel = old + delta;
            view.ZoomLevel = old;
        }
    }
}