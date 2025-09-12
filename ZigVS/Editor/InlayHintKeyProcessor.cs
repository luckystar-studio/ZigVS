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