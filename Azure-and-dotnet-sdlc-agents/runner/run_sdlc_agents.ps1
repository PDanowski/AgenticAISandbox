param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("codex", "copilot")]
    [string]$Profile,

    # Optional explicit model name (overrides -ModelPreset).
    # Examples:
    # -Model "gpt-5.4"      -> best quality for complex architecture/review output
    # -Model "gpt-5.4-mini" -> balanced quality/speed (good default)
    # -Model "gpt-4.1-mini" -> faster, lower-cost runs for drafts
    [Parameter(Mandatory = $false)]
    [string]$Model = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("quality", "balanced", "fast")]
    [string]$ModelPreset = "balanced",

    [Parameter(Mandatory = $false)]
    [string]$FeatureFile,

    [Parameter(Mandatory = $false)]
    [string]$FeatureText,

    [Parameter(Mandatory = $false)]
    [string]$OutDir = ""
)

if (-not $env:OPENAI_API_KEY) {
    throw "OPENAI_API_KEY environment variable is required."
}

if ([string]::IsNullOrWhiteSpace($FeatureFile) -eq [string]::IsNullOrWhiteSpace($FeatureText)) {
    throw "Provide exactly one of -FeatureFile or -FeatureText."
}

$scriptPath = Join-Path $PSScriptRoot "run_sdlc_agents.py"
$packRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($OutDir)) {
    $OutDir = Join-Path $packRoot "automations/codex/outbox"
}

$args = @(
    $scriptPath,
    "--profile", $Profile,
    "--model-preset", $ModelPreset,
    "--out-dir", $OutDir
)

if (-not [string]::IsNullOrWhiteSpace($Model)) {
    $args += @("--model", $Model)
}

if (-not [string]::IsNullOrWhiteSpace($FeatureFile)) {
    $args += @("--feature-file", $FeatureFile)
}
else {
    $args += @("--feature-text", $FeatureText)
}

python @args
