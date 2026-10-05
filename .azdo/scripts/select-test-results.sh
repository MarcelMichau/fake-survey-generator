#!/usr/bin/env bash
# MTP emits both per-project and merged reports. Select each type independently to avoid duplication.
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

merged_coverage=("$TEST_RESULTS_DIRECTORY"/merged/*.coverage)
if (( ${#merged_coverage[@]} > 0 )); then
    echo "Publishing the merged coverage report explicitly to the pipeline coverage UI."
    echo '##vso[task.setvariable variable=BackendCoveragePattern]merged/*.coverage'
else
    # MTP writes original per-project coverage files directly in the results directory.
    # Do not recurse: TRX attachment folders also contain copies of these same files.
    echo "No merged coverage found; publishing available per-project coverage reports."
    echo '##vso[task.setvariable variable=BackendCoveragePattern]*.coverage'
fi
