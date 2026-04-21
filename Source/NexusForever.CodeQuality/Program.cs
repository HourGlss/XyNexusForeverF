using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NexusForever.CodeQuality;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static int Main(string[] args)
    {
        try
        {
            Options options = Options.Parse(args);
            string sourceRoot = Path.GetFullPath(options.SourceRoot);
            string outputRoot = Path.GetFullPath(options.OutputRoot);
            string rawRoot = Path.Combine(outputRoot, "raw");

            Directory.CreateDirectory(outputRoot);
            Directory.CreateDirectory(rawRoot);

            List<SourceFileMetric> files = AnalyseSource(sourceRoot);
            BuildSummary? buildSummary = BuildSummary.TryLoad(options.BuildLog);
            CoverageSummary coverageSummary = CoverageSummary.Load(options.CoverageRoot);

            LaneReport complexity = BuildComplexityReport(files);
            LaneReport cleanCode = BuildCleanCodeReport(files, buildSummary);
            LaneReport coverage = BuildCoverageReport(coverageSummary, sourceRoot);

            QualityReport report = BuildQualityReport(sourceRoot, files, complexity, cleanCode, coverage, buildSummary);

            WriteJson(Path.Combine(outputRoot, "quality-report.json"), report);
            WriteJson(Path.Combine(rawRoot, "files.json"), files);
            WriteJson(Path.Combine(rawRoot, "functions.json"), files.SelectMany(f => f.Functions).ToList());
            WriteJson(Path.Combine(rawRoot, "coverage.json"), coverageSummary);
            WriteCsv(Path.Combine(rawRoot, "files.csv"), files);
            WriteCsv(Path.Combine(rawRoot, "functions.csv"), files.SelectMany(f => f.Functions));
            WriteHtml(Path.Combine(outputRoot, "index.html"), report, files);

            Console.WriteLine($"Quality report written to {outputRoot}");
            Console.WriteLine($"Overall score: {FormatScore(report.Overall.Score)} {report.Overall.Grade}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static List<SourceFileMetric> AnalyseSource(string sourceRoot)
    {
        List<SourceFileMetric> files = [];

        foreach (string path in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                     .Where(IsSourcePath)
                     .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            string relativePath = NormalizePath(Path.GetRelativePath(sourceRoot, path));
            string text = File.ReadAllText(path);
            SourceClassification classification = Classify(relativePath, text);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(text, path: path);
            SyntaxNode root = tree.GetRoot();

            List<FunctionMetric> functions = CollectFunctions(relativePath, root, text);
            int lineCount = CountLines(text);
            int meaningfulLineCount = CountMeaningfulLines(text);
            int worstCyclomatic = functions.Count == 0 ? 0 : functions.Max(f => f.Cyclomatic);
            double averageCyclomatic = functions.Count == 0 ? 0 : Math.Round(functions.Average(f => f.Cyclomatic), 2);
            int worstCognitive = functions.Count == 0 ? 0 : functions.Max(f => f.Cognitive);
            string sizeBand = GetSizeBand(classification, lineCount);
            double cleanCodeScore = CalculateCleanCodeFileScore(classification, lineCount, HasInlineTests(text));
            double complexityScore = CalculateFileComplexityScore(functions);
            double hotspotScore = CalculateFileHotspotScore(functions, meaningfulLineCount, cleanCodeScore, complexityScore);

            files.Add(new SourceFileMetric
            {
                Path = relativePath,
                Classification = classification.ToString().ToLowerInvariant(),
                LineCount = lineCount,
                MeaningfulLineCount = meaningfulLineCount,
                HasInlineTests = HasInlineTests(text),
                Functions = functions,
                FunctionCount = functions.Count,
                WorstCyclomatic = worstCyclomatic,
                AverageCyclomatic = averageCyclomatic,
                WorstCognitive = worstCognitive,
                SizeBand = sizeBand,
                CleanCodeScore = cleanCodeScore,
                ComplexityScore = complexityScore,
                HotspotScore = hotspotScore,
                HotspotReason = DescribeFileHotspot(functions, meaningfulLineCount, cleanCodeScore, complexityScore)
            });
        }

        return files;
    }

    private static List<FunctionMetric> CollectFunctions(string relativePath, SyntaxNode root, string text)
    {
        List<FunctionMetric> functions = [];

        foreach (BaseMethodDeclarationSyntax node in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
            functions.Add(BuildFunctionMetric(relativePath, node, GetFunctionName(node), text));

        foreach (LocalFunctionStatementSyntax node in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
            functions.Add(BuildFunctionMetric(relativePath, node, GetFunctionName(node), text));

        foreach (AccessorDeclarationSyntax node in root.DescendantNodes().OfType<AccessorDeclarationSyntax>())
        {
            if (node.Body == null && node.ExpressionBody == null)
                continue;

            functions.Add(BuildFunctionMetric(relativePath, node, GetFunctionName(node), text));
        }

        return functions
            .OrderBy(f => f.StartLine)
            .ThenBy(f => f.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static FunctionMetric BuildFunctionMetric(string relativePath, SyntaxNode node, string name, string text)
    {
        FileLinePositionSpan lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
        int startLine = lineSpan.StartLinePosition.Line + 1;
        int endLine = lineSpan.EndLinePosition.Line + 1;
        int sloc = CountMeaningfulLines(GetTextSlice(text, node.Span));

        ComplexityWalker walker = new();
        walker.Visit(node);
        double maintainabilityIndex = CalculateMaintainabilityIndex(walker.Cyclomatic, walker.Cognitive, sloc);
        double hotspotScore = CalculateFunctionHotspotScore(walker.Cyclomatic, walker.Cognitive, maintainabilityIndex, sloc);

        return new FunctionMetric
        {
            FilePath = relativePath,
            Name = name,
            StartLine = startLine,
            EndLine = endLine,
            Cyclomatic = walker.Cyclomatic,
            Cognitive = walker.Cognitive,
            MaintainabilityIndex = maintainabilityIndex,
            Sloc = sloc,
            Grade = GradeCyclomatic(walker.Cyclomatic),
            HotspotScore = hotspotScore,
            HotspotReason = DescribeFunctionHotspot(walker.Cyclomatic, walker.Cognitive, maintainabilityIndex, sloc)
        };
    }

    private static LaneReport BuildComplexityReport(IReadOnlyCollection<SourceFileMetric> files)
    {
        List<SourceFileMetric> measuredFiles = files
            .Where(f => IsMeasuredSource(f.Classification))
            .ToList();
        List<SourceFileMetric> scoredFiles = measuredFiles
            .Where(f => f.Functions.Count > 0)
            .ToList();

        List<FunctionMetric> functions = scoredFiles.SelectMany(f => f.Functions).ToList();

        if (scoredFiles.Count == 0)
        {
            return new LaneReport
            {
                Name = "Complexity",
                Status = "degraded",
                Summary = "No measured source functions were available for complexity scoring.",
                Formula = "Average file complexity score; each file is 50% worst-function grade, 30% average-function grade, and 20% average maintainability.",
                Findings = [],
                Notes = ["Only test and tooling files are excluded from complexity scoring."]
            };
        }

        double averageFileComplexityScore = scoredFiles.Average(f => f.ComplexityScore);
        double averageWorstFunctionHotspotScore = scoredFiles.Average(f => GetWorstFunction(f)?.HotspotScore ?? 0.0);
        double filesWithoutEfHotspots = 100.0 * scoredFiles.Count(f => !IsEfGrade(GradeCyclomatic(f.WorstCyclomatic))) / scoredFiles.Count;
        double score = RoundScore(averageFileComplexityScore);

        List<string> findings = measuredFiles
            .OrderByDescending(f => f.HotspotScore)
            .ThenByDescending(f => f.WorstCyclomatic)
            .ThenByDescending(f => f.WorstCognitive)
            .ThenBy(f => f.Path, StringComparer.Ordinal)
            .Take(12)
            .Select(f =>
            {
                FunctionMetric? worst = GetWorstFunction(f);
                string worstFunction = worst == null
                    ? "no functions"
                    : $"{worst.Name}:{worst.StartLine} hotspot={FormatScore(worst.HotspotScore)}, cyclomatic={worst.Cyclomatic}, cognitive={worst.Cognitive}";
                return $"{f.Path}: file_hotspot={FormatScore(f.HotspotScore)}, complexity_score={FormatScore(f.ComplexityScore)}, clean_score={FormatScore(f.CleanCodeScore)}, {worstFunction}";
            })
            .ToList();

        return new LaneReport
        {
            Name = "Complexity",
            Status = score >= 70.0 ? "ok" : "degraded",
            Score = score,
            Grade = GradeScore(score),
            Summary = $"{measuredFiles.Count} measured source files and {functions.Count} functions scored; {files.Count - measuredFiles.Count} test/tooling files excluded.",
            Formula = "Average file complexity score; each file is 50% worst-function grade, 30% average-function grade, and 20% average maintainability.",
            Findings = findings,
            Notes =
            [
                "Cyclomatic complexity uses Roslyn syntax counting for branches, loops, switch arms, catch blocks, conditionals, and short-circuit boolean operators.",
                "Function hotspot score explains local risk: 40% cyclomatic risk, 25% cognitive risk, 20% maintainability risk, and 15% function size risk.",
                "File hotspot score explains file-level risk: 45% complexity risk, 25% clean-code risk, 20% file-size risk, and 10% function-count risk.",
                "Only test and tooling files are excluded. Generated, migration, designer, and other .cs files are measured."
            ],
            Metrics = new Dictionary<string, object?>
            {
                ["measured_source_file_count"] = measuredFiles.Count,
                ["function_scored_file_count"] = scoredFiles.Count,
                ["excluded_test_tooling_file_count"] = files.Count - measuredFiles.Count,
                ["measured_function_count"] = functions.Count,
                ["average_file_complexity_score"] = RoundScore(averageFileComplexityScore),
                ["average_worst_function_hotspot_score"] = RoundScore(averageWorstFunctionHotspotScore),
                ["measured_files_without_ef_hotspots_percent"] = RoundScore(filesWithoutEfHotspots)
            }
        };
    }

    private static LaneReport BuildCleanCodeReport(IReadOnlyCollection<SourceFileMetric> files, BuildSummary? buildSummary)
    {
        List<SourceFileMetric> measuredFiles = files
            .Where(f => IsMeasuredSource(f.Classification))
            .ToList();
        List<SourceFileMetric> excludedFiles = files
            .Where(f => !IsMeasuredSource(f.Classification))
            .ToList();

        double measuredAverage = measuredFiles.Count == 0 ? 100.0 : measuredFiles.Average(f => f.CleanCodeScore);
        double compactMeasuredPercent = measuredFiles.Count == 0
            ? 100.0
            : 100.0 * measuredFiles.Count(f => f.SizeBand != "oversized" && !f.HasInlineTests) / measuredFiles.Count;
        double structuralScore = RoundScore(0.85 * measuredAverage + 0.15 * compactMeasuredPercent);

        double? staticAnalysisScore = buildSummary == null
            ? null
            : CalculateStaticAnalysisScore(buildSummary.WarningCount, buildSummary.BuildSucceeded);
        double score = staticAnalysisScore == null
            ? structuralScore
            : RoundScore(0.80 * structuralScore + 0.20 * staticAnalysisScore.Value);

        List<string> findings = measuredFiles
            .OrderByDescending(f => f.HotspotScore)
            .ThenByDescending(f => f.LineCount)
            .ThenBy(f => f.Path, StringComparer.Ordinal)
            .Take(15)
            .Select(f => $"{f.Path}: hotspot={FormatScore(f.HotspotScore)}, clean_score={FormatScore(f.CleanCodeScore)}, {f.LineCount} lines, {f.FunctionCount} functions, {f.SizeBand}")
            .ToList();

        if (buildSummary is { WarningCount: > 0 })
            findings.Add($"Build log contains {buildSummary.WarningCount} distinct warning line(s).");

        List<string> notes =
        [
            "Structural scoring follows the Rusaren/Safer template pattern: production size, test separation, and static-analysis evidence are measured separately.",
            "Only test and tooling files are excluded from clean-code scoring.",
            "Generated, migration, designer, and other .cs files are measured with the same source file-size bands.",
            "Inline tests are detected with common C# test attributes such as Fact, Theory, Test, and TestMethod, then excluded as test code."
        ];

        if (buildSummary == null)
            notes.Add("No build log was supplied, so the clean-code score uses structural evidence only.");

        return new LaneReport
        {
            Name = "Clean Code",
            Status = score >= 70.0 ? "ok" : "degraded",
            Score = score,
            Grade = GradeScore(score),
            Summary = $"{measuredFiles.Count} measured source files, {excludedFiles.Count} test/tooling files excluded, {measuredFiles.Count(f => f.SizeBand == "oversized")} measured oversized files.",
            Formula = staticAnalysisScore == null
                ? "85% average measured-file clean score + 15% non-oversized measured-file rate"
                : "80% structural clean-code score + 20% build warning score",
            Findings = findings,
            Notes = notes,
            Metrics = new Dictionary<string, object?>
            {
                ["structural_clean_code_score"] = structuralScore,
                ["static_analysis_score"] = staticAnalysisScore,
                ["measured_source_files"] = measuredFiles.Count,
                ["excluded_test_tooling_files"] = excludedFiles.Count,
                ["test_files"] = files.Count(f => f.Classification == "test"),
                ["tooling_files"] = files.Count(f => f.Classification == "tooling"),
                ["generated_files_measured"] = files.Count(f => f.Classification == "generated"),
                ["oversized_measured_files"] = measuredFiles.Count(f => f.SizeBand == "oversized"),
                ["measured_inline_tests"] = measuredFiles.Count(f => f.HasInlineTests),
                ["compact_measured_percent"] = RoundScore(compactMeasuredPercent)
            }
        };
    }

    private static LaneReport BuildCoverageReport(CoverageSummary summary, string sourceRoot)
    {
        int testProjectCount = CountTestProjects(sourceRoot);

        if (!summary.HasCoverage)
        {
            return new LaneReport
            {
                Name = "Coverage",
                Status = "degraded",
                Summary = "No Cobertura coverage artifact was found.",
                Formula = "70% line coverage + 30% branch coverage when coverage artifacts exist",
                Findings = testProjectCount == 0
                    ? ["No test projects were detected in the solution source tree."]
                    : ["Coverage was not generated. Ensure test projects reference coverlet.collector or another Cobertura-producing collector."],
                Notes =
                [
                    "Coverage is intentionally reported as unavailable rather than estimated.",
                    "The quality wrapper looks for coverage.cobertura.xml under the configured test results root."
                ],
                Metrics = new Dictionary<string, object?>
                {
                    ["test_project_count"] = testProjectCount,
                    ["coverage_file_count"] = 0
                }
            };
        }

        double branchCoverage = summary.BranchCoveragePercent ?? summary.LineCoveragePercent;
        double score = RoundScore(0.70 * summary.LineCoveragePercent + 0.30 * branchCoverage);

        return new LaneReport
        {
            Name = "Coverage",
            Status = score >= 70.0 ? "ok" : "degraded",
            Score = score,
            Grade = GradeScore(score),
            Summary = $"Line coverage {FormatScore(summary.LineCoveragePercent)}%, branch coverage {FormatScore(summary.BranchCoveragePercent)}%.",
            Formula = "70% line coverage + 30% branch coverage",
            Findings = summary.Files
                .Take(10)
                .Select(f => $"Coverage artifact: {f}")
                .ToList(),
            Notes =
            [
                "Coverage reports are scoped to whatever the test runner and collector emitted.",
                "Per-file critical thresholds should be added once test projects exist."
            ],
            Metrics = new Dictionary<string, object?>
            {
                ["test_project_count"] = testProjectCount,
                ["coverage_file_count"] = summary.Files.Count,
                ["line_coverage_percent"] = RoundScore(summary.LineCoveragePercent),
                ["branch_coverage_percent"] = summary.BranchCoveragePercent == null ? null : RoundScore(summary.BranchCoveragePercent.Value),
                ["lines_covered"] = summary.LinesCovered,
                ["lines_valid"] = summary.LinesValid
            }
        };
    }

    private static QualityReport BuildQualityReport(
        string sourceRoot,
        IReadOnlyCollection<SourceFileMetric> files,
        LaneReport complexity,
        LaneReport cleanCode,
        LaneReport coverage,
        BuildSummary? buildSummary)
    {
        List<LaneReport> lanes = [complexity, cleanCode, coverage];
        List<double> scoredLanes = lanes.Where(l => l.Score != null).Select(l => l.Score!.Value).ToList();
        double? overallScore = scoredLanes.Count == 0 ? null : RoundScore(scoredLanes.Average());

        List<string> knownGaps = [];
        if (coverage.Score == null)
            knownGaps.Add("Coverage artifacts are missing, so coverage is reported as unavailable.");
        if (CountTestProjects(sourceRoot) == 0)
            knownGaps.Add("No C# test projects were detected; coverage and dynamic correctness cannot yet provide evidence.");
        if (buildSummary == null)
            knownGaps.Add("No build log was supplied; static-analysis scoring is structural-only.");

        return new QualityReport
        {
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            SourceRoot = NormalizePath(sourceRoot),
            Overall = new OverallSummary
            {
                Score = overallScore,
                Grade = GradeScore(overallScore),
                Summary = overallScore == null
                    ? "No scored lanes were available."
                    : $"{lanes.Count(l => l.Score != null)} scored lane(s), {knownGaps.Count} known gap(s).",
                WouldFailGate = lanes.Any(l => l.Status == "failed")
            },
            Lanes = lanes,
            KnownGaps = knownGaps,
            Inventory = new InventorySummary
            {
                SourceFiles = files.Count,
                MeasuredFiles = files.Count(f => IsMeasuredSource(f.Classification)),
                RuntimeFiles = files.Count(f => f.Classification == "runtime"),
                EntrypointFiles = files.Count(f => f.Classification == "entrypoint"),
                GeneratedFiles = files.Count(f => f.Classification == "generated"),
                TestFiles = files.Count(f => f.Classification == "test"),
                ToolingFiles = files.Count(f => f.Classification == "tooling"),
                ExcludedFromMeasurements = files.Count(f => !IsMeasuredSource(f.Classification)),
                FunctionCount = files.Sum(f => f.FunctionCount)
            },
            Artifacts = new Dictionary<string, string>
            {
                ["html"] = "index.html",
                ["json"] = "quality-report.json",
                ["raw_files"] = "raw/files.json",
                ["raw_functions"] = "raw/functions.json",
                ["raw_coverage"] = "raw/coverage.json"
            }
        };
    }

    private static string GetFunctionName(BaseMethodDeclarationSyntax node)
    {
        string name = node switch
        {
            MethodDeclarationSyntax method => method.Identifier.ValueText,
            ConstructorDeclarationSyntax constructor => constructor.Identifier.ValueText,
            DestructorDeclarationSyntax destructor => "~" + destructor.Identifier.ValueText,
            OperatorDeclarationSyntax op => "operator " + op.OperatorToken.ValueText,
            ConversionOperatorDeclarationSyntax conversion => "operator " + conversion.Type,
            _ => node.Kind().ToString()
        };

        return AddContainerPrefix(node, name);
    }

    private static string GetFunctionName(LocalFunctionStatementSyntax node)
        => AddContainerPrefix(node, node.Identifier.ValueText);

    private static string GetFunctionName(AccessorDeclarationSyntax node)
    {
        string memberName = node.Parent?.Parent switch
        {
            PropertyDeclarationSyntax property => property.Identifier.ValueText,
            IndexerDeclarationSyntax => "this[]",
            EventDeclarationSyntax evt => evt.Identifier.ValueText,
            EventFieldDeclarationSyntax evtField => string.Join(",", evtField.Declaration.Variables.Select(v => v.Identifier.ValueText)),
            _ => "accessor"
        };

        return AddContainerPrefix(node, $"{memberName}.{node.Keyword.ValueText}");
    }

    private static string AddContainerPrefix(SyntaxNode node, string memberName)
    {
        List<string> parts = [];

        for (SyntaxNode? current = node.Parent; current != null; current = current.Parent)
        {
            switch (current)
            {
                case BaseTypeDeclarationSyntax type:
                    parts.Add(type.Identifier.ValueText);
                    break;
                case FileScopedNamespaceDeclarationSyntax fileScopedNamespace:
                    parts.Add(fileScopedNamespace.Name.ToString());
                    break;
                case NamespaceDeclarationSyntax namespaceDeclaration:
                    parts.Add(namespaceDeclaration.Name.ToString());
                    break;
            }
        }

        parts.Reverse();
        parts.Add(memberName);
        return string.Join(".", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static SourceClassification Classify(string relativePath, string text)
    {
        string lower = relativePath.ToLowerInvariant();
        string fileName = Path.GetFileName(relativePath);

        if (lower.StartsWith("nexusforever.codequality/", StringComparison.Ordinal)
            || lower.Contains("/tools/", StringComparison.Ordinal)
            || lower.Contains("/scripts/", StringComparison.Ordinal))
            return SourceClassification.Tooling;

        if (IsTestPath(lower) || HasInlineTests(text))
            return SourceClassification.Test;

        if (lower.Contains("/migrations/", StringComparison.Ordinal)
            || lower.EndsWith(".designer.cs", StringComparison.Ordinal)
            || lower.EndsWith(".g.cs", StringComparison.Ordinal)
            || lower.EndsWith(".generated.cs", StringComparison.Ordinal)
            || lower.EndsWith("assemblyinfo.cs", StringComparison.Ordinal)
            || lower.EndsWith("globalusings.g.cs", StringComparison.Ordinal))
            return SourceClassification.Generated;

        if (fileName.Equals("Program.cs", StringComparison.OrdinalIgnoreCase))
            return SourceClassification.Entrypoint;

        return SourceClassification.Runtime;
    }

    private static bool IsSourcePath(string path)
    {
        string normalized = NormalizePath(path).ToLowerInvariant();
        return !normalized.Contains("/bin/", StringComparison.Ordinal)
            && !normalized.Contains("/obj/", StringComparison.Ordinal)
            && !normalized.Contains("/.git/", StringComparison.Ordinal);
    }

    private static bool IsTestPath(string lowerPath)
        => lowerPath.Contains("/test/", StringComparison.Ordinal)
            || lowerPath.Contains("/tests/", StringComparison.Ordinal)
            || lowerPath.Contains(".tests/", StringComparison.Ordinal)
            || lowerPath.Contains(".test/", StringComparison.Ordinal)
            || Path.GetFileNameWithoutExtension(lowerPath).EndsWith("tests", StringComparison.Ordinal);

    private static bool HasInlineTests(string text)
        => Regex.IsMatch(text, @"(?m)^\s*\[\s*(Fact|Theory|Test|TestCase|TestMethod|DataTestMethod)\b", RegexOptions.CultureInvariant);

    private static bool IsMeasuredSource(string classification)
        => classification is not "test" and not "tooling";

    private static bool IsEfGrade(string grade)
        => grade is "E" or "F";

    private static string GetTextSlice(string text, TextSpan span)
        => span.End <= text.Length ? text.Substring(span.Start, span.Length) : string.Empty;

    private static int CountLines(string text)
    {
        if (text.Length == 0)
            return 0;

        int lines = 1;
        foreach (char c in text)
        {
            if (c == '\n')
                lines++;
        }

        return text.EndsWith('\n') ? lines - 1 : lines;
    }

    private static int CountMeaningfulLines(string text)
    {
        bool inBlockComment = false;
        int count = 0;

        foreach (string rawLine in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0)
                continue;

            if (inBlockComment)
            {
                int end = line.IndexOf("*/", StringComparison.Ordinal);
                if (end < 0)
                    continue;

                line = line[(end + 2)..].Trim();
                inBlockComment = false;
                if (line.Length == 0)
                    continue;
            }

            while (line.StartsWith("/*", StringComparison.Ordinal))
            {
                int end = line.IndexOf("*/", 2, StringComparison.Ordinal);
                if (end < 0)
                {
                    inBlockComment = true;
                    line = string.Empty;
                    break;
                }

                line = line[(end + 2)..].Trim();
            }

            if (line.Length == 0
                || line.StartsWith("//", StringComparison.Ordinal)
                || line.StartsWith("///", StringComparison.Ordinal)
                || line.StartsWith("#pragma", StringComparison.Ordinal)
                || line.StartsWith("#nullable", StringComparison.Ordinal))
                continue;

            count++;
        }

        return count;
    }

    private static double CalculateMaintainabilityIndex(int cyclomatic, int cognitive, int sloc)
    {
        double score = 100.0 - (cyclomatic * 2.5) - (cognitive * 1.25) - (Math.Max(0, sloc - 30) * 0.5);
        return RoundScore(Math.Clamp(score, 0.0, 100.0));
    }

    private static double CalculateFunctionHotspotScore(int cyclomatic, int cognitive, double maintainabilityIndex, int sloc)
    {
        double cyclomaticRisk = RiskFromThreshold(cyclomatic, healthy: 5.0, high: 45.0);
        double cognitiveRisk = RiskFromThreshold(cognitive, healthy: 10.0, high: 90.0);
        double maintainabilityRisk = 100.0 - maintainabilityIndex;
        double sizeRisk = RiskFromThreshold(sloc, healthy: 30.0, high: 180.0);

        return RoundScore(0.40 * cyclomaticRisk
            + 0.25 * cognitiveRisk
            + 0.20 * maintainabilityRisk
            + 0.15 * sizeRisk);
    }

    private static string DescribeFunctionHotspot(int cyclomatic, int cognitive, double maintainabilityIndex, int sloc)
    {
        List<string> reasons = [];

        if (cyclomatic > 10)
            reasons.Add($"cyclomatic {cyclomatic}");
        if (cognitive > 15)
            reasons.Add($"cognitive {cognitive}");
        if (maintainabilityIndex < 70.0)
            reasons.Add($"maintainability {FormatScore(maintainabilityIndex)}");
        if (sloc > 60)
            reasons.Add($"SLOC {sloc}");

        return reasons.Count == 0
            ? "low local complexity"
            : string.Join("; ", reasons);
    }

    private static double CalculateFileComplexityScore(IReadOnlyCollection<FunctionMetric> functions)
    {
        if (functions.Count == 0)
            return 100.0;

        double worstFunctionGradeScore = functions.Min(f => GradeScore(f.Grade));
        double averageFunctionGradeScore = functions.Average(f => GradeScore(f.Grade));
        double averageMaintainabilityIndex = functions.Average(f => f.MaintainabilityIndex);

        return RoundScore(0.50 * worstFunctionGradeScore
            + 0.30 * averageFunctionGradeScore
            + 0.20 * averageMaintainabilityIndex);
    }

    private static double CalculateFileHotspotScore(
        IReadOnlyCollection<FunctionMetric> functions,
        int meaningfulLineCount,
        double cleanCodeScore,
        double complexityScore)
    {
        double complexityRisk = 100.0 - complexityScore;
        double cleanCodeRisk = 100.0 - cleanCodeScore;
        double sizeRisk = RiskFromThreshold(meaningfulLineCount, healthy: 300.0, high: 1600.0);
        double functionCountRisk = RiskFromThreshold(functions.Count, healthy: 12.0, high: 120.0);

        return RoundScore(0.45 * complexityRisk
            + 0.25 * cleanCodeRisk
            + 0.20 * sizeRisk
            + 0.10 * functionCountRisk);
    }

    private static string DescribeFileHotspot(
        IReadOnlyCollection<FunctionMetric> functions,
        int meaningfulLineCount,
        double cleanCodeScore,
        double complexityScore)
    {
        List<string> reasons = [];
        FunctionMetric? worst = functions
            .OrderByDescending(f => f.HotspotScore)
            .ThenByDescending(f => f.Cyclomatic)
            .FirstOrDefault();

        if (complexityScore < 80.0)
            reasons.Add($"complexity score {FormatScore(complexityScore)}");
        if (cleanCodeScore < 80.0)
            reasons.Add($"clean score {FormatScore(cleanCodeScore)}");
        if (meaningfulLineCount > 600)
            reasons.Add($"{meaningfulLineCount} meaningful lines");
        if (functions.Count > 30)
            reasons.Add($"{functions.Count} functions");
        if (worst is { HotspotScore: >= 35.0 })
            reasons.Add($"worst function {worst.Name}:{worst.StartLine} hotspot {FormatScore(worst.HotspotScore)}");

        return reasons.Count == 0
            ? "no dominant hotspot signal; ranked by combined score"
            : string.Join("; ", reasons);
    }

    private static FunctionMetric? GetWorstFunction(SourceFileMetric file)
        => file.Functions
            .OrderByDescending(f => f.HotspotScore)
            .ThenByDescending(f => f.Cyclomatic)
            .ThenByDescending(f => f.Cognitive)
            .FirstOrDefault();

    private static double RiskFromThreshold(double value, double healthy, double high)
    {
        if (value <= healthy)
            return 0.0;
        if (value >= high)
            return 100.0;

        return RoundScore(100.0 * (value - healthy) / (high - healthy));
    }

    private static double CalculateCleanCodeFileScore(SourceClassification classification, int lineCount, bool hasInlineTests)
    {
        double score = classification switch
        {
            SourceClassification.Runtime => ScoreByBands(lineCount, [(250, 100), (400, 90), (600, 75), (800, 60), (1000, 45), (1400, 30)], 15),
            SourceClassification.Entrypoint => ScoreByBands(lineCount, [(150, 100), (250, 85), (400, 70), (600, 50)], 25),
            SourceClassification.Generated => ScoreByBands(lineCount, [(250, 100), (400, 90), (600, 75), (800, 60), (1000, 45), (1400, 30)], 15),
            SourceClassification.Test => ScoreByBands(lineCount, [(250, 100), (400, 92), (600, 82), (900, 68), (1200, 52), (1800, 35)], 20),
            SourceClassification.Tooling => ScoreByBands(lineCount, [(250, 100), (500, 85), (900, 65)], 40),
            _ => 100
        };

        if (hasInlineTests && classification is SourceClassification.Runtime or SourceClassification.Entrypoint)
            score -= 35.0;

        if ((classification is SourceClassification.Runtime or SourceClassification.Generated) && lineCount > 1000)
            score -= 10.0;
        if (classification == SourceClassification.Entrypoint && lineCount > 500)
            score -= 10.0;
        if (classification == SourceClassification.Test && lineCount > 1800)
            score -= 10.0;

        return RoundScore(Math.Clamp(score, 0.0, 100.0));
    }

    private static double ScoreByBands(int lineCount, IReadOnlyList<(int MaxLineCount, double Score)> bands, double fallbackScore)
    {
        foreach ((int maxLineCount, double score) in bands)
        {
            if (lineCount <= maxLineCount)
                return score;
        }

        return fallbackScore;
    }

    private static string GetSizeBand(SourceClassification classification, int lineCount)
    {
        return classification switch
        {
            SourceClassification.Runtime => lineCount <= 400 ? "compact" : lineCount <= 800 ? "large" : "oversized",
            SourceClassification.Entrypoint => lineCount <= 250 ? "compact" : lineCount <= 500 ? "large" : "oversized",
            SourceClassification.Generated => lineCount <= 400 ? "compact" : lineCount <= 800 ? "large" : "oversized",
            SourceClassification.Test => lineCount <= 400 ? "compact" : lineCount <= 1200 ? "large" : "oversized",
            SourceClassification.Tooling => lineCount <= 500 ? "compact" : lineCount <= 900 ? "large" : "oversized",
            _ => "excluded"
        };
    }

    private static string GradeCyclomatic(double cyclomatic)
    {
        if (cyclomatic <= 5)
            return "A";
        if (cyclomatic <= 10)
            return "B";
        if (cyclomatic <= 20)
            return "C";
        if (cyclomatic <= 30)
            return "D";
        if (cyclomatic <= 40)
            return "E";
        return "F";
    }

    private static double GradeScore(string grade)
    {
        return grade switch
        {
            "A" => 100.0,
            "B" => 85.0,
            "C" => 70.0,
            "D" => 55.0,
            "E" => 40.0,
            "F" => 20.0,
            _ => 0.0
        };
    }

    private static string GradeScore(double? score)
    {
        if (score == null)
            return "N/A";
        if (score >= 90)
            return "A";
        if (score >= 80)
            return "B";
        if (score >= 70)
            return "C";
        if (score >= 60)
            return "D";
        if (score >= 40)
            return "E";
        return "F";
    }

    private static double CalculateStaticAnalysisScore(int warningCount, bool buildSucceeded)
    {
        double warningPenalty = Math.Min(60.0, warningCount * 4.0);
        double baseline = buildSucceeded ? 100.0 : 70.0;
        return RoundScore(Math.Max(0.0, baseline - warningPenalty));
    }

    private static int CountTestProjects(string sourceRoot)
    {
        int count = 0;

        foreach (string projectPath in Directory.EnumerateFiles(sourceRoot, "*.csproj", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileNameWithoutExtension(projectPath);
            if (fileName.Contains("Test", StringComparison.OrdinalIgnoreCase))
            {
                count++;
                continue;
            }

            try
            {
                XDocument project = XDocument.Load(projectPath);
                if (project.Descendants("IsTestProject").Any(e => string.Equals(e.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase)))
                    count++;
            }
            catch
            {
                // A malformed project file will be surfaced by the build lane; keep report generation resilient.
            }
        }

        return count;
    }

    private static void WriteJson<T>(string path, T value)
        => File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions) + Environment.NewLine);

    private static void WriteCsv(string path, IEnumerable<SourceFileMetric> files)
    {
        StringBuilder builder = new();
        builder.AppendLine("path,classification,line_count,meaningful_line_count,function_count,worst_cyclomatic,average_cyclomatic,worst_cognitive,complexity_score,clean_code_score,hotspot_score,hotspot_reason,size_band,has_inline_tests");

        foreach (SourceFileMetric file in files)
        {
            builder.Append(Csv(file.Path)).Append(',')
                .Append(Csv(file.Classification)).Append(',')
                .Append(file.LineCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.MeaningfulLineCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.FunctionCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.WorstCyclomatic.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.AverageCyclomatic.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.WorstCognitive.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.ComplexityScore.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.CleanCodeScore.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(file.HotspotScore.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Csv(file.HotspotReason)).Append(',')
                .Append(Csv(file.SizeBand)).Append(',')
                .Append(file.HasInlineTests ? "true" : "false")
                .AppendLine();
        }

        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteCsv(string path, IEnumerable<FunctionMetric> functions)
    {
        StringBuilder builder = new();
        builder.AppendLine("file_path,name,start_line,end_line,cyclomatic,cognitive,maintainability_index,sloc,grade,hotspot_score,hotspot_reason");

        foreach (FunctionMetric function in functions)
        {
            builder.Append(Csv(function.FilePath)).Append(',')
                .Append(Csv(function.Name)).Append(',')
                .Append(function.StartLine.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(function.EndLine.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(function.Cyclomatic.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(function.Cognitive.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(function.MaintainabilityIndex.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(function.Sloc.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Csv(function.Grade)).Append(',')
                .Append(function.HotspotScore.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Csv(function.HotspotReason))
                .AppendLine();
        }

        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteHtml(string path, QualityReport report, IReadOnlyCollection<SourceFileMetric> files)
    {
        IEnumerable<SourceFileMetric> fileHotspots = files
            .Where(f => IsMeasuredSource(f.Classification))
            .OrderByDescending(f => f.HotspotScore)
            .ThenBy(f => f.Path, StringComparer.Ordinal)
            .Take(25);

        IEnumerable<SourceFileMetric> largestFiles = files
            .Where(f => IsMeasuredSource(f.Classification))
            .OrderByDescending(f => f.LineCount)
            .Take(25);

        StringBuilder builder = new();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\"><head><meta charset=\"utf-8\"><title>NexusForever Quality Report</title>");
        builder.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;line-height:1.45;color:#20242a}table{border-collapse:collapse;width:100%;margin:1rem 0}th,td{border:1px solid #d7dce2;padding:.45rem;text-align:left;vertical-align:top}th{background:#eef2f6}.lane{border:1px solid #d7dce2;border-radius:8px;padding:1rem;margin:1rem 0}.score{font-size:2rem;font-weight:700}.muted{color:#5c6670}code{background:#eef2f6;padding:.1rem .25rem;border-radius:4px}</style>");
        builder.AppendLine("</head><body>");
        builder.AppendLine("<h1>NexusForever Quality Report</h1>");
        builder.AppendLine($"<p class=\"muted\">Generated {Html(report.GeneratedAtUtc.ToString("u", CultureInfo.InvariantCulture))}</p>");
        builder.AppendLine($"<p class=\"score\">Overall {Html(FormatScore(report.Overall.Score))}/100 {Html(report.Overall.Grade)}</p>");
        builder.AppendLine($"<p>{Html(report.Overall.Summary)}</p>");

        builder.AppendLine("<h2>Available Reports</h2>");
        foreach (LaneReport lane in report.Lanes)
        {
            builder.AppendLine("<section class=\"lane\">");
            builder.AppendLine($"<h3>{Html(lane.Status.ToUpperInvariant())} {Html(lane.Name)} - {Html(FormatScore(lane.Score))}/100 {Html(lane.Grade ?? "N/A")}</h3>");
            builder.AppendLine($"<p>{Html(lane.Summary)}</p>");
            builder.AppendLine($"<p><strong>Formula:</strong> {Html(lane.Formula)}</p>");
            if (lane.Findings.Count > 0)
            {
                builder.AppendLine("<ul>");
                foreach (string finding in lane.Findings.Take(8))
                    builder.AppendLine($"<li>{Html(finding)}</li>");
                builder.AppendLine("</ul>");
            }
            builder.AppendLine("</section>");
        }

        if (report.KnownGaps.Count > 0)
        {
            builder.AppendLine("<h2>Known Gaps</h2><ul>");
            foreach (string gap in report.KnownGaps)
                builder.AppendLine($"<li>{Html(gap)}</li>");
            builder.AppendLine("</ul>");
        }

        builder.AppendLine("<h2>File Hotspots</h2>");
        builder.AppendLine("<p class=\"muted\">Hotspot score is a 0-100 attention score, where higher means the file is more likely to deserve cleanup. It combines complexity risk, clean-code risk, file size, and function count. Tests and tooling are excluded; every other .cs file is included.</p>");
        builder.AppendLine("<table><thead><tr><th>File</th><th>Hotspot Score</th><th>Complexity Score</th><th>Clean Score</th><th>Why</th><th>Lines</th><th>Meaningful Lines</th><th>Functions</th><th>Worst Function</th><th>Worst Function Hotspot</th><th>Worst Cyclomatic</th><th>Worst Cognitive</th><th>Worst MI</th><th>Classification</th></tr></thead><tbody>");
        foreach (SourceFileMetric file in fileHotspots)
        {
            FunctionMetric? worst = GetWorstFunction(file);
            builder.AppendLine("<tr>"
                + $"<td>{Html(file.Path)}</td>"
                + $"<td>{FormatScore(file.HotspotScore)}</td>"
                + $"<td>{FormatScore(file.ComplexityScore)}</td>"
                + $"<td>{FormatScore(file.CleanCodeScore)}</td>"
                + $"<td>{Html(file.HotspotReason)}</td>"
                + $"<td>{file.LineCount}</td>"
                + $"<td>{file.MeaningfulLineCount}</td>"
                + $"<td>{file.FunctionCount}</td>"
                + $"<td>{Html(worst == null ? "N/A" : $"{worst.Name}:{worst.StartLine}")}</td>"
                + $"<td>{(worst == null ? "N/A" : FormatScore(worst.HotspotScore))}</td>"
                + $"<td>{file.WorstCyclomatic}</td>"
                + $"<td>{file.WorstCognitive}</td>"
                + $"<td>{(worst == null ? "N/A" : FormatScore(worst.MaintainabilityIndex))}</td>"
                + $"<td>{Html(file.Classification)}</td>"
                + "</tr>");
        }
        builder.AppendLine("</tbody></table>");

        builder.AppendLine("<h2>Largest Measured Files</h2>");
        builder.AppendLine("<table><thead><tr><th>File</th><th>Lines</th><th>Meaningful Lines</th><th>Functions</th><th>Size Band</th><th>Clean Score</th></tr></thead><tbody>");
        foreach (SourceFileMetric file in largestFiles)
        {
            builder.AppendLine("<tr>"
                + $"<td>{Html(file.Path)}</td>"
                + $"<td>{file.LineCount}</td>"
                + $"<td>{file.MeaningfulLineCount}</td>"
                + $"<td>{file.FunctionCount}</td>"
                + $"<td>{Html(file.SizeBand)}</td>"
                + $"<td>{FormatScore(file.CleanCodeScore)}</td>"
                + "</tr>");
        }
        builder.AppendLine("</tbody></table>");

        builder.AppendLine("<h2>Artifacts</h2><ul>");
        foreach ((string name, string artifactPath) in report.Artifacts)
            builder.AppendLine($"<li><code>{Html(name)}</code>: <code>{Html(artifactPath)}</code></li>");
        builder.AppendLine("</ul>");
        builder.AppendLine("</body></html>");

        File.WriteAllText(path, builder.ToString());
    }

    private static string Csv(string value)
        => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string Html(string value)
        => value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal);

    private static string FormatScore(double? value)
        => value == null ? "N/A" : FormatScore(value.Value);

    private static string FormatScore(double value)
        => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static double RoundScore(double value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string NormalizePath(string path)
        => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private enum SourceClassification
    {
        Runtime,
        Entrypoint,
        Test,
        Tooling,
        Generated
    }
}

internal sealed class ComplexityWalker : CSharpSyntaxWalker
{
    private int nesting;

    public int Cyclomatic { get; private set; } = 1;

    public int Cognitive { get; private set; }

    public override void VisitIfStatement(IfStatementSyntax node)
        => VisitDecision(node, () => base.VisitIfStatement(node));

    public override void VisitForStatement(ForStatementSyntax node)
        => VisitDecision(node, () => base.VisitForStatement(node));

    public override void VisitForEachStatement(ForEachStatementSyntax node)
        => VisitDecision(node, () => base.VisitForEachStatement(node));

    public override void VisitWhileStatement(WhileStatementSyntax node)
        => VisitDecision(node, () => base.VisitWhileStatement(node));

    public override void VisitDoStatement(DoStatementSyntax node)
        => VisitDecision(node, () => base.VisitDoStatement(node));

    public override void VisitCatchClause(CatchClauseSyntax node)
        => VisitDecision(node, () => base.VisitCatchClause(node));

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        => VisitDecision(node, () => base.VisitConditionalExpression(node));

    public override void VisitSwitchSection(SwitchSectionSyntax node)
    {
        int labels = node.Labels.Count(static label => label is not DefaultSwitchLabelSyntax);
        for (int i = 0; i < labels; i++)
            AddDecision();

        nesting++;
        base.VisitSwitchSection(node);
        nesting--;
    }

    public override void VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        => VisitDecision(node, () => base.VisitSwitchExpressionArm(node));

    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (node.IsKind(SyntaxKind.LogicalAndExpression) || node.IsKind(SyntaxKind.LogicalOrExpression) || node.IsKind(SyntaxKind.CoalesceExpression))
            AddDecision(includeNesting: false);

        base.VisitBinaryExpression(node);
    }

    public override void VisitBinaryPattern(BinaryPatternSyntax node)
    {
        if (node.IsKind(SyntaxKind.AndPattern) || node.IsKind(SyntaxKind.OrPattern))
            AddDecision(includeNesting: false);

        base.VisitBinaryPattern(node);
    }

    public override void VisitWhenClause(WhenClauseSyntax node)
        => VisitDecision(node, () => base.VisitWhenClause(node));

    private void VisitDecision(SyntaxNode node, Action visit)
    {
        AddDecision();
        nesting++;
        visit();
        nesting--;
    }

    private void AddDecision(bool includeNesting = true)
    {
        Cyclomatic++;
        Cognitive += includeNesting ? 1 + nesting : 1;
    }
}

internal sealed record Options
{
    public string SourceRoot { get; init; } = Path.Combine(Environment.CurrentDirectory, "Source");

    public string OutputRoot { get; init; } = Path.Combine(Environment.CurrentDirectory, "artifacts", "code-quality", "latest");

    public string? BuildLog { get; init; }

    public string? CoverageRoot { get; init; } = Path.Combine(Environment.CurrentDirectory, "artifacts", "TestResults");

    public static Options Parse(string[] args)
    {
        Options options = new();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string Next()
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"Missing value for {arg}.");
                return args[++i];
            }

            options = arg switch
            {
                "--source-root" => options with { SourceRoot = Next() },
                "--output" => options with { OutputRoot = Next() },
                "--build-log" => options with { BuildLog = Next() },
                "--coverage-root" => options with { CoverageRoot = Next() },
                "--help" or "-h" => throw new ArgumentException(Usage),
                _ => throw new ArgumentException($"Unknown argument '{arg}'.{Environment.NewLine}{Usage}")
            };
        }

        return options;
    }

    private const string Usage = """
        Usage:
          dotnet run --project Source/NexusForever.CodeQuality -- \
            --source-root Source \
            --output artifacts/code-quality/latest \
            --build-log artifacts/code-quality/latest/build.log \
            --coverage-root artifacts/TestResults
        """;
}

internal sealed record SourceFileMetric
{
    public required string Path { get; init; }

    public required string Classification { get; init; }

    public int LineCount { get; init; }

    public int MeaningfulLineCount { get; init; }

    public int FunctionCount { get; init; }

    public int WorstCyclomatic { get; init; }

    public double AverageCyclomatic { get; init; }

    public int WorstCognitive { get; init; }

    public required string SizeBand { get; init; }

    public double ComplexityScore { get; init; }

    public double CleanCodeScore { get; init; }

    public double HotspotScore { get; init; }

    public required string HotspotReason { get; init; }

    public bool HasInlineTests { get; init; }

    public required List<FunctionMetric> Functions { get; init; }
}

internal sealed record FunctionMetric
{
    public required string FilePath { get; init; }

    public required string Name { get; init; }

    public int StartLine { get; init; }

    public int EndLine { get; init; }

    public int Cyclomatic { get; init; }

    public int Cognitive { get; init; }

    public double MaintainabilityIndex { get; init; }

    public int Sloc { get; init; }

    public required string Grade { get; init; }

    public double HotspotScore { get; init; }

    public required string HotspotReason { get; init; }
}

internal sealed record QualityReport
{
    public DateTimeOffset GeneratedAtUtc { get; init; }

    public required string SourceRoot { get; init; }

    public required OverallSummary Overall { get; init; }

    public required List<LaneReport> Lanes { get; init; }

    public required List<string> KnownGaps { get; init; }

    public required InventorySummary Inventory { get; init; }

    public required Dictionary<string, string> Artifacts { get; init; }
}

internal sealed record OverallSummary
{
    public double? Score { get; init; }

    public required string Grade { get; init; }

    public required string Summary { get; init; }

    public bool WouldFailGate { get; init; }
}

internal sealed record InventorySummary
{
    public int SourceFiles { get; init; }

    public int MeasuredFiles { get; init; }

    public int RuntimeFiles { get; init; }

    public int EntrypointFiles { get; init; }

    public int GeneratedFiles { get; init; }

    public int TestFiles { get; init; }

    public int ToolingFiles { get; init; }

    public int ExcludedFromMeasurements { get; init; }

    public int FunctionCount { get; init; }
}

internal sealed record LaneReport
{
    public required string Name { get; init; }

    public required string Status { get; init; }

    public double? Score { get; init; }

    public string? Grade { get; init; }

    public required string Summary { get; init; }

    public required string Formula { get; init; }

    public required List<string> Findings { get; init; }

    public required List<string> Notes { get; init; }

    public Dictionary<string, object?> Metrics { get; init; } = [];
}

internal sealed record BuildSummary
{
    private static readonly Regex WarningRegex = new(@"\bwarning\s+([A-Z]{1,4}\d{3,5}|NU\d{4})\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool BuildSucceeded { get; init; }

    public int WarningCount { get; init; }

    public static BuildSummary? TryLoad(string? buildLog)
    {
        if (string.IsNullOrWhiteSpace(buildLog) || !File.Exists(buildLog))
            return null;

        string text = File.ReadAllText(buildLog);
        int warningCount = text
            .Split('\n')
            .Select(static line => line.Trim())
            .Where(line => WarningRegex.IsMatch(line))
            .Distinct(StringComparer.Ordinal)
            .Count();

        return new BuildSummary
        {
            BuildSucceeded = text.Contains("Build succeeded.", StringComparison.OrdinalIgnoreCase),
            WarningCount = warningCount
        };
    }
}

internal sealed record CoverageSummary
{
    public bool HasCoverage => Files.Count > 0;

    public List<string> Files { get; init; } = [];

    public double LineCoveragePercent { get; init; }

    public double? BranchCoveragePercent { get; init; }

    public int? LinesCovered { get; init; }

    public int? LinesValid { get; init; }

    public static CoverageSummary Load(string? coverageRoot)
    {
        if (string.IsNullOrWhiteSpace(coverageRoot) || !Directory.Exists(coverageRoot))
            return new CoverageSummary();

        List<string> files = Directory
            .EnumerateFiles(coverageRoot, "coverage.cobertura.xml", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (files.Count == 0)
            return new CoverageSummary();

        int linesCovered = 0;
        int linesValid = 0;
        int branchesCovered = 0;
        int branchesValid = 0;
        List<double> lineRates = [];
        List<double> branchRates = [];

        foreach (string file in files)
        {
            XDocument document = XDocument.Load(file);
            XElement? coverage = document.Root;
            if (coverage == null)
                continue;

            int? fileLinesCovered = ReadInt(coverage, "lines-covered");
            int? fileLinesValid = ReadInt(coverage, "lines-valid");
            int? fileBranchesCovered = ReadInt(coverage, "branches-covered");
            int? fileBranchesValid = ReadInt(coverage, "branches-valid");

            if (fileLinesCovered != null && fileLinesValid is > 0)
            {
                linesCovered += fileLinesCovered.Value;
                linesValid += fileLinesValid.Value;
            }
            else if (ReadDouble(coverage, "line-rate") is { } lineRate)
            {
                lineRates.Add(lineRate * 100.0);
            }

            if (fileBranchesCovered != null && fileBranchesValid is > 0)
            {
                branchesCovered += fileBranchesCovered.Value;
                branchesValid += fileBranchesValid.Value;
            }
            else if (ReadDouble(coverage, "branch-rate") is { } branchRate)
            {
                branchRates.Add(branchRate * 100.0);
            }
        }

        double lineCoveragePercent = linesValid > 0
            ? 100.0 * linesCovered / linesValid
            : lineRates.Count == 0 ? 0.0 : lineRates.Average();
        double? branchCoveragePercent = branchesValid > 0
            ? 100.0 * branchesCovered / branchesValid
            : branchRates.Count == 0 ? null : branchRates.Average();

        return new CoverageSummary
        {
            Files = files.Select(p => ProgramPath.NormalizeForReport(coverageRoot, p)).ToList(),
            LineCoveragePercent = Math.Round(lineCoveragePercent, 2, MidpointRounding.AwayFromZero),
            BranchCoveragePercent = branchCoveragePercent == null ? null : Math.Round(branchCoveragePercent.Value, 2, MidpointRounding.AwayFromZero),
            LinesCovered = linesValid > 0 ? linesCovered : null,
            LinesValid = linesValid > 0 ? linesValid : null
        };
    }

    private static int? ReadInt(XElement element, string attributeName)
        => int.TryParse(element.Attribute(attributeName)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : null;

    private static double? ReadDouble(XElement element, string attributeName)
        => double.TryParse(element.Attribute(attributeName)?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : null;
}

internal static class ProgramPath
{
    public static string NormalizeForReport(string root, string path)
        => Path.GetRelativePath(root, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
