#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"

if command -v pwsh >/dev/null 2>&1; then
  exec pwsh -NoLogo -NoProfile -File "${script_dir}/quality.ps1" "$@"
fi

task="all"
configuration="Release"
skip_build="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --task)
      task="$2"
      shift 2
      ;;
    --configuration|-c)
      configuration="$2"
      shift 2
      ;;
    --skip-build)
      skip_build="true"
      shift
      ;;
    --no-progress)
      shift
      ;;
    all|build|test|report|format)
      task="$1"
      shift
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 2
      ;;
  esac
done

solution="${repo_root}/Source/NexusForever.sln"
quality_project="${repo_root}/Source/NexusForever.CodeQuality/NexusForever.CodeQuality.csproj"
report_root="${repo_root}/artifacts/code-quality/latest"
test_results_root="${repo_root}/artifacts/TestResults"
build_log="${report_root}/build.log"

mkdir -p "${report_root}" "${test_results_root}"

run_build() {
  echo "Building ${solution} (${configuration})..."
  set +e
  dotnet build "${solution}" -c "${configuration}" --nologo 2>&1 | tee "${build_log}"
  status="${PIPESTATUS[0]}"
  set -e
  if [[ "${status}" -ne 0 ]]; then
    echo "dotnet build failed with exit code ${status}" >&2
    exit "${status}"
  fi
}

has_test_projects() {
  find "${repo_root}/Source" -name "*.csproj" -print | while read -r project; do
    base="$(basename "${project}" .csproj)"
    if [[ "${base}" =~ Tests?$ ]] || grep -Eq "<IsTestProject>[[:space:]]*true[[:space:]]*</IsTestProject>" "${project}"; then
      echo "${project}"
    fi
  done
}

run_tests() {
  mapfile -t tests < <(has_test_projects)
  if [[ "${#tests[@]}" -eq 0 ]]; then
    echo "No test projects detected; skipping dotnet test and marking coverage unavailable in the report."
    return
  fi

  echo "Running tests with coverage collection..."
  dotnet test "${solution}" \
    -c "${configuration}" \
    --no-build \
    --results-directory "${test_results_root}" \
    --collect:"XPlat Code Coverage" \
    --settings "${repo_root}/eng/CodeCoverage.runsettings"
}

run_report() {
  echo "Generating C# quality report..."
  dotnet run --project "${quality_project}" -c "${configuration}" -- \
    --source-root "${repo_root}/Source" \
    --output "${report_root}" \
    --build-log "${build_log}" \
    --coverage-root "${test_results_root}"
}

run_format() {
  echo "Checking dotnet formatting without changing files..."
  dotnet format "${solution}" --verify-no-changes --verbosity minimal
}

case "${task}" in
  build)
    run_build
    ;;
  test)
    if [[ "${skip_build}" != "true" ]]; then
      run_build
    fi
    run_tests
    ;;
  report)
    run_report
    ;;
  format)
    run_format
    ;;
  all)
    if [[ "${skip_build}" != "true" ]]; then
      run_build
    fi
    run_tests
    run_report
    ;;
  *)
    echo "Unknown task: ${task}" >&2
    exit 2
    ;;
esac

echo "Quality task '${task}' complete. Report root: ${report_root}"
