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

    // public async Task RunAsync(PipelineRequest request, CancellationToken ct)
    // {
    //     var normalized = request.InputFiles
    //         .Select(p => Path.IsPathFullyQualified(p)
    //             ? p
    //             : Path.GetFullPath(Path.Combine(_opt.UploadRoot, p)))
    //         .ToList();
    //
    //     foreach (var f in normalized)
    //     {
    //         if (string.IsNullOrWhiteSpace(f) || !Path.IsPathFullyQualified(f))
    //             throw new InvalidOperationException($"Invalid input file path: '{f}'");
    //
    //         if (!File.Exists(f))
    //             throw new FileNotFoundException($"Input file not found: {f}");
    //     }
    //
    //     var args = BuildArgs(request.JobId, normalized);
    //
    //     var psi = new ProcessStartInfo
    //     {
    //         FileName = _opt.PythonPath,
    //         Arguments = args,
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         CreateNoWindow = true,
    //         WorkingDirectory = _opt.WorkingDirectory
    //     };
    //
    //     log.LogInformation("Running pipeline: {FileName} {Args} (wd={Wd})",
    //         psi.FileName, psi.Arguments, psi.WorkingDirectory);
    //
    //     using var process = new Process();
    //     process.StartInfo = psi;
    //     process.EnableRaisingEvents = true;
    //
    //     try
    //     {
    //         process.Start();
    //
    //         var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
    //         var stderrTask = process.StandardError.ReadToEndAsync(ct);
    //
    //         using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    //         timeoutCts.CancelAfter(TimeSpan.FromSeconds(_opt.TimeoutSeconds));
    //
    //         await process.WaitForExitAsync(timeoutCts.Token);
    //
    //         var stdout = await stdoutTask;
    //         var stderr = await stderrTask;
    //
    //         if (process.ExitCode != 0)
    //         {
    //             var outTr = Truncate(stdout, 8000);
    //             var errTr = Truncate(stderr, 8000);
    //             
    //             log.LogError("Pipeline failed exitCode={ExitCode}\nSTDOUT:\n{Stdout}\nSTDERR:\n{Stderr}",
    //                 process.ExitCode, outTr, errTr);
    //             
    //             throw new PipelineFailedException(
    //                 exitCode: process.ExitCode,
    //                 stdout: Truncate(stdout, 8000),
    //                 stderr: Truncate(stderr, 8000));
    //         }
    //
    //         log.LogInformation("Pipeline finished OK (exitCode=0).");
    //     }
    //     catch (OperationCanceledException) when (!ct.IsCancellationRequested)
    //     {
    //         TryKill(process);
    //         throw new TimeoutException($"Pipeline timed out after {_opt.TimeoutSeconds}s.");
    //     }
    // }
    
    private string BuildArgs(string inputPath, string outputPath)
    {
        // python release5.py -i "<in>" -o "<out>" -s 0.5
        return string.Join(" ", new[]
        {
            EscapeArg(_opt.ScriptPath),
            "-i", EscapeArg(inputPath),
            "-o", EscapeArg(outputPath),
            "-s", EscapeArg(_opt.Strength)
        });
    }
    
    public async Task<string> RunAsync(PipelineRequest request, CancellationToken ct)
    {
        var normalized = request.InputFiles
            .Select(p => Path.IsPathFullyQualified(p)
                ? p
                : Path.GetFullPath(Path.Combine(_opt.UploadRoot, p)))
            .ToList();

        foreach (var input in normalized)
        {
            if (!File.Exists(input))
                throw new FileNotFoundException($"Input file not found: {input}");

            var output = BuildOutputPath(request.JobId, input);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);

            await RunProcessOnceAsync(input, output, ct);
            
            return output;
        }

        return "just testing";
    }
    
    private string BuildOutputPath(Guid jobId, string inputAbsPath)
    {
        var ext = Path.GetExtension(inputAbsPath);
        var name = Path.GetFileNameWithoutExtension(inputAbsPath);

        return Path.GetFullPath(Path.Combine(
            _opt.OutputRoot,          // np. ".../uploads"
            jobId.ToString(),
            "out",
            $"{name}_processed{ext}"
        ));
    }


    private async Task RunProcessOnceAsync(string inputPath, string outputPath, CancellationToken ct)
    {
        var args = BuildArgs(inputPath, outputPath);

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

        log.LogInformation("Running pipeline: {FileName} {Args}", psi.FileName, psi.Arguments);

        using var process = new Process { StartInfo = psi };

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
        catch
        {
            /* best effort */
        }
    }
}

public sealed class PipelineFailedException(int exitCode, string stdout, string stderr)
    : Exception($"Pipeline failed (exitCode={exitCode}).")
{
    public int ExitCode { get; } = exitCode;
    public string Stdout { get; } = stdout;
    public string Stderr { get; } = stderr;
}