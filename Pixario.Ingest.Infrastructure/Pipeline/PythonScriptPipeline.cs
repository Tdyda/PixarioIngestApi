using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pixario.Ingest.Application.Ports;

namespace Pixario.Ingest.Infrastructure.Pipeline;

public sealed class PythonScriptPipeline(IOptions<PythonPipelineOptions> options, ILogger<PythonScriptPipeline> log)
    : IImageProcessingPipeline
{
    private readonly PythonPipelineOptions _opt = options.Value;

    public async Task<IReadOnlyList<string>> RunAsync(PipelineRequest request, CancellationToken ct)
    {
        if (request.InputFiles.Count == 0)
            return [];

        var inputs = request.InputFiles
            .Select(p => Path.IsPathFullyQualified(p)
                ? p
                : Path.GetFullPath(Path.Combine(_opt.UploadRoot, p)))
            .ToList();

        foreach (var input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input) || !Path.IsPathFullyQualified(input))
                throw new InvalidOperationException($"Invalid input file path: '{input}'");

            if (!File.Exists(input))
                throw new FileNotFoundException($"Input file not found: {input}");
        }

        var outputs = inputs.Select(i =>
        {
            var outp = BuildOutputPath(request.JobId, i);
            Directory.CreateDirectory(Path.GetDirectoryName(outp)!);
            return outp;
        }).ToList();
        
        await RunProcessOnceAsync(inputs, outputs, ct);

        return outputs;
    }

    private string BuildOutputPath(Guid jobId, string inputAbsPath)
    {
        var ext = Path.GetExtension(inputAbsPath);
        var name = Path.GetFileNameWithoutExtension(inputAbsPath);

        return Path.GetFullPath(Path.Combine(
            _opt.OutputRoot,
            jobId.ToString(),
            "out",
            $"{name}_processed{ext}"
        ));
    }

    private string BuildArgs(IReadOnlyList<string> inputPaths, IReadOnlyList<string> outputPaths)
    {
        if (inputPaths.Count != outputPaths.Count)
            throw new InvalidOperationException("inputPaths/outputPaths count mismatch.");

        var sb = new StringBuilder();

        sb.Append(EscapeArg(_opt.ScriptPath)).Append(' ');

        for (var i = 0; i < inputPaths.Count; i++)
        {
            sb.Append("-i ").Append(EscapeArg(inputPaths[i])).Append(' ');
            sb.Append("-o ").Append(EscapeArg(outputPaths[i])).Append(' ');
        }

        sb.Append("-s ").Append(EscapeArg(_opt.Strength));

        return sb.ToString();
    }

    private async Task RunProcessOnceAsync(
        IReadOnlyList<string> inputPaths,
        IReadOnlyList<string> outputPaths,
        CancellationToken ct)
    {
        var args = BuildArgs(inputPaths, outputPaths);

        var psi = new ProcessStartInfo
        {
            FileName = _opt.PythonPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _opt.WorkingDirectory
        };

        log.LogInformation("Running pipeline: {FileName} {Args} (wd={Wd})",
            psi.FileName, psi.Arguments, psi.WorkingDirectory);

        using var process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;

        try
        {
            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_opt.TimeoutSeconds));

            await process.WaitForExitAsync(timeoutCts.Token);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                log.LogError(
                    "Pipeline failed exitCode={ExitCode}\nSTDOUT:\n{Stdout}\nSTDERR:\n{Stderr}",
                    process.ExitCode,
                    Truncate(stdout, 8000),
                    Truncate(stderr, 8000));

                throw new PipelineFailedException(
                    process.ExitCode,
                    Truncate(stdout, 8000),
                    Truncate(stderr, 8000));
            }

            log.LogInformation("Pipeline finished OK (exitCode=0).");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException($"Pipeline timed out after {_opt.TimeoutSeconds}s.");
        }
    }

    private static string EscapeArg(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return "\"\"";
        if (arg.IndexOfAny([' ', '\t', '\n', '\r', '"']) < 0) return arg;
        return "\"" + arg.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max] + "\n...<truncated>";

    private static void TryKill(Process p)
    {
        try
        {
            if (!p.HasExited) p.Kill(entireProcessTree: true);
        }
        catch { /* best effort */ }
    }
}

public sealed class PipelineFailedException(int exitCode, string stdout, string stderr)
    : Exception($"Pipeline failed (exitCode={exitCode}).")
{
    public int ExitCode { get; } = exitCode;
    public string Stdout { get; } = stdout;
    public string Stderr { get; } = stderr;
}
