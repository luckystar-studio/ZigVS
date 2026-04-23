namespace ZigVS.CoreCompatibility.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class ZigTestSkeletonGenerator
    {
        public bool TryGenerate(string sourceText, int caretLineNumber, out GenerateTestSkeletonResult? result)
        {
            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }

            if (caretLineNumber <= 0)
            {
                result = CreateFailure("The caret is not inside a Zig function.");
                return false;
            }

            string newline = sourceText.IndexOf("\r\n", StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";
            List<int> lineStarts = GetLineStarts(sourceText);
            string sanitizedText = Sanitize(sourceText);
            List<FunctionDeclarationMatch> functions = ParseFunctions(sourceText, sanitizedText, lineStarts);

            FunctionDeclarationMatch? selectedMatch = null;
            foreach (FunctionDeclarationMatch candidate in functions)
            {
                if (caretLineNumber < candidate.DeclarationStartLineNumber ||
                    caretLineNumber > candidate.BodyEndLineNumber)
                {
                    continue;
                }

                if (selectedMatch == null ||
                    candidate.BodyEndOffset - candidate.DeclarationOffset < selectedMatch.BodyEndOffset - selectedMatch.DeclarationOffset)
                {
                    selectedMatch = candidate;
                }
            }

            if (selectedMatch == null)
            {
                result = CreateFailure("The caret is not inside a Zig function.");
                return false;
            }

            int replaceStartOffset = selectedMatch.BodyEndOffset + 1;
            int replaceLength = ComputeReplaceLength(sourceText, replaceStartOffset);
            bool hasTrailingCode = replaceStartOffset + replaceLength < sourceText.Length;

            string generatedBlock = BuildTestBlock(selectedMatch, newline);
            string generatedText = newline + newline + generatedBlock + newline + (hasTrailingCode ? newline : string.Empty);
            int todoIndex = generatedText.IndexOf("// TODO: add test", StringComparison.Ordinal);

            result = new GenerateTestSkeletonResult
            {
                Success = true,
                GeneratedText = generatedText,
                ReplaceStartOffset = replaceStartOffset,
                ReplaceLength = replaceLength,
                CaretOffset = todoIndex >= 0
                    ? replaceStartOffset + todoIndex
                    : replaceStartOffset,
                Match = selectedMatch
            };

            return true;
        }

        static GenerateTestSkeletonResult CreateFailure(string errorMessage)
        {
            return new GenerateTestSkeletonResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        static string BuildTestBlock(FunctionDeclarationMatch match, string newline)
        {
            StringBuilder builder = new StringBuilder();
            string indent = match.Indentation;
            string innerIndent = indent + "    ";
            string argumentIndent = innerIndent + "    ";

            builder.Append(indent);
            builder.Append("test \"");
            builder.Append(EscapeString(match.FunctionName));
            builder.Append("\" {");
            builder.Append(newline);

            if (match.Parameters.Count == 0)
            {
                builder.Append(innerIndent);
                builder.Append("const actual = ");
                if (match.ReturnsErrorUnion)
                {
                    builder.Append("try ");
                }

                builder.Append(match.InvocationName);
                builder.Append("();");
                builder.Append(newline);
            }
            else
            {
                builder.Append(innerIndent);
                builder.Append("const actual = ");
                if (match.ReturnsErrorUnion)
                {
                    builder.Append("try ");
                }

                builder.Append(match.InvocationName);
                builder.Append("(");
                builder.Append(newline);

                foreach (FunctionParameterMatch parameter in match.Parameters)
                {
                    builder.Append(argumentIndent);
                    builder.Append(GetPlaceholderValue(parameter.TypeText));
                    builder.Append(",");
                    builder.Append(newline);
                }

                builder.Append(innerIndent);
                builder.Append(");");
                builder.Append(newline);
            }

            builder.Append(innerIndent);
            builder.Append("_ = actual;");
            builder.Append(newline);
            builder.Append(innerIndent);
            builder.Append("// TODO: add test");
            builder.Append(newline);
            builder.Append(indent);
            builder.Append("}");

            return builder.ToString();
        }

        static int ComputeReplaceLength(string sourceText, int replaceStartOffset)
        {
            int position = replaceStartOffset;

            if (TryConsumeNewLine(sourceText, ref position))
            {
                while (TryConsumeBlankLine(sourceText, ref position))
                {
                }
            }

            return position - replaceStartOffset;
        }

        static bool TryConsumeBlankLine(string text, ref int position)
        {
            int probe = position;
            while (probe < text.Length && (text[probe] == ' ' || text[probe] == '\t'))
            {
                probe++;
            }

            if (!TryConsumeNewLine(text, ref probe))
            {
                return false;
            }

            position = probe;
            return true;
        }

        static bool TryConsumeNewLine(string text, ref int position)
        {
            if (position >= text.Length)
            {
                return false;
            }

            if (text[position] == '\r')
            {
                position++;
                if (position < text.Length && text[position] == '\n')
                {
                    position++;
                }

                return true;
            }

            if (text[position] == '\n')
            {
                position++;
                return true;
            }

            return false;
        }

        static List<FunctionDeclarationMatch> ParseFunctions(string sourceText, string sanitizedText, List<int> lineStarts)
        {
            List<FunctionDeclarationMatch> functions = new List<FunctionDeclarationMatch>();

            for (int index = 0; index < sanitizedText.Length - 1; index++)
            {
                if (!IsWordAt(sanitizedText, index, "fn"))
                {
                    continue;
                }

                if (!TryParseFunction(sourceText, sanitizedText, lineStarts, index, out FunctionDeclarationMatch? match))
                {
                    continue;
                }

                if (match != null)
                {
                    functions.Add(match);
                    index = match.BodyEndOffset;
                }
            }

            return functions;
        }

        static bool TryParseFunction(
            string sourceText,
            string sanitizedText,
            List<int> lineStarts,
            int fnIndex,
            out FunctionDeclarationMatch? match)
        {
            match = null;

            int nameStart = SkipWhitespace(sanitizedText, fnIndex + 2);
            if (!TryParseIdentifier(sourceText, nameStart, out string invocationName, out string functionName, out int nameEnd))
            {
                return false;
            }

            int openParenIndex = SkipWhitespace(sanitizedText, nameEnd);
            if (openParenIndex >= sanitizedText.Length || sanitizedText[openParenIndex] != '(')
            {
                return false;
            }

            int closeParenIndex = FindMatchingPair(sanitizedText, openParenIndex, '(', ')');
            if (closeParenIndex < 0)
            {
                return false;
            }

            if (!TryFindBodyStartOrSemicolon(sanitizedText, closeParenIndex + 1, out int bodyStartIndex, out bool hasBody))
            {
                return false;
            }

            if (!hasBody)
            {
                return true;
            }

            int bodyEndIndex = FindMatchingPair(sanitizedText, bodyStartIndex, '{', '}');
            if (bodyEndIndex < 0)
            {
                return false;
            }

            int fnLine = GetLineNumberForOffset(lineStarts, fnIndex);
            int declarationStartLine = FindDeclarationStartLine(sourceText, lineStarts, fnLine);
            int declarationOffset = lineStarts[declarationStartLine - 1];
            int declarationEndLine = GetLineNumberForOffset(lineStarts, bodyStartIndex);
            int bodyEndLine = GetLineNumberForOffset(lineStarts, bodyEndIndex);

            match = new FunctionDeclarationMatch
            {
                FunctionName = functionName,
                InvocationName = invocationName,
                DeclarationOffset = declarationOffset,
                DeclarationStartLineNumber = declarationStartLine,
                DeclarationEndLineNumber = declarationEndLine,
                BodyStartOffset = bodyStartIndex,
                BodyEndOffset = bodyEndIndex,
                BodyEndLineNumber = bodyEndLine,
                Indentation = GetIndentationForLine(sourceText, lineStarts[fnLine - 1]),
                ReturnsErrorUnion = ContainsTopLevelErrorUnion(sourceText.Substring(closeParenIndex + 1, bodyStartIndex - closeParenIndex - 1)),
                Parameters = ParseParameters(sourceText.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1))
            };

            return true;
        }

        static bool TryFindBodyStartOrSemicolon(string sanitizedText, int startIndex, out int bodyStartIndex, out bool hasBody)
        {
            bodyStartIndex = -1;
            hasBody = false;

            int index = startIndex;
            while (index < sanitizedText.Length)
            {
                char current = sanitizedText[index];
                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                if (current == ';')
                {
                    return true;
                }

                if (current == '(')
                {
                    index = FindMatchingPair(sanitizedText, index, '(', ')');
                    if (index < 0)
                    {
                        return false;
                    }

                    index++;
                    continue;
                }

                if (current == '[')
                {
                    index = FindMatchingPair(sanitizedText, index, '[', ']');
                    if (index < 0)
                    {
                        return false;
                    }

                    index++;
                    continue;
                }

                if (TryGetKeywordNeedingBraceSkip(sanitizedText, index, out int keywordLength))
                {
                    int braceIndex = SkipWhitespace(sanitizedText, index + keywordLength);
                    if (braceIndex < sanitizedText.Length && sanitizedText[braceIndex] == '{')
                    {
                        int endIndex = FindMatchingPair(sanitizedText, braceIndex, '{', '}');
                        if (endIndex < 0)
                        {
                            return false;
                        }

                        index = endIndex + 1;
                        continue;
                    }
                }

                if (current == '{')
                {
                    bodyStartIndex = index;
                    hasBody = true;
                    return true;
                }

                index++;
            }

            return false;
        }

        static bool TryGetKeywordNeedingBraceSkip(string sanitizedText, int index, out int keywordLength)
        {
            foreach (string keyword in s_BraceScopedKeywords)
            {
                if (IsWordAt(sanitizedText, index, keyword))
                {
                    keywordLength = keyword.Length;
                    return true;
                }
            }

            keywordLength = 0;
            return false;
        }

        static readonly string[] s_BraceScopedKeywords = { "struct", "union", "enum", "opaque", "error" };

        static bool ContainsTopLevelErrorUnion(string returnTypeText)
        {
            int parenDepth = 0;
            int bracketDepth = 0;
            int braceDepth = 0;

            foreach (char current in returnTypeText)
            {
                switch (current)
                {
                    case '(':
                        parenDepth++;
                        break;
                    case ')':
                        parenDepth = Math.Max(0, parenDepth - 1);
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth = Math.Max(0, bracketDepth - 1);
                        break;
                    case '{':
                        braceDepth++;
                        break;
                    case '}':
                        braceDepth = Math.Max(0, braceDepth - 1);
                        break;
                    case '!':
                        if (parenDepth == 0 && bracketDepth == 0 && braceDepth == 0)
                        {
                            return true;
                        }

                        break;
                }
            }

            return false;
        }

        static List<FunctionParameterMatch> ParseParameters(string parameterListText)
        {
            List<FunctionParameterMatch> parameters = new List<FunctionParameterMatch>();
            foreach (string rawParameter in SplitTopLevel(parameterListText, ','))
            {
                string trimmed = rawParameter.Trim();
                if (trimmed.Length == 0 || trimmed == "...")
                {
                    continue;
                }

                int colonIndex = FindTopLevel(trimmed, ':');
                string typeText = colonIndex >= 0
                    ? trimmed.Substring(colonIndex + 1).Trim()
                    : trimmed;

                parameters.Add(new FunctionParameterMatch
                {
                    RawText = trimmed,
                    TypeText = typeText
                });
            }

            return parameters;
        }

        static int FindTopLevel(string text, char target)
        {
            int parenDepth = 0;
            int bracketDepth = 0;
            int braceDepth = 0;
            bool inString = false;
            bool inChar = false;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];

                if (inString)
                {
                    if (current == '\\' && index + 1 < text.Length)
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (inChar)
                {
                    if (current == '\\' && index + 1 < text.Length)
                    {
                        index++;
                    }
                    else if (current == '\'')
                    {
                        inChar = false;
                    }

                    continue;
                }

                switch (current)
                {
                    case '"':
                        inString = true;
                        break;
                    case '\'':
                        inChar = true;
                        break;
                    case '(':
                        parenDepth++;
                        break;
                    case ')':
                        parenDepth = Math.Max(0, parenDepth - 1);
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth = Math.Max(0, bracketDepth - 1);
                        break;
                    case '{':
                        braceDepth++;
                        break;
                    case '}':
                        braceDepth = Math.Max(0, braceDepth - 1);
                        break;
                    default:
                        if (current == target &&
                            parenDepth == 0 &&
                            bracketDepth == 0 &&
                            braceDepth == 0)
                        {
                            return index;
                        }

                        break;
                }
            }

            return -1;
        }

        static List<string> SplitTopLevel(string text, char separator)
        {
            List<string> parts = new List<string>();
            int start = 0;
            int parenDepth = 0;
            int bracketDepth = 0;
            int braceDepth = 0;
            bool inString = false;
            bool inChar = false;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];

                if (inString)
                {
                    if (current == '\\' && index + 1 < text.Length)
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (inChar)
                {
                    if (current == '\\' && index + 1 < text.Length)
                    {
                        index++;
                    }
                    else if (current == '\'')
                    {
                        inChar = false;
                    }

                    continue;
                }

                switch (current)
                {
                    case '"':
                        inString = true;
                        break;
                    case '\'':
                        inChar = true;
                        break;
                    case '(':
                        parenDepth++;
                        break;
                    case ')':
                        parenDepth = Math.Max(0, parenDepth - 1);
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth = Math.Max(0, bracketDepth - 1);
                        break;
                    case '{':
                        braceDepth++;
                        break;
                    case '}':
                        braceDepth = Math.Max(0, braceDepth - 1);
                        break;
                    default:
                        if (current == separator &&
                            parenDepth == 0 &&
                            bracketDepth == 0 &&
                            braceDepth == 0)
                        {
                            parts.Add(text.Substring(start, index - start));
                            start = index + 1;
                        }

                        break;
                }
            }

            parts.Add(text.Substring(start));
            return parts;
        }

        static string GetPlaceholderValue(string typeText)
        {
            string normalized = NormalizeType(typeText);
            if (normalized.Length == 0)
            {
                return "undefined";
            }

            if (normalized.StartsWith("?", StringComparison.Ordinal))
            {
                return "null";
            }

            switch (normalized)
            {
                case "bool":
                    return "false";
                case "type":
                    return "u8";
                case "usize":
                case "isize":
                case "comptime_int":
                case "comptime_float":
                case "f16":
                case "f32":
                case "f64":
                case "f80":
                case "f128":
                    return "0";
                case "anytype":
                    return "undefined";
            }

            if (IsIntegerType(normalized))
            {
                return "0";
            }

            return "undefined";
        }

        static string NormalizeType(string typeText)
        {
            string normalized = (typeText ?? string.Empty).Trim();
            if (normalized.StartsWith("comptime ", StringComparison.Ordinal))
            {
                normalized = normalized.Substring("comptime ".Length).Trim();
            }

            if (normalized.StartsWith("noalias ", StringComparison.Ordinal))
            {
                normalized = normalized.Substring("noalias ".Length).Trim();
            }

            return normalized;
        }

        static bool IsIntegerType(string typeText)
        {
            if (typeText.Length < 2)
            {
                return false;
            }

            char prefix = typeText[0];
            if (prefix != 'i' && prefix != 'u')
            {
                return false;
            }

            for (int index = 1; index < typeText.Length; index++)
            {
                if (!char.IsDigit(typeText[index]))
                {
                    return false;
                }
            }

            return true;
        }

        static string EscapeString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        static int FindDeclarationStartLine(string sourceText, List<int> lineStarts, int fnLine)
        {
            int line = fnLine;
            while (line > 1)
            {
                string previousLine = GetLineText(sourceText, lineStarts, line - 1).Trim();
                if (!LooksLikeModifierLine(previousLine))
                {
                    break;
                }

                line--;
            }

            return line;
        }

        static bool LooksLikeModifierLine(string trimmedLine)
        {
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                return false;
            }

            if (trimmedLine.StartsWith("//", StringComparison.Ordinal) ||
                trimmedLine.StartsWith("/*", StringComparison.Ordinal) ||
                trimmedLine.IndexOf('=') >= 0 ||
                trimmedLine.IndexOf('{') >= 0 ||
                trimmedLine.IndexOf('}') >= 0 ||
                trimmedLine.IndexOf(';') >= 0)
            {
                return false;
            }

            string[] tokens = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                return false;
            }

            foreach (string token in tokens)
            {
                if (token == "pub" ||
                    token == "inline" ||
                    token == "noinline" ||
                    token == "extern" ||
                    token == "export" ||
                    token == "comptime")
                {
                    continue;
                }

                if (token.StartsWith("callconv", StringComparison.Ordinal) ||
                    token.StartsWith("linksection", StringComparison.Ordinal) ||
                    token.StartsWith("addrspace", StringComparison.Ordinal) ||
                    token.StartsWith("align", StringComparison.Ordinal))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        static string GetLineText(string sourceText, List<int> lineStarts, int lineNumber)
        {
            int start = lineStarts[lineNumber - 1];
            int end = lineNumber < lineStarts.Count
                ? lineStarts[lineNumber] - 1
                : sourceText.Length;
            return sourceText.Substring(start, end - start);
        }

        static string GetIndentationForLine(string sourceText, int lineStartOffset)
        {
            int index = lineStartOffset;
            while (index < sourceText.Length &&
                   (sourceText[index] == ' ' || sourceText[index] == '\t'))
            {
                index++;
            }

            return sourceText.Substring(lineStartOffset, index - lineStartOffset);
        }

        static int SkipWhitespace(string text, int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return index;
        }

        static int FindMatchingPair(string text, int openIndex, char openChar, char closeChar)
        {
            int depth = 0;
            for (int index = openIndex; index < text.Length; index++)
            {
                if (text[index] == openChar)
                {
                    depth++;
                }
                else if (text[index] == closeChar)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        static bool TryParseIdentifier(string sourceText, int startIndex, out string invocationName, out string functionName, out int endIndex)
        {
            invocationName = string.Empty;
            functionName = string.Empty;
            endIndex = startIndex;

            if (startIndex >= sourceText.Length)
            {
                return false;
            }

            if (sourceText[startIndex] == '@' &&
                startIndex + 1 < sourceText.Length &&
                sourceText[startIndex + 1] == '"')
            {
                int index = startIndex + 2;
                while (index < sourceText.Length)
                {
                    if (sourceText[index] == '\\' && index + 1 < sourceText.Length)
                    {
                        index += 2;
                        continue;
                    }

                    if (sourceText[index] == '"')
                    {
                        invocationName = sourceText.Substring(startIndex, index - startIndex + 1);
                        functionName = sourceText.Substring(startIndex + 2, index - startIndex - 2);
                        endIndex = index + 1;
                        return true;
                    }

                    index++;
                }

                return false;
            }

            if (!IsIdentifierStart(sourceText[startIndex]))
            {
                return false;
            }

            int end = startIndex + 1;
            while (end < sourceText.Length && IsIdentifierPart(sourceText[end]))
            {
                end++;
            }

            invocationName = sourceText.Substring(startIndex, end - startIndex);
            functionName = invocationName;
            endIndex = end;
            return true;
        }

        static bool IsIdentifierStart(char value)
        {
            return value == '_' || char.IsLetter(value);
        }

        static bool IsIdentifierPart(char value)
        {
            return value == '_' || char.IsLetterOrDigit(value);
        }

        static bool IsWordAt(string text, int index, string word)
        {
            if (index < 0 || index + word.Length > text.Length)
            {
                return false;
            }

            if (!string.Equals(text.Substring(index, word.Length), word, StringComparison.Ordinal))
            {
                return false;
            }

            bool hasPrevious = index > 0 && IsIdentifierPart(text[index - 1]);
            bool hasNext = index + word.Length < text.Length && IsIdentifierPart(text[index + word.Length]);
            return !hasPrevious && !hasNext;
        }

        static int GetLineNumberForOffset(List<int> lineStarts, int offset)
        {
            int low = 0;
            int high = lineStarts.Count - 1;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                int lineStart = lineStarts[mid];
                int nextLineStart = mid + 1 < lineStarts.Count ? lineStarts[mid + 1] : int.MaxValue;

                if (offset < lineStart)
                {
                    high = mid - 1;
                }
                else if (offset >= nextLineStart)
                {
                    low = mid + 1;
                }
                else
                {
                    return mid + 1;
                }
            }

            return lineStarts.Count;
        }

        static List<int> GetLineStarts(string sourceText)
        {
            List<int> lineStarts = new List<int> { 0 };
            for (int index = 0; index < sourceText.Length; index++)
            {
                if (sourceText[index] == '\n' && index + 1 < sourceText.Length)
                {
                    lineStarts.Add(index + 1);
                }
            }

            return lineStarts;
        }

        static string Sanitize(string sourceText)
        {
            StringBuilder sanitized = new StringBuilder(sourceText.Length);
            State state = State.Normal;

            for (int index = 0; index < sourceText.Length; index++)
            {
                char current = sourceText[index];
                switch (state)
                {
                    case State.Normal:
                        if (current == '/' &&
                            index + 1 < sourceText.Length &&
                            sourceText[index + 1] == '/')
                        {
                            sanitized.Append(' ');
                            sanitized.Append(' ');
                            index++;
                            state = State.LineComment;
                            continue;
                        }

                        if (current == '/' &&
                            index + 1 < sourceText.Length &&
                            sourceText[index + 1] == '*')
                        {
                            sanitized.Append(' ');
                            sanitized.Append(' ');
                            index++;
                            state = State.BlockComment;
                            continue;
                        }

                        if (current == '"')
                        {
                            sanitized.Append(' ');
                            state = State.String;
                            continue;
                        }

                        if (current == '\'')
                        {
                            sanitized.Append(' ');
                            state = State.Char;
                            continue;
                        }

                        sanitized.Append(current);
                        break;

                    case State.LineComment:
                        if (current == '\n')
                        {
                            sanitized.Append('\n');
                            state = State.Normal;
                        }
                        else
                        {
                            sanitized.Append(' ');
                        }

                        break;

                    case State.BlockComment:
                        if (current == '*' &&
                            index + 1 < sourceText.Length &&
                            sourceText[index + 1] == '/')
                        {
                            sanitized.Append(' ');
                            sanitized.Append(' ');
                            index++;
                            state = State.Normal;
                        }
                        else if (current == '\n')
                        {
                            sanitized.Append('\n');
                        }
                        else
                        {
                            sanitized.Append(' ');
                        }

                        break;

                    case State.String:
                        if (current == '\\' && index + 1 < sourceText.Length)
                        {
                            sanitized.Append(' ');
                            sanitized.Append(' ');
                            index++;
                        }
                        else if (current == '"')
                        {
                            sanitized.Append(' ');
                            state = State.Normal;
                        }
                        else if (current == '\n')
                        {
                            sanitized.Append('\n');
                        }
                        else
                        {
                            sanitized.Append(' ');
                        }

                        break;

                    case State.Char:
                        if (current == '\\' && index + 1 < sourceText.Length)
                        {
                            sanitized.Append(' ');
                            sanitized.Append(' ');
                            index++;
                        }
                        else if (current == '\'')
                        {
                            sanitized.Append(' ');
                            state = State.Normal;
                        }
                        else if (current == '\n')
                        {
                            sanitized.Append('\n');
                        }
                        else
                        {
                            sanitized.Append(' ');
                        }

                        break;
                }
            }

            return sanitized.ToString();
        }

        enum State
        {
            Normal = 0,
            LineComment = 1,
            BlockComment = 2,
            String = 3,
            Char = 4
        }
    }

    internal sealed class GenerateTestSkeletonResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string GeneratedText { get; set; } = string.Empty;
        public int ReplaceStartOffset { get; set; }
        public int ReplaceLength { get; set; }
        public int CaretOffset { get; set; }
        public FunctionDeclarationMatch? Match { get; set; }
    }

    internal sealed class FunctionDeclarationMatch
    {
        public string FunctionName { get; set; } = string.Empty;
        public string InvocationName { get; set; } = string.Empty;
        public int DeclarationOffset { get; set; }
        public int DeclarationStartLineNumber { get; set; }
        public int DeclarationEndLineNumber { get; set; }
        public int BodyStartOffset { get; set; }
        public int BodyEndOffset { get; set; }
        public int BodyEndLineNumber { get; set; }
        public string Indentation { get; set; } = string.Empty;
        public bool ReturnsErrorUnion { get; set; }
        public IReadOnlyList<FunctionParameterMatch> Parameters { get; set; } = Array.Empty<FunctionParameterMatch>();
    }

    internal sealed class FunctionParameterMatch
    {
        public string RawText { get; set; } = string.Empty;
        public string TypeText { get; set; } = string.Empty;
    }
}
