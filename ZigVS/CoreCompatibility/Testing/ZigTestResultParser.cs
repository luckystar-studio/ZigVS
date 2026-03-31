namespace ZigVS.CoreCompatibility.Testing
{
    using System;
    using System.Text.RegularExpressions;

    internal sealed class ZigTestResultParser : ITestResultParser
    {
        static readonly Regex s_ResultRegex = new Regex(@"(?<passed>\d+)\s+passed;\s+(?<skipped>\d+)\s+skipped;\s+(?<failed>\d+)\s+failed", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ZigTestParseResult Parse(ZigTestProcessOutput output)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            string combinedOutput = CombineOutput(output.StandardError, output.StandardOutput);
            string normalizedOutput = combinedOutput.Trim();

            ZigTestParseResult result = new ZigTestParseResult
            {
                Output = normalizedOutput
            };

            string lowerOutput = normalizedOutput.ToLowerInvariant();
            if (lowerOutput.Contains("tests passed"))
            {
                result.Outcome = lowerOutput.Contains("all 0 tests passed")
                    ? ZigTestOutcome.NotFound
                    : ZigTestOutcome.Passed;
                return result;
            }

            Match match = s_ResultRegex.Match(normalizedOutput);
            if (match.Success)
            {
                int passed = int.Parse(match.Groups["passed"].Value);
                int skipped = int.Parse(match.Groups["skipped"].Value);
                int failed = int.Parse(match.Groups["failed"].Value);

                if (failed > 0)
                {
                    result.Outcome = ZigTestOutcome.Failed;
                }
                else if (skipped > 0)
                {
                    result.Outcome = ZigTestOutcome.Skipped;
                }
                else if (passed > 0)
                {
                    result.Outcome = ZigTestOutcome.Passed;
                }
                else
                {
                    result.Outcome = ZigTestOutcome.NotFound;
                }

                return result;
            }

            if (lowerOutput.Contains("error:") || output.ExitCode != 0)
            {
                result.Outcome = ZigTestOutcome.Failed;
                if (string.IsNullOrWhiteSpace(result.Output))
                {
                    result.DiagnosticMessage = "The test process exited with code " + output.ExitCode + ".";
                }
                return result;
            }

            result.Outcome = ZigTestOutcome.Skipped;
            result.DiagnosticMessage = "The test results could not be parsed. [" + normalizedOutput + "]";
            return result;
        }

        static string CombineOutput(string standardError, string standardOutput)
        {
            string left = (standardError ?? string.Empty).Trim();
            string right = (standardOutput ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(left))
            {
                return right;
            }

            if (string.IsNullOrEmpty(right))
            {
                return left;
            }

            return left + Environment.NewLine + right;
        }
    }
}
