namespace Pixario.Ingest.Infrastructure.Pipeline;

public sealed class PythonPipelineOptions
{
    public string PythonPath { get; init; } = "/home/tomek/miniconda3/envs/sdxl-inpaint/bin/python";
    public string WorkingDirectory { get; init; } = "/home/tomek/dev/projects/sdxl";
    public string ScriptPath { get; init; } = "/home/tomek/dev/projects/sdxl/release6.py";
    public int TimeoutSeconds { get; init; } = 1800;
    public string UploadRoot { get; init; } = "/home/tomek/dev/projects/PixarioIngestApi/Pixario.Ingest.Api";
    public string OutputRoot { get; init; } = "uploads"; // albo osobny katalog
    public string Strength { get; init; } = "0.5"; // lub float
}