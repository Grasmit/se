<#
Render Mermaid diagrams to PNG using Dockerized mermaid-cli.

Prereqs: Docker installed and running. This script mounts the current folder
and renders `architecture.mmd` to `architecture.png`.

Usage: Run from repository root or this folder:
    PS> .\render-diagrams.ps1
#>

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

$input = "architecture.mmd"
$output = "architecture.png"

Write-Host "Rendering $input -> $output using Docker mermaid-cli"

docker run --rm -v ${PWD}:/work -w /work minlag/mermaid-cli -i $input -o $output

if (Test-Path $output) { Write-Host "Rendered: $output" } else { Write-Host "Render failed" }

Pop-Location
