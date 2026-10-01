# Tic Tac Toe Lab

Tic Tac Toe Lab is a Unity 6 learning project for practicing automated game testing with a coding agent. A small, playable tic-tac-toe game serves as the system under test:

- **PvP:** two people play on the same device.
- **PvE:** the player uses X against an Amateur or Professional bot using O.
- **EditMode tests:** cover deterministic rules, state transitions, bots, and other engine-independent behavior.
- **PlayMode tests:** cover Unity integration, lifecycle behavior, prefab wiring, UI flows, and critical player journeys.

## Prerequisites

- Unity Editor **6000.3.18f1**

## Setup

1. Clone the repository:
2. In Unity Hub, select **Add > Add project from disk** and choose the repository directory.
3. Open the project with Unity **6000.3.18f1**.

## Running the game

1. Open `Assets/_TicTacToeLab/Scenes/Main.unity`.
2. Enter Play Mode.
3. From Home, choose PvP or PvE. For PvE, choose an Amateur or Professional bot.

## Running tests

### Unity Test Runner

1. Open **Window > General > Test Runner**.
2. Select the **EditMode** tab and click **Run All** to run the deterministic test suite.
3. Select the **PlayMode** tab and click **Run All** to run the Unity integration and player-journey suite.

### Command line

Close any Unity Editor instance using this project before running tests from the command line. Set `UNITY_EDITOR` to the Unity executable installed on your machine. Test results and logs are written to `TestResults/`.

#### Bash

```bash
export UNITY_EDITOR="/path/to/Unity"
mkdir -p TestResults

"$UNITY_EDITOR" -batchmode -nographics \
  -projectPath "$(pwd)" \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/TestResults/editmode.xml" \
  -logFile "$(pwd)/TestResults/editmode.log"

"$UNITY_EDITOR" -batchmode -nographics \
  -projectPath "$(pwd)" \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/TestResults/playmode.xml" \
  -logFile "$(pwd)/TestResults/playmode.log"
```

#### PowerShell

```powershell
$env:UNITY_EDITOR = "C:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe"
$project = (Get-Location).Path
$results = Join-Path $project "TestResults"
New-Item -ItemType Directory -Force $results | Out-Null

& $env:UNITY_EDITOR -batchmode -nographics `
  -projectPath $project `
  -runTests -testPlatform EditMode `
  -testResults "$results/editmode.xml" `
  -logFile "$results/editmode.log"

& $env:UNITY_EDITOR -batchmode -nographics `
  -projectPath $project `
  -runTests -testPlatform PlayMode `
  -testResults "$results/playmode.xml" `
  -logFile "$results/playmode.log"
```

## Project structure

```text
Assets/_TicTacToeLab/
├── Prefabs/                 Gameplay, service, and UI prefabs
├── Scenes/Main.unity        Playable scene
├── Scripts/Runtime/         Application and game code
├── Scripts/Test/
│   ├── EditModeTests/       Deterministic unit tests
│   └── PlayModeTests/       Integration and E2E tests
└── URP/                     Render pipeline assets
docs/                        Coding and testing standards
CONTEXT.md                   Game-domain glossary
```

Runtime code belongs to `TicTacToeLab.Runtime`; tests are separated by the Unity behavior they need to exercise.

## License

[MIT](LICENSE)
