#!/usr/bin/env bash
# Install and launch an Android build, asking for everything it needs.
#
# Usage:
#   tools/android-install.sh
#
# Takes no arguments — it prompts for the APK to install and whether to
# stream logcat after launching. Nothing is remembered between runs: every
# prompt is answered from scratch. Choosing an APK source, streaming logcat,
# and the final install confirmation are arrow-key lists (Up/Down, Enter to
# choose) with a Back row to revisit the previous question. A custom APK
# path is typed instead; type "b" at that prompt (it's reserved — never a
# real answer) to go back. A summary is shown before anything runs, as one
# of the arrow-key confirmations.
#
# The package name comes from ProjectSettings' Android applicationIdentifier
# and is never prompted for when that value looks valid. If it's missing or
# malformed, you're asked to type it instead, as the first question.
#
# The APK prompt first asks whether to load from Builds/ or type a custom
# path. Loading from Builds/ lists every .apk found there, oldest first, as
# an arrow-key list; "Load from Builds/" is omitted entirely when none exist.
#
# adb is auto-detected: first from PATH, then by searching Unity Hub's
# installed editors for the copy bundled with the Android module. Override
# the detected path by setting ADB, e.g.:
#   ADB="/custom/path/adb.exe" tools/android-install.sh
#
# The app is always installed and launched. If you choose to stream logcat,
# stop it with Ctrl+C; the app stays installed and running either way.
#
# Build one with tools/android-build.sh.

set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_DIR"

PROJECT_SETTINGS="ProjectSettings/ProjectSettings.asset"

if [ ! -t 0 ]; then
    echo "This script asks questions — run it from a terminal, not a pipe or a hook." >&2
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
    local default="$2"
    local validator="${3:-}"
    local error="${4:-}"
    local show_back="${5:-1}"
    local hint=""
    local reply

    if [ "$show_back" -eq 1 ]; then
        hint=", b"
    fi

    while true; do
        if [ -n "$default" ]; then
            read -r -p "$prompt [Enter for $default$hint]: " reply
            reply="${reply:-$default}"
        elif [ -n "$hint" ]; then
            read -r -p "$prompt [${hint#, }]: " reply
        else
            read -r -p "$prompt: " reply
        fi
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

# ask_confirm <header> <cancel label> <action label>
# ANSWER is "n" for cancel, "y" for the action; returns 1 for Back.
ask_confirm()
{
    local header="$1"
    local cancel_label="$2"
    local action_label="$3"

    if pick_list "$header" "$cancel_label" "$action_label" "Back"; then
        if [ "$PICK_INDEX" -eq 0 ]; then ANSWER="n"; else ANSWER="y"; fi
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
is_package() { printf '%s' "$1" | grep -qE '^com\.[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$'; }
is_file() { [ -f "$1" ]; }

ask_apk_source()
{
    local show_back="${1:-1}"
    local labels=()

    if [ "${#APK_LIST[@]}" -gt 0 ]; then
        labels+=("Load from Builds/")
    fi
    labels+=("Custom path")
    if [ "$show_back" -eq 1 ]; then
        labels+=("Back")
    fi

    if pick_list "APK:" "${labels[@]}"; then
        if [ "${labels[$PICK_INDEX]}" = "Load from Builds/" ]; then
            ANSWER="list"
        else
            ANSWER="path"
        fi
        return 0
    fi
    return 1
}

ask_apk_from_list()
{
    local labels=("${APK_LIST[@]}" "Back")

    if pick_list "APK — choose a build (oldest first):" "${labels[@]}"; then
        ANSWER="${APK_LIST[$PICK_INDEX]}"
        return 0
    fi
    return 1
}

# --- defaults -------------------------------------------------------------------

DEFAULT_PACKAGE="$(awk '/^  applicationIdentifier:/{f=1;next} f&&/^    Android:/{print $2; exit}' "$PROJECT_SETTINGS" | tr -d '[:space:]')"

mapfile -t APK_LIST < <(find Builds -iname '*.apk' -printf '%T@ %p\n' 2>/dev/null | sort -n | cut -d' ' -f2-)

# --- adb detection ---------------------------------------------------------------

if [ -z "${ADB:-}" ]; then
    if command -v adb >/dev/null 2>&1; then
        ADB="$(command -v adb)"
    else
        ADB="$(find "/c/Program Files/Unity/Hub/Editor" -iname 'adb.exe' -path '*platform-tools*' 2>/dev/null | sort -r | head -n1)"
    fi
fi

if [ -z "${ADB:-}" ] || [ ! -x "$ADB" ]; then
    echo "Could not find adb. Install it, add it to PATH, or set ADB explicitly:" >&2
    echo "  ADB=\"<path-to-adb>\" $0" >&2
    exit 1
fi

# --- question sequence ---------------------------------------------------------

# A small state machine, not a straight-line script, so that "b"/Back can
# return to whichever question was actually asked. LAST_APK_STEP remembers
# whether the APK came from the list or a typed path, so backing out of
# logcat returns to that exact screen rather than the source picker.
#
# The package prompt is skipped entirely when ProjectSettings' Android
# applicationIdentifier is valid — apk_source becomes the first question
# (no Back row) in that case. It only appears as a fallback when detection
# fails.

if is_package "$DEFAULT_PACKAGE"; then
    PACKAGE="$DEFAULT_PACKAGE"
    STATE="apk_source"
    APK_SOURCE_SHOW_BACK=0
else
    STATE="package"
    APK_SOURCE_SHOW_BACK=1
fi

LAST_APK_STEP="apk_source"

while true; do
    clear

    case "$STATE" in
        package)
            ask_text "Package name" "" is_package "Package name must look like com.example or com.example.app." 0
            PACKAGE="$ANSWER"
            STATE="apk_source"
            ;;

        apk_source)
            if ask_apk_source "$APK_SOURCE_SHOW_BACK"; then
                if [ "$ANSWER" = "list" ]; then
                    STATE="apk_list"
                else
                    STATE="apk_path"
                fi
            else
                STATE="package"
            fi
            ;;

        apk_list)
            if ask_apk_from_list; then
                APK="$ANSWER"
                LAST_APK_STEP="apk_list"
                STATE="logcat"
            else
                STATE="apk_source"
            fi
            ;;

        apk_path)
            if ask_text "APK path" "" is_file "That path does not exist."; then
                APK="$ANSWER"
                LAST_APK_STEP="apk_path"
                STATE="logcat"
            else
                STATE="apk_source"
            fi
            ;;

        logcat)
            if ask_confirm "Stream logcat after launching?" "No" "Yes"; then
                if [ "$ANSWER" = "y" ]; then
                    STREAM_LOGCAT="true"
                else
                    STREAM_LOGCAT="false"
                fi
                STATE="summary"
            else
                STATE="$LAST_APK_STEP"
            fi
            ;;

        summary)
            SUMMARY_MSG="$(printf 'adb:     %s\napk:     %s\npackage: %s\nlogcat:  %s\n\nInstall with these settings?' \
                "$ADB" "$APK" "$PACKAGE" "$STREAM_LOGCAT")"

            if ask_proceed "$SUMMARY_MSG" "Install"; then
                break
            else
                STATE="logcat"
            fi
            ;;
    esac
done

echo "==> Installing"
"$ADB" install -r "$APK"

echo "==> Launching"
"$ADB" shell monkey -p "$PACKAGE" -c android.intent.category.LAUNCHER 1

if [ "$STREAM_LOGCAT" = "true" ]; then
    echo "==> Streaming logs (Ctrl+C to stop)"
    "$ADB" logcat -s Unity
fi
