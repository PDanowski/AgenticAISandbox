param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("codex", "copilot")]
    [string]$Profile,

    [Parameter(Mandatory = $false)]
    [string]$Model = "gpt-4.1",

    [Parameter(Mandatory = $false)]
    [string]$FeatureFile,

    [Parameter(Mandatory = $false)]
    [string]$FeatureText,

    [Parameter(Mandatory = $false)]
    [string]$OutDir = "GitHub/automations/codex/outbox"
)

if (-not $env:OPENAI_API_KEY) {
    throw "OPENAI_API_KEY environment variable is required."
}

if ([string]::IsNullOrWhiteSpace($FeatureFile) -eq [string]::IsNullOrWhiteSpace($FeatureText)) {
    throw "Provide exactly one of -FeatureFile or -FeatureText."
}

$scriptPath = Join-Path $PSScriptRoot "run_sdlc_agents.py"

$args = @(
    $scriptPath,
    "--profile", $Profile,
    "--model", $Model,
    "--out-dir", $OutDir
)

if (-not [string]::IsNullOrWhiteSpace($FeatureFile)) {
    $args += @("--feature-file", $FeatureFile)
}
else {
    $args += @("--feature-text", $FeatureText)
}

python @args
