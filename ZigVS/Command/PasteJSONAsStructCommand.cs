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

namespace ZigVS.Command
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Windows.Forms;

#nullable enable
    internal sealed class PasteJSONAsStructCommand
    {
        private static PasteJSONAsStructCommand? s_Instance;

        private PasteJSONAsStructCommand(OleMenuCommandService? i_OleMenuCommandService)
        {
            if (i_OleMenuCommandService != null)
            {
                var l_CommandId = new CommandID(CommandDefinition.s_CommandSetGuid, (int)CommandDefinition.CommandId.PasteJSONAsStruct);
                var l_MenuCommand = new MenuCommand(this.Execute, l_CommandId);
                i_OleMenuCommandService.AddCommand(l_MenuCommand);
            }
        }

        public static System.Threading.Tasks.Task InitializeAsync()
        {
            var l_CommandService = ZigVSPackage.GetInstance().GetService<IMenuCommandService, OleMenuCommandService>();
            s_Instance = new PasteJSONAsStructCommand(l_CommandService);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string? l_ClipboardText;
            try
            {
                l_ClipboardText = Clipboard.ContainsText() ? Clipboard.GetText() : null;
            }
            catch (Exception l_Exception)
            {
                VsShellUtilities.ShowMessageBox(
                    ZigVSPackage.GetInstance(),
                    string.Format(CultureInfo.CurrentCulture, "Failed to access clipboard: {0}", l_Exception.Message),
                    "Paste Zig Struct",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            if (string.IsNullOrWhiteSpace(l_ClipboardText))
            {
                VsShellUtilities.ShowMessageBox(
                    ZigVSPackage.GetInstance(),
                    "Clipboard does not contain text to convert.",
                    "Paste Zig Struct",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            if (!ZigStructGenerator.TryGenerate(l_ClipboardText, out var l_ResultText, out var l_ErrorMessage))
            {
                VsShellUtilities.ShowMessageBox(
                    ZigVSPackage.GetInstance(),
                    l_ErrorMessage,
                    "Paste Zig Struct",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            var l_DTE = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            if (l_DTE?.ActiveDocument?.Selection is TextSelection l_Selection)
            {
                l_Selection.Insert(l_ResultText, (int)vsInsertFlags.vsInsertFlagsContainNewText);
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                    ZigVSPackage.GetInstance(),
                    "An active document is required to paste the generated struct.",
                    "Paste Zig Struct",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static class ZigStructGenerator
        {
            private abstract class ZigType
            {
                public abstract bool RequiresStd { get; }
                public abstract void Render(StringBuilder i_StringBuilder, int i_IndentLevel);
            }

            private sealed class ZigStringType : ZigType
            {
                public override bool RequiresStd => false;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("[]const u8");
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigStringType;
                }

                public override int GetHashCode()
                {
                    return typeof(ZigStringType).GetHashCode();
                }
            }

            private sealed class ZigBoolType : ZigType
            {
                public override bool RequiresStd => false;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("bool");
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigBoolType;
                }

                public override int GetHashCode()
                {
                    return typeof(ZigBoolType).GetHashCode();
                }
            }

            private sealed class ZigIntType : ZigType
            {
                public override bool RequiresStd => false;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("i64");
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigIntType;
                }

                public override int GetHashCode()
                {
                    return typeof(ZigIntType).GetHashCode();
                }
            }

            private sealed class ZigFloatType : ZigType
            {
                public override bool RequiresStd => false;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("f64");
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigFloatType;
                }

                public override int GetHashCode()
                {
                    return typeof(ZigFloatType).GetHashCode();
                }
            }

            private sealed class ZigJsonValueType : ZigType
            {
                public static ZigJsonValueType Instance { get; } = new ZigJsonValueType();

                private ZigJsonValueType() { }

                public override bool RequiresStd => true;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("std.json.Value");
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigJsonValueType;
                }

                public override int GetHashCode()
                {
                    return typeof(ZigJsonValueType).GetHashCode();
                }
            }

            private sealed class ZigOptionalType : ZigType
            {
                public ZigOptionalType(ZigType i_Inner)
                {
                    Inner = i_Inner;
                }

                public ZigType Inner { get; }

                public override bool RequiresStd => Inner.RequiresStd;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append('?');
                    Inner.Render(i_StringBuilder, i_IndentLevel);
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigOptionalType l_Other && l_Other.Inner.Equals(Inner);
                }

                public override int GetHashCode()
                {
                    return Inner.GetHashCode() * 397 ^ 17;
                }
            }

            private sealed class ZigArrayType : ZigType
            {
                public ZigArrayType(ZigType i_ElementType)
                {
                    ElementType = i_ElementType;
                }

                public ZigType ElementType { get; }

                public override bool RequiresStd => ElementType.RequiresStd;

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    i_StringBuilder.Append("[]const ");
                    ElementType.Render(i_StringBuilder, i_IndentLevel);
                }

                public override bool Equals(object? obj)
                {
                    return obj is ZigArrayType l_Other && l_Other.ElementType.Equals(ElementType);
                }

                public override int GetHashCode()
                {
                    return ElementType.GetHashCode() * 397 ^ 23;
                }
            }

            private sealed class ZigStructType : ZigType
            {
                public ZigStructType(System.Collections.Generic.IReadOnlyList<Field> i_Fields)
                {
                    Fields = i_Fields;
                }

                public System.Collections.Generic.IReadOnlyList<Field> Fields { get; }

                public override bool RequiresStd => Fields.Any(i_Field => i_Field.Type.RequiresStd);

                public override void Render(StringBuilder i_StringBuilder, int i_IndentLevel)
                {
                    if (Fields.Count == 0)
                    {
                        i_StringBuilder.Append("struct {}");
                        return;
                    }

                    i_StringBuilder.Append("struct {\n");
                    foreach (var l_Field in Fields)
                    {
                        AppendIndent(i_StringBuilder, i_IndentLevel + 1);
                        i_StringBuilder.Append(FormatFieldName(l_Field.Name));
                        i_StringBuilder.Append(": ");
                        l_Field.Type.Render(i_StringBuilder, i_IndentLevel + 1);
                        i_StringBuilder.Append(",\n");
                    }

                    AppendIndent(i_StringBuilder, i_IndentLevel);
                    i_StringBuilder.Append('}');
                }

                public override bool Equals(object? obj)
                {
                    if (!(obj is ZigStructType l_Other && l_Other.Fields.Count == Fields.Count))
                    {
                        return false;
                    }

                    for (int i = 0; i < Fields.Count; i++)
                    {
                        if (!string.Equals(Fields[i].Name, l_Other.Fields[i].Name, StringComparison.Ordinal) || !Fields[i].Type.Equals(l_Other.Fields[i].Type))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int l_Hash = 17;
                        foreach (var l_Field in Fields)
                        {
                            l_Hash = (l_Hash * 23) + l_Field.Name.GetHashCode(/*StringComparison.Ordinal*/); // ToDo
                            l_Hash = (l_Hash * 23) + l_Field.Type.GetHashCode();
                        }

                        return l_Hash;
                    }
                }
            }

            private sealed class Field
            {
                public Field(string i_Name, ZigType i_Type)
                {
                    Name = i_Name;
                    Type = i_Type;
                }

                public string Name { get; }

                public ZigType Type { get; }
            }

            public static bool TryGenerate(string i_JsonText, out string o_ResultText, out string o_ErrorMessage)
            {
                o_ResultText = string.Empty;
                o_ErrorMessage = string.Empty;

                JsonDocument? l_Document = null;
                try
                {
                    l_Document = JsonDocument.Parse(i_JsonText);
                }
                catch (JsonException l_JsonException)
                {
                    o_ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Clipboard text is not valid JSON: {0}", l_JsonException.Message);
                    return false;
                }
                catch (Exception l_Exception)
                {
                    o_ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Failed to parse JSON: {0}", l_Exception.Message);
                    return false;
                }

                using (l_Document)
                {
                    ZigType l_RootType = CreateType(l_Document.RootElement);

                    var l_StringBuilder = new StringBuilder();
                    if (l_RootType.RequiresStd)
                    {
                        l_StringBuilder.AppendLine("const std = @import(\"std\");");
                    }

                    l_StringBuilder.Append("const Root = ");
                    l_RootType.Render(l_StringBuilder, 0);
                    l_StringBuilder.Append(';');
                    l_StringBuilder.Append(Environment.NewLine);
                    l_StringBuilder.AppendLine("// Example deserialization using std.json:");
                    l_StringBuilder.AppendLine("// const std = @import(\"std\");");
                    l_StringBuilder.AppendLine("// const allocator = std.heap.page_allocator;");
                    l_StringBuilder.AppendLine("// var parsed = try std.json.parseFromSlice(Root, allocator, jsonText, .{});");
                    l_StringBuilder.AppendLine("// defer parsed.deinit();");
                    l_StringBuilder.AppendLine("// const root = parsed.value;");

                    o_ResultText = l_StringBuilder.ToString();
                    return true;
                }
            }

            private static ZigType CreateType(JsonElement i_Element)
            {
                switch (i_Element.ValueKind)
                {
                    case JsonValueKind.Object:
                        var l_Fields = i_Element.EnumerateObject()
                            .Select(i_Property => new Field(i_Property.Name, CreateType(i_Property.Value)))
                            .ToList();
                        return new ZigStructType(l_Fields);
                    case JsonValueKind.Array:
                        ZigType? l_ElementType = null;
                        foreach (var l_Item in i_Element.EnumerateArray())
                        {
                            var l_NextType = CreateType(l_Item);
                            if (l_ElementType == null)
                            {
                                l_ElementType = l_NextType;
                            }
                            else if (!l_ElementType.Equals(l_NextType))
                            {
                                l_ElementType = ZigJsonValueType.Instance;
                                break;
                            }
                        }

                        l_ElementType ??= ZigJsonValueType.Instance;
                        return new ZigArrayType(l_ElementType);
                    case JsonValueKind.String:
                        return new ZigStringType();
                    case JsonValueKind.Number:
                        if (i_Element.TryGetInt64(out _))
                        {
                            return new ZigIntType();
                        }

                        return new ZigFloatType();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return new ZigBoolType();
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        return new ZigOptionalType(ZigJsonValueType.Instance);
                    default:
                        return ZigJsonValueType.Instance;
                }
            }

            private static void AppendIndent(StringBuilder i_StringBuilder, int i_Level)
            {
                if (i_Level <= 0)
                {
                    return;
                }

                i_StringBuilder.Append(' ', i_Level * 4);
            }

            private static string FormatFieldName(string i_Name)
            {
                if (string.IsNullOrEmpty(i_Name) || !IsValidIdentifier(i_Name))
                {
                    return "@\"" + EscapeString(i_Name ?? string.Empty) + "\"";
                }

                return i_Name;
            }

            private static bool IsValidIdentifier(string i_Value)
            {
                if (string.IsNullOrEmpty(i_Value))
                {
                    return false;
                }

                if (!(char.IsLetter(i_Value[0]) || i_Value[0] == '_'))
                {
                    return false;
                }

                for (int i = 1; i < i_Value.Length; i++)
                {
                    char l_Character = i_Value[i];
                    if (!(char.IsLetterOrDigit(l_Character) || l_Character == '_'))
                    {
                        return false;
                    }
                }

                return true;
            }

            private static string EscapeString(string i_Value)
            {
                return i_Value.Replace("\\", "\\\\"/*, StringComparison.Ordinal*/).Replace("\"", "\\\""/*, StringComparison.Ordinal*/);
            }
        }
    }
}