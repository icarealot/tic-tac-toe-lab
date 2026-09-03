#!/usr/bin/env bash
# Build the Android player, asking for everything it needs.
#
# Usage:
#   tools/android-build.sh
#
# Takes no arguments — it prompts for the export type, version, build number,
# Scripting Define Symbols, and the output file name. Nothing is remembered
# between runs: every prompt is answered from scratch. Export type and every
# confirmation are arrow-key lists (Up/Down, Enter to choose) with a Back row
# to revisit the previous question — except the very first prompt, which has
# nowhere to go back to. Version, build number, and Scripting Define Symbols
# are typed instead; type "b" at those (it's reserved — never a real answer)
# to go back. The version and build number prompts print existing
# Builds/Android/*.<export type> files above the prompt, oldest first, purely
# for reference — nothing is picked from it. The output name defaults to a
# template, <ProductName>_<version>(<buildNumber>), or you can pick to type
# your own base name instead — the .<export type> extension is always
# appended for you. A summary is shown before anything builds, as one of
# these arrow-key confirmations. Every build is a release build.
#
# Signing is the exception: it comes from .env in the project root, so passwords
# are never typed. Leave the file out, or leave ANDROID_KEYSTORE_PATH unset, and
# the build is signed with Unity's debug keystore.
#
#   ANDROID_KEYSTORE_PATH=/c/Users/you/keys/app.keystore
#   ANDROID_KEYSTORE_PASSWORD=...
#   ANDROID_KEY_ALIAS=app
#   ANDROID_KEY_ALIAS_PASSWORD=...      # optional, defaults to the keystore password
#
# The artifact lands in Builds/Android/<ProductName>_<version>(<buildNumber>).apk
# (or .aab). Older artifacts are left where they are. If that exact file already
# exists you are asked before anything is overwritten, and before Unity starts.
# On success, Explorer opens with the built file selected.
#
# Every setting is written into ProjectSettings.asset and left there, keystore
# path and alias included, so each build leaves a git diff that is yours to
# commit or discard. Passwords reach Unity through the environment; on the path
# that drives an already-open Editor they travel as command-line arguments and
# are visible in the process list.
#
# Two ways in, picked automatically:
#   - Unity already open  -> a detached `android_build` Pipeline command runs the
#                            build inside that Editor.
#   - Unity closed        -> `unity build --execute-method` spawns a batch Editor.
#
# Output is quiet either way. On failure you get the log path and its tail.
#
# Install and launch what you built with tools/android-install.sh.

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_DIR"

BUILD_METHOD="Icarealot.UnityTools.AndroidBuilder.BuildFromCommandLine"
PIPELINE_COMMAND="android_build"
PROJECT_SETTINGS="ProjectSettings/ProjectSettings.asset"
ENV_FILE=".env"

if [ ! -t 0 ]; then
    echo "This script asks questions — run it from a terminal, not a pipe or a hook." >&2
    exit 1
fi

if ! command -v unity >/dev/null 2>&1; then
    echo "The unity CLI is not on PATH — install it and reopen your shell." >&2
    exit 1
fi

# A picker hides the cursor while it's redrawing; this guarantees it comes
# back no matter how the script exits (including Ctrl+C mid-picker).
trap 'printf "\033[?25h"' EXIT

# --- prompts ------------------------------------------------------------------

# Every prompt lets you revisit the previous question — arrow-key pickers via
# a "Back" row, typed prompts via the reserved answer "b" (it can never be a
# real answer). Each ask_* function returns 0 with the answer in ANSWER, or
# returns 1 to mean "go back".

ask_text()
{
    local prompt="$1"
    local validator="${2:-}"
    local error="${3:-}"
    local show_back="${4:-1}"
    local hint=""
    local reply

    if [ "$show_back" -eq 1 ]; then
        hint=" [b]"
    fi

    while true; do
        read -r -p "$prompt$hint: " reply
        if [ "$show_back" -eq 1 ] && [ "$reply" = "b" ]; then
            return 1
        fi
        if [ -z "$validator" ] || "$validator" "$reply"; then
            ANSWER="$reply"
            return 0
        fi
        echo "  $error" >&2
    done
}

# Reads one keypress into KEY: "UP", "DOWN", "ENTER", or "OTHER". An escape
# sequence's remaining two bytes are read with a short timeout so a lone Esc
# (no Up/Down following) still resolves to "OTHER" instead of hanging.
read_key()
{
    local first rest

    IFS= read -rsn1 first || true

    if [ -z "$first" ]; then
        KEY="ENTER"
        return
    fi

    if [ "$first" = $'\x1b' ]; then
        IFS= read -rsn2 -t 0.05 rest || true
        case "$rest" in
            '[A') KEY="UP" ;;
            '[B') KEY="DOWN" ;;
            *) KEY="OTHER" ;;
        esac
        return
    fi

    KEY="OTHER"
}

# Arrow-key list: $1 is the header (may be multi-line), the rest are row
# labels. If the last label is "Back", selecting it returns 1 instead of
# setting PICK_INDEX. Up/Down wraps; Enter confirms.
pick_list()
{
    local header="$1"
    shift
    local labels=("$@")
    local count=${#labels[@]}
    local selected=0
    local back_index=-1
    local i

    if [ "${labels[$((count - 1))]}" = "Back" ]; then
        back_index=$((count - 1))
    fi

    printf '\033[?25l'

    while true; do
        clear
        printf '%s\n\n' "$header"

        for i in "${!labels[@]}"; do
            if [ "$i" -eq "$selected" ]; then
                echo "> ${labels[$i]}"
            else
                echo "  ${labels[$i]}"
            fi
        done

        read_key
        case "$KEY" in
            UP) selected=$(( (selected - 1 + count) % count )) ;;
            DOWN) selected=$(( (selected + 1) % count )) ;;
            ENTER)
                printf '\033[?25h'
                if [ "$selected" -eq "$back_index" ]; then
                    return 1
                fi
                PICK_INDEX=$selected
                return 0
                ;;
        esac
    done
}

ask_export_type()
{
    if pick_list "Export type:" "apk" "aab"; then
        if [ "$PICK_INDEX" -eq 0 ]; then ANSWER="apk"; else ANSWER="aab"; fi
        return 0
    fi
    return 1
}

ask_naming_mode()
{
    local template_name="$1"

    if pick_list "Output name:" "Use template name ($template_name)" "Enter custom name" "Back"; then
        if [ "$PICK_INDEX" -eq 0 ]; then ANSWER="template"; else ANSWER="custom"; fi
        return 0
    fi
    return 1
}

# ask_proceed <header> <action label>
# Returns 0 to proceed with the action, 1 for Back.
ask_proceed()
{
    local header="$1"
    local action_label="$2"

    pick_list "$header" "$action_label" "Back"
}

is_nonempty() { [ -n "$1" ]; }
is_integer() { printf '%s' "$1" | grep -qE '^[0-9]+$'; }
is_valid_filename() { [ -n "$1" ] && ! printf '%s' "$1" | grep -qE '[/\:*?"<>|]'; }

# Reference only — printed above the version/build-number prompts so you can
# see what's already there without picking from it.
list_existing_builds()
{
    local list

    mapfile -t list < <(find Builds/Android -iname "*.${EXPORT_TYPE}" -printf '%T@ %p\n' 2>/dev/null | sort -n | cut -d' ' -f2-)

    if [ "${#list[@]}" -eq 0 ]; then
        echo "No existing .$EXPORT_TYPE builds under Builds/Android."
    else
        echo "Existing .$EXPORT_TYPE builds under Builds/Android (oldest first):"
        local b
        for b in "${list[@]}"; do
            echo "  $b"
        done
    fi
    echo
}

# --- signing (from .env, not prompted) -----------------------------------------

# Read one KEY=value line rather than sourcing the file, so a stray command in
# .env is never executed.
read_env()
{
    local name="$1"
    local line

    if [ ! -f "$ENV_FILE" ]; then
        return 0
    fi

    line="$(grep -E "^[[:space:]]*(export[[:space:]]+)?${name}=" "$ENV_FILE" | tail -n1 || true)"

    if [ -z "$line" ]; then
        return 0
    fi

    line="${line#*=}"
    line="$(printf '%s' "$line" | tr -d '\r')"
    line="${line%\"}"
    line="${line#\"}"
    line="${line%\'}"
    line="${line#\'}"

    printf '%s' "$line"
}

KEYSTORE_PATH="$(read_env ANDROID_KEYSTORE_PATH)"
KEYSTORE_PASSWORD="$(read_env ANDROID_KEYSTORE_PASSWORD)"
KEY_ALIAS="$(read_env ANDROID_KEY_ALIAS)"
KEY_ALIAS_PASSWORD="$(read_env ANDROID_KEY_ALIAS_PASSWORD)"

if [ -n "$KEYSTORE_PATH" ]; then
    if [ ! -f "$KEYSTORE_PATH" ]; then
        echo "ANDROID_KEYSTORE_PATH in $ENV_FILE points at nothing: $KEYSTORE_PATH" >&2
        exit 1
    fi

    if [ -z "$KEYSTORE_PASSWORD" ]; then
        echo "$ENV_FILE has ANDROID_KEYSTORE_PATH but no ANDROID_KEYSTORE_PASSWORD." >&2
        exit 1
    fi

    if [ -z "$KEY_ALIAS" ]; then
        echo "$ENV_FILE has ANDROID_KEYSTORE_PATH but no ANDROID_KEY_ALIAS." >&2
        exit 1
    fi

    if [ -z "$KEY_ALIAS_PASSWORD" ]; then
        KEY_ALIAS_PASSWORD="$KEYSTORE_PASSWORD"
    fi
fi

PRODUCT_NAME="$(grep -m1 '^  productName:' "$PROJECT_SETTINGS" \
    | sed -e 's/^  productName:[[:space:]]*//' -e "s/^'//" -e "s/'$//" \
    | tr -d '[:space:]')"

if [ -z "$PRODUCT_NAME" ]; then
    echo "Could not read productName from $PROJECT_SETTINGS." >&2
    exit 1
fi

# --- question sequence ---------------------------------------------------------

# A small state machine, not a straight-line script, so that "b" can return
# to whichever question was actually asked — skipping the states that only
# show up conditionally (custom_name, keystore_fallback, overwrite).

STATE="export_type"

while true; do
    clear

    case "$STATE" in
        export_type)
            ask_export_type
            EXPORT_TYPE="$ANSWER"
            STATE="version"
            ;;

        version)
            list_existing_builds
            if ask_text "Version (e.g. 0.1.0)" is_nonempty "A version is required."; then
                VERSION="$ANSWER"
                STATE="build_number"
            else
                STATE="export_type"
            fi
            ;;

        build_number)
            list_existing_builds
            if ask_text "Build number (integer)" is_integer "A whole number is required."; then
                BUILD_NUMBER="$ANSWER"
                STATE="naming"
            else
                STATE="version"
            fi
            ;;

        naming)
            if ask_naming_mode "${PRODUCT_NAME}_${VERSION}(${BUILD_NUMBER})"; then
                if [ "$ANSWER" = "template" ]; then
                    OUTPUT_NAME="${PRODUCT_NAME}_${VERSION}(${BUILD_NUMBER})"
                    STATE="defines"
                else
                    STATE="custom_name"
                fi
            else
                STATE="build_number"
            fi
            ;;

        custom_name)
            if ask_text "Output name (no extension)" is_valid_filename 'Name must be non-empty and not contain / \ : * ? " < > |'; then
                OUTPUT_NAME="$ANSWER"
                STATE="defines"
            else
                STATE="naming"
            fi
            ;;

        defines)
            if ask_text "Scripting Define Symbols (semicolon-separated, blank for none)"; then
                DEFINES="$ANSWER"

                if [ -z "$KEYSTORE_PATH" ]; then
                    STATE="keystore_fallback"
                else
                    STATE="overwrite_check"
                fi
            else
                STATE="naming"
            fi
            ;;

        keystore_fallback)
            # Falling back to the debug keystore is a decision, not a default: a
            # debug-signed app cannot be updated over a release-signed one, and
            # Play refuses it outright.
            if [ "$EXPORT_TYPE" = "aab" ]; then
                KEYSTORE_MSG="No keystore in $ENV_FILE — this .aab would be debug-signed and Play will reject it."
            else
                KEYSTORE_MSG="No keystore in $ENV_FILE — this build would be signed with Unity's debug keystore."
            fi

            if ask_proceed "$KEYSTORE_MSG" "Build with debug keystore"; then
                STATE="overwrite_check"
            else
                STATE="defines"
            fi
            ;;

        overwrite_check)
            OUTPUT_PATH="Builds/Android/${OUTPUT_NAME}.${EXPORT_TYPE}"

            if [ -e "$OUTPUT_PATH" ]; then
                STATE="overwrite"
            else
                STATE="summary"
            fi
            ;;

        overwrite)
            if ask_proceed "$OUTPUT_PATH already exists — overwrite it?" "Overwrite"; then
                STATE="summary"
            else
                if [ -z "$KEYSTORE_PATH" ]; then
                    STATE="keystore_fallback"
                else
                    STATE="defines"
                fi
            fi
            ;;

        summary)
            SUMMARY_MSG="$(printf 'output:  %s\nversion: %s (%s)\ndefines: %s\nbuild:   release\nsigning: %s\n\nBuild with these settings?' \
                "$OUTPUT_PATH" "$VERSION" "$BUILD_NUMBER" "${DEFINES:-<none>}" "${KEYSTORE_PATH:-debug keystore}")"

            if ask_proceed "$SUMMARY_MSG" "Build"; then
                break
            else
                STATE="defines"
            fi
            ;;
    esac
done

mkdir -p "$(dirname "$OUTPUT_PATH")"
rm -f "$OUTPUT_PATH"

# --- build --------------------------------------------------------------------

if unity status --json --no-banner 2>/dev/null | grep -q '"ready"'; then
    echo "==> Building in the open Editor"

    # Only non-empty arguments are passed: an empty value shifts the parser onto the
    # next token and silently binds it to the wrong parameter. Everything sits
    # after `--` because the CLI would otherwise claim --version for itself and
    # print its own version.
    COMMAND_ARGS=(--outputPath "$OUTPUT_PATH" --version "$VERSION" --buildNumber "$BUILD_NUMBER")

    if [ -n "$DEFINES" ]; then
        COMMAND_ARGS+=(--defines "$DEFINES")
    fi

    if [ -n "$KEYSTORE_PATH" ]; then
        COMMAND_ARGS+=(--keystorePath "$KEYSTORE_PATH")
        COMMAND_ARGS+=(--keystorePassword "$KEYSTORE_PASSWORD")
        COMMAND_ARGS+=(--keyAlias "$KEY_ALIAS")
        COMMAND_ARGS+=(--keyAliasPassword "$KEY_ALIAS_PASSWORD")
    fi

    set +e
    DETACH_OUTPUT="$(unity command --detach "$PIPELINE_COMMAND" --json --no-banner -- \
        "${COMMAND_ARGS[@]}" 2>&1)"
    DETACH_STATUS=$?
    set -e

    JOB_ID="$(printf '%s' "$DETACH_OUTPUT" \
        | grep -oE '"jobId"[[:space:]]*:[[:space:]]*"[^"]+"' \
        | head -n1 \
        | sed -E 's/.*"([^"]+)"$/\1/' || true)"

    if [ $DETACH_STATUS -ne 0 ] || [ -z "$JOB_ID" ]; then
        echo "Could not start the build in the open Editor." >&2
        printf '%s\n' "$DETACH_OUTPUT" >&2
        exit 1
    fi

    echo "job:     $JOB_ID"
    echo "Building…"

    set +e
    BUILD_OUTPUT="$(unity job wait "$JOB_ID" --timeout 0 --json --no-banner 2>&1)"
    BUILD_STATUS=$?
    set -e

    BUILD_LOG=""
else
    echo "==> Building in a batch Editor"
    echo "Building…"

    BUILD_LOG="Logs/android-build-$(date +%Y%m%d-%H%M%S).log"

    export ANDROID_OUTPUT_PATH="$OUTPUT_PATH"
    export ANDROID_VERSION="$VERSION"
    export ANDROID_BUILD_NUMBER="$BUILD_NUMBER"
    export ANDROID_DEFINES="$DEFINES"
    export ANDROID_KEYSTORE_PATH="$KEYSTORE_PATH"
    export ANDROID_KEYSTORE_PASSWORD="$KEYSTORE_PASSWORD"
    export ANDROID_KEY_ALIAS="$KEY_ALIAS"
    export ANDROID_KEY_ALIAS_PASSWORD="$KEY_ALIAS_PASSWORD"

    set +e
    BUILD_OUTPUT="$(unity build . --no-banner \
        --target Android \
        --execute-method "$BUILD_METHOD" \
        --output-path "$OUTPUT_PATH" \
        --log-file "$BUILD_LOG" \
        --no-tail 2>&1)"
    BUILD_STATUS=$?
    set -e
fi

# --- verdict ------------------------------------------------------------------

if [ $BUILD_STATUS -eq 0 ] && [ -e "$OUTPUT_PATH" ]; then
    echo
    echo "Built $OUTPUT_PATH"

    if command -v cygpath >/dev/null 2>&1; then
        WIN_OUTPUT_PATH="$(cygpath -w "$PROJECT_DIR/$OUTPUT_PATH")"
        # explorer.exe routinely exits non-zero even when it opens fine — its
        # status is not a signal of anything, so it must not affect ours.
        explorer.exe /select,"$WIN_OUTPUT_PATH" >/dev/null 2>&1 || true
    fi

    exit 0
fi

echo >&2
echo "Build failed." >&2
printf '%s\n' "$BUILD_OUTPUT" >&2

if [ -n "$BUILD_LOG" ] && [ -f "$BUILD_LOG" ]; then
    echo >&2
    echo "Last 40 lines of $BUILD_LOG:" >&2
    tail -n 40 "$BUILD_LOG" >&2
elif [ -z "$BUILD_LOG" ]; then
    echo >&2
    echo "The build ran inside the open Editor — see its Editor.log for the reason." >&2
fi

exit 1
