#!/usr/bin/env bash
# MTP emits both per-project and merged TRX files. Publishing both doubles the test count.
set -euo pipefail
: "${TEST_RESULTS_DIRECTORY:?TEST_RESULTS_DIRECTORY is required}"

shopt -s nullglob
merged_reports=("$TEST_RESULTS_DIRECTORY"/merged/*.trx)
if (( ${#merged_reports[@]} > 0 )); then
    echo "Publishing merged TRX reports, including their coverage attachments."
    echo '##vso[task.setvariable variable=BackendTestResultsPattern]merged/*.trx'
else
    # A failed test process may exit before post-processing; retain any individual reports.
    echo "No merged report found; publishing available per-project reports."
    echo '##vso[task.setvariable variable=BackendTestResultsPattern]**/*.trx'
fi
