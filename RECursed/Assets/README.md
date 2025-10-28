# RECursed

**A first-person horror surveillance game where you monitor a house through security cameras and report supernatural anomalies before your battery runs out.**
---

## 📖 Table of Contents

- [About](#about)
- [Features](#features)
- [Gameplay](#gameplay)
- [Installation](#installation)
- [How to Play](#how-to-play)
- [Game Mechanics](#game-mechanics)
- [Project Structure](#project-structure)
- [Development](#development)
- [Team](#team)
- [System Requirements](#system-requirements)
- [Acknowledgments](#acknowledgments)
- [License](#license)

---

## 📖 About

**RECursed** is a tense, atmospheric horror game developed as part of **COMP602** coursework. Players take on the role of a security monitor watching over a seemingly normal house through a network of surveillance cameras. As the night progresses through 5 increasingly difficult rounds, supernatural anomalies begin to manifest—objects move on their own, items vanish without a trace, strange entities appear, and lights flicker ominously.

Your job is to spot these anomalies, report them correctly through your tablet interface, and survive until 6:00 AM. But be careful—your tablet runs on limited battery power, and every missed anomaly or incorrect report drains your charge. Run out of battery, and you're left defenseless in the dark.

**Core Concept:**  
RECursed combines the tension of surveillance horror games with strategic decision-making under time pressure. Players must balance vigilance with battery conservation while dealing with increasingly frequent paranormal activity.

---

## ✨ Features

### 🎥 Camera-Based Surveillance Gameplay
- Monitor **9 different rooms** through security camera feeds
- Seamlessly switch between cameras using tablet interface
- Real-time anomaly detection system

### 👻 Multiple Anomaly Types
- **MovedObject** - Objects that relocate to different positions
- **DisappearedObject** - Items that vanish from their original locations
- **ExtraObject** - New objects that appear where they shouldn't exist
- **LightFlicker** - Lights that behave erratically or flicker ominously
- **Shadow Entity** - Fleeting dark figures that vanish when directly observed—these entities defy standard anomaly reporting

### 📱 Intuitive Tablet Interface
- Immersive in-game tablet with camera grid view
- Simple anomaly reporting system (select room + anomaly type)
- Visual feedback for correct/incorrect reports

### 🔋 Strategic Battery Management
- Limited battery power adds constant tension
- Battery drains when anomaly timers expire
- Incorrect reports accelerate battery depletion
- Manage resources carefully to survive

### ⏰ Time-Pressure Survival
- Survive from **12:00 AM to 6:00 AM** each round
- Real-time in-game clock display
- Each round lasts approximately 6 minutes of game time

### 🎚️ Progressive Difficulty System
- **5 rounds** with escalating challenge
- Increasing anomaly frequency per round
- Reduced reaction time as rounds progress
- Multiple unique map environments

### 🗺️ Multiple Map Environments
- **Rounds 1-2**: Introductory house layout
- **Rounds 3-4**: Expanded environment with new rooms
- **Round 5**: Final challenge with maximum anomaly density

### 📊 Performance Tracking
- End-of-round summary reports
- Accuracy statistics and performance metrics
- Round completion tracking

---

## 🎮 Gameplay

### Objective

Monitor the house through security cameras and report any supernatural anomalies you observe before their timers expire. Survive all 5 rounds to win the game!

### Rooms to Monitor

1. **Kitchen** - Watch for moved utensils and appliances
2. **Bedroom** - Monitor for disturbed furniture
3. **Living Room** - Check for displaced decorations
4. **Bathroom** - Observe vanishing toiletries
5. **Office** - Track moved documents and supplies
6. **Dining Room** - Notice table setting changes
7. **Hallway** - Detect unusual object placements
8. **Garage** - Watch for tools and other miscellaneous stuff
9. **Ensuite Bedroom** - Check for bedroom objects

### Round Progression

| Round | Duration | Anomaly Count | Spawn Interval | Start Delay  | Difficulty              |
|-------|----------|---------------|----------------|--------------|-------------------------|
| **1** | 6 min    | 3 anomalies   | 67 seconds     | 120 seconds  | ⭐ Easy                |
| **2** | 6 min    | 5 anomalies   | 45 seconds     | 120 seconds  | ⭐⭐ Medium            |
| **3** | 6 min    | 8 anomalies   | 25 seconds     | 120 seconds  | ⭐⭐⭐ Medium-Hard    |
| **4** | 6 min    | 12 anomalies  | 18 seconds     | 120 seconds  | ⭐⭐⭐⭐ Hard         |
| **5** | 6 min    | 15 anomalies  | 15 seconds     | 120 seconds  | ⭐⭐⭐⭐⭐ Very Hard |

### Win/Loss Conditions

**🏆 Victory:**
- Survive all 5 rounds
- Reach 6:00 AM in the final round
- Victory screen displays with main menu option

**💀 Game Over:**
- Battery depletes completely (all 3 segments consumed)
- Too many anomalies expire without being reported
- Lose screen displays with restart option

---

## 🎮 How to Play

### Controls

| Action | Control |
|--------|---------|
| **Move** | WASD |
| **Look Around** | Mouse |
| **Open/Close Tablet** | Q |
| **Select Camera** | Left Click (on camera buttons) |
| **Report Anomaly** | Left Click (select room + type + report) |
| **Pause Game** | P or ESC |

### Gameplay Loop

1. **👁️ Monitor the Cameras**  
   - Press **Q** to open your tablet
   - Cycle through the 9 room cameras using the grid interface
   - Watch carefully for anything unusual

2. **🔍 Spot Anomalies**  
   - Look for objects that have moved, disappeared, or appeared
   - Notice lights flickering or behaving strangely
   - Pay attention to small details—anomalies can be subtle

3. **⚡ Report Quickly**  
   - Each anomaly has a timer bar above it
   - Open the anomaly report menu
   - Select the correct room and anomaly type
   - Submit before the timer expires

4. **🔋 Manage Your Battery**  
   - Incorrect reports drain battery faster
   - Expired anomaly timers also consume battery
   - Monitor your battery level in the UI
   - Conserve power—you need it to survive

5. **⏰ Survive Until 6:00 AM**  
   - Each round lasts from 12:00 AM to 6:00 AM
   - Complete all 5 rounds to win the game

### Tips & Strategies

- 🎯 **Be accurate** - It's better to report one anomaly correctly than multiple incorrectly
- ⏱️ **Watch the timers** - Prioritize anomalies with expiring timers
- 🔍 **Scan methodically** - Develop a systematic pattern for checking rooms
- 🧠 **Remember layouts** - Familiarize yourself with normal room states
- 🔋 **Conserve battery** - Don't guess—only report when you're certain

---

## 💿 Installation

### Prerequisites

- **Unity 2022.3 LTS** or newer
- **Visual Studio 2022** or **JetBrains Rider** (for development)
- **Windows 10/11**, **macOS**, or **Linux**
- Minimum 8 GB RAM recommended

### Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Arvinder26/RECursedProject.git
   cd RECursedProject
   ```

2. **Open in Unity Hub**
   - Launch Unity Hub
   - Click "Add" → "Add project from disk"
   - Navigate to the cloned repository folder
   - Select the folder and open

3. **Load the Starting Scene**
   - In Unity, navigate to: `Assets/Scenes/MainGame.unity`
   - Double-click to open the scene

4. **Press Play**
   - Click the Play button in Unity Editor
   - The game will start from Round 1

### Build Instructions

**For Windows:**
```bash
File → Build Settings → Platform: Windows → Build
```

**For macOS:**
```bash
File → Build Settings → Platform: Mac → Build
```

**For Linux:**
```bash
File → Build Settings → Platform: Linux → Build
```

---

## 🎮 Game Mechanics

### Anomaly System

Each anomaly type has unique behavior and visual characteristics:

#### **MovedObject**
- Objects change position or rotation
- Compare current state with mental reference
- Can be subtle (slight rotations) or obvious (complete relocations)

#### **DisappearedObject**
- Objects become invisible/disabled
- Empty spaces where items should be
- Pay attention to missing familiar objects

#### **ExtraObject**
- New objects spawn where they shouldn't be
- Duplicates of existing items
- Out-of-place objects in unexpected locations

#### **LightFlicker**
- Lights turn on/off repeatedly
- Erratic lighting behavior
- Visible flashing in camera feeds

#### **ShadowEntity**
- Ominous dark figure
- Teleports and stalks you all around
- Unable to Report

### Battery System

- **3 battery segments** total
- Each segment can be consumed independently
- Battery drains when:
  - Anomaly timers expire without being reported
  - Incorrect reports are submitted (in some implementations)
- Battery refills to full at the start of each round

### Timer System

- Each active anomaly displays a timer bar
- Timer duration varies by anomaly type
- When timer reaches zero:
  - Anomaly "expires"
  - Battery drains by 1 segment
  - Visual/audio notification plays

### Round Management

- Automatic round progression
- Scene transitions between rounds
- Round summary displays after each completion
- Battery resets to full between rounds
- Player position resets to starting location

---

## 📁 Project Structure

...

RECursed/
├─ Assets/
│  ├─ Scenes/                          # Game scenes
│  │  ├─ MainGame.unity
│  │  ├─ Round 3 and 4 Scene.unity
│  │  └─ Round 5 Map.unity
│  │
│  ├─ Scripts/                         # C# game scripts (organized by category)
│  │  │
│  │  ├─ TabletScreenUI/               # Tablet interface scripts
│  │  │  ├─ AnomalyTimerUI.cs         # Timer bars and battery drain
│  │  │  ├─ BatteryLossScreen.cs      # Battery depletion UI
│  │  │  ├─ LossScreen.cs             # Game over screen
│  │  │  └─ PauseGameScript.cs        # Pause functionality
│  │  │
│  │  ├─ PlayerModelScripts/           # Player controller scripts
│  │  │  ├─ AnamolySpawner.cs         # Anomaly spawning system
│  │  │  ├─ ChaseAndJumpscare.cs      # Chase mechanic and jumpscares
│  │  │  ├─ DirectionalFootstepLooper.cs # Directional footstep audio
│  │  │  ├─ ProximityScare.cs         # Proximity-based scares
│  │  │  ├─ Round2Script.cs           # Round 2 specific logic
│  │  │  └─ SimpleMove.cs             # Simple movement script
│  │  │
│  │  ├─ Anomaly Scripts/              # Anomaly behavior scripts
│  │  │  ├─ DisappearedAnomaly.cs     # Disappeared object logic
│  │  │  ├─ ExtraObject.cs            # Extra object spawning
│  │  │  ├─ LightFlickerAnomaly.cs    # Light flicker behavior
│  │  │  ├─ MovedObject.cs            # Moved object anomaly logic
│  │  │  └─ WritesOnWalls.cs          # Wall writing anomaly
│  │  │
│  │  ├─ AudioScripts/                 # Audio management scripts
│  │  │  ├─ CaptureAudioPlayed.cs     # Audio capture system
│  │  │  ├─ FootstepSubtitles.cs      # Footstep sound controller
│  │  │  └─ SubtitleUI.cs             # Subtitle data
│  │  │
│  │  ├─ Rounds/                       # Round management
│  │  │  └─ RoundManager.cs           # Round progression & scene management
│  │  │
│  │  ├─ ReportMenu/                   # Anomaly reporting system
│  │  │  ├─ AnomalyCicleButton.cs     # Validates anomaly reports
│  │  │  ├─ AnomalyLog.cs             # Logs reported anomalies
│  │  │  ├─ AnomalyManager.cs         # Manages active anomalies
│  │  │  ├─ AnomalyMenuController.cs  # Difficulty scaling
│  │  │  ├─ ReportBadAnomaly.cs       # Report panel UI logic
│  │  │  ├─ ReportMenuController.cs   # Main reporting interface
│  │  │  └─ SummaryReportManager.cs   # End-of-round statistics
│  │  │
│  │  ├─ MainMenuScripts/              # Main menu and UI
│  │  │  ├─ BrightnessManager.cs      # Brightness settings manager
│  │  │  ├─ BrightnessSlider.cs       # Brightness slider control
│  │  │  ├─ GameClockController.cs    # In-game clock system
│  │  │  ├─ MainMenuHandler.cs        # Main menu controller
│  │  │  └─ PlayerMovement.cs         # Menu navigation
│  │  │
│  │  ├─ CCTV System/                  # Camera system scripts
│  │  │  ├─ CCTVFeedController.cs     # Manages camera feeds
│  │  │  ├─ CCTVScanline.cs           # Camera scanning logic
│  │  │  ├─ PanelOpener.cs            # Camera panel controls
│  │  │  └─ TabletPanelController.cs  # Tablet camera interface
│  │  │
│  │  ├─ ButtonSounds/                 # UI sound effects
│  │  │  └─ ButtonSfx.cs              # Button click sounds
│  │  │
│  │  ├─ PauseMenu/                    # Pause functionality
│  │  │  └─ PauseMenu.cs              # Pause screen controller
│  │  │
│  │  └─ Subtitles/                    # Subtitle system
│  │     ├─ AnomalySubtitles.cs       # Anomaly notification subtitles
│  │     └─ SubtitleUI.cs             # Subtitle display manager
│  │
│  ├─ Interior Prefabs/                # Furnished room prefabs
│  │  ├─ Bathroom/
│  │  ├─ Bedroom/
│  │  ├─ DiningRoom/
│  │  ├─ Hallway/
│  │  ├─ Kitchen/
│  │  ├─ LivingRoom/
│  │  ├─ Office/
│  │  ├─ Study/
│  │  └─ Laundry/
│  │
│  ├─ Animation/                       # Animation files
│  ├─ Audio/                           # Audio clips
│  ├─ Sounds/                          # Sound effects
│  ├─ CCTV Camera/                     # Camera assets and prefabs
│  ├─ PlayerModel/                     # Player character assets
│  ├─ AnomalyModels/                   # Anomaly-specific models
│  ├─ House Blender/                   # House 3D models
│  ├─ Prefabs/                         # General prefabs
│  ├─ Images/                          # Textures and sprites
│  ├─ Fonts/                           # Typography assets
│  ├─ TextMesh Pro/                    # TMP resources
│  ├─ Settings/                        # Unity project settings
│  ├─ IngameTime/                      # Clock system assets
│  ├─ UnitTesting/                     # Unit test files
│  ├─ FootstepPro/                     # Footstep sound system
│  └─ InputSystem/                     # New Input System setup
│
├─ Packages/                           # Unity package dependencies
├─ ProjectSettings/                    # Unity project configuration
├─ README.md                           # This file
└─ .gitignore                          # Git ignore rules
---

## 🛠️ Development

### Key Scripts Explained

#### **Core Systems**

**RoundManager.cs** (`Scripts/Rounds/`)
- Manages round progression (1-5)
- Handles scene transitions between maps
- Spawns anomalies at timed intervals
- Tracks round completion status
- Integrates with GameClockController
- Refills battery between rounds
- Resets player position at round start

**GameClockController.cs** (`Scripts/MainMenuScripts/`)
- Simulates 12:00 AM - 6:00 AM time cycle
- Real-time clock display
- Triggers round end at 6:00 AM
- Integrates with RoundManager

#### **Anomaly Scripts** (`Scripts/Anomaly Scripts/`)

**MovedObject.cs**
- Handles object position and rotation changes
- Context menu workflow for setup
- Tracks original and anomaly transforms
- Reverts when correctly reported

**DisappearanceAnomaly.cs**
- Manages object visibility toggling
- Makes objects invisible during anomaly state
- Restores visibility when reported

**ExtraObject.cs**
- Spawns duplicate or extra objects
- Uses prefab instantiation system
- Context menu for spawn position setup

**LightFlickerAnomaly.cs**
- Controls erratic light behavior
- Toggles lights on/off repeatedly
- Visual indicator of paranormal activity

#### **Camera & Tablet System** (`Scripts/CCTV System/`)

**TabletPanelController.cs**
- Main tablet interface controller
- Camera grid navigation
- Opens/closes with Q key

**CCTVFeedController.cs**
- Manages multiple camera feeds
- Switches between room cameras
- Updates feed displays

**CCTVScanline.cs**
- Camera scanning visual effect
- Scanline overlay for CCTV authenticity
- Visual feedback system

**PanelOpener.cs**
- Controls camera panel UI
- Shows/hides camera interface

#### **UI & Display** (`Scripts/TabletScreenUI/`)

**AnomalyTimerUI.cs**
- Displays timer bars above anomalies
- Detects when timers expire
- Triggers battery consumption
- Visual and audio notifications
- Tracks active anomalies

**BatteryLossScreen.cs**
- Battery depletion UI
- Displays when battery runs out
- Game over state handler

**LossScreen.cs**
- Defeat screen display
- Shows game over message
- Restart/quit options

**PauseGameScript.cs**
- Pauses game state
- Freezes time and input
- Shows pause menu

**SimpleTimer.cs**
- Basic timer utility
- Used across multiple systems
- Countdown functionality

#### **Player Systems** (`Scripts/PlayerModelScripts/`)

**AnamolySpawner.cs**
- Spawns anomalies dynamically
- Controls anomaly placement
- Manages spawn timing

**ChaseAndJumpscare.cs**
- Chase sequence mechanics
- Jumpscare triggers and animations
- Horror event controller

**DirectionalFootstepLooper.cs**
- Directional footstep audio
- Loops footstep sounds based on direction
- Spatial audio positioning

**ProximityScare.cs**
- Proximity-based scare events
- Triggers when player gets close
- Distance-based horror mechanics

**Round2Script.cs**
- Round 2 specific logic
- Custom events for second round
- Round-specific behaviors

**SimpleMove.cs**
- Simple movement implementation
- Basic character controller
- Movement helper script

#### **Reporting System** (`Scripts/ReportMenu/`)

**ReportMenuController.cs**
- Main anomaly reporting interface
- Room and type selection
- Submit button handler

**AnomalyChoiceButton.cs**
- Validates anomaly reports
- Checks if report matches active anomaly
- Determines correct/incorrect reports

**AnomalyManager.cs**
- Manages all active anomalies
- Tracks reported vs unreported
- Coordinates with RoundManager

**AnomalyLog.cs**
- Logs all player reports
- Tracks accuracy statistics
- Used for end-of-round summary

**AnomalyMenuController.cs**
- Anomaly menu UI controller
- Handles menu interactions
- Difficulty scaling interface

**ReportFeedOverlay.cs**
- Report feedback overlay
- Visual confirmation of reports
- Shows correct/incorrect feedback

**SummaryReportManager.cs**
- End-of-round statistics screen
- Shows accuracy, reports, and performance
- Display completion data

#### **Audio System** (`Scripts/AudioScripts/`)

**RoundMusicController.cs**
- Manages background music per round
- Changes music based on tension
- Fades between tracks

**FootstepSubtitles.cs**
- Displays footstep subtitles
- Accessibility feature for audio cues
- Synchronized with footstep sounds

**SubtitleHeartbeatSync.cs**
- Syncs subtitles with heartbeat audio
- Tension-based subtitle timing
- Audio-visual synchronization

**CaptureWhenPlayed.cs**
- Audio capture system
- Records when audio plays
- Audio event logging

**Subtitles.cs**
- Subtitle data structure
- Contains subtitle text and timing
- Used by subtitle display systems

#### **Menu Systems** (`Scripts/MainMenuScripts/` & `Scripts/PauseMenu/`)

**MainMenuHandler.cs**
- Main menu controller
- Start game, settings, quit options
- Scene loading

**PauseMenu.cs**
- Pause functionality
- Resume, settings, quit to menu
- Saves game state

**BrightnessManager.cs**
- Manages brightness settings
- Saves/loads brightness preference
- Applies brightness adjustments

**BrightnessSlider.cs**
- Brightness slider UI control
- Interactive brightness adjustment
- Real-time preview

**PlayerMovement.cs** (in MainMenuScripts)
- Menu navigation movement
- Character movement in menu scenes
- Menu-specific controls

#### **Subtitle System** (`Scripts/Subtitles/`)

**SubtitleUI.cs**
- Central subtitle display manager
- Formats and positions text
- Handles timing

**AnamolySubtitles.cs**
- Subtitle notifications for anomalies
- "Anomaly Detected" messages
- Warning text display

#### **UI Feedback** (`Scripts/ButtonSounds/`)

**ButtonSfx.cs**
- Button click sound effects
- Hover sound feedback
- UI audio cues

### Setting Up Anomalies

Each anomaly script includes **context menu shortcuts** for easy setup:

#### **MovedObject Setup:**
1. Attach `MovedObject.cs` to any GameObject
2. Right-click script header → **"STEP 1: Save Original Transform"**
3. In Scene view, move/rotate object to anomaly position
4. Right-click → **"STEP 2: Copy Current to New Position"**
5. Right-click → **"STEP 3: Return to Original Transform"**
6. Configure room, trigger time, and report window in Inspector

#### **ExtraObject Setup:**
1. Attach `ExtraObject.cs` to the object you want to spawn
2. Right-click → **"STEP 1: Save Original Transform"**
3. Move object to desired spawn position
4. Right-click → **"STEP 2: Copy Transform & Create Prefab"**
5. Right-click → **"STEP 3: Return to Original Transform"**
6. Assign prefab reference in Inspector if needed

### Testing Features

- **Skip Rounds**: Right-click RoundManager → "Skip to Next Round" (in Play mode)
- **Debug Mode**: Enable in RoundManager Inspector for detailed console logs
- **Test Anomalies**: Right-click anomaly scripts for "Test Trigger" option

---

## 👥 Team

### Development Team

| Name | Role | Responsibilities |
|------|------|------------------|
| **Tristan** | Developer | Core gameplay systems, round management, anomaly logic |
| **Kyle** | Product Owner | UI/UX design, player experience, interface programming |
| **Justine** | Scrum Master | Environment design, 3D modeling, anomaly implementation |
| **Arvinder** | Developer | Bug reporting, playtesting, quality assurance |

### Course Information

- **Institution**: [Auckland University of Technology]
- **Course**: COMP602 - Software Development Practice
- **Semester**: [Semester 2 / 2025]
- **Instructor**: [Matthew Kuo]

---

## 💻 System Requirements

### Minimum Requirements

| Component | Specification |
|-----------|---------------|
| **OS** | Windows 10 64-bit / macOS 10.13+ / Ubuntu 20.04+ |
| **Processor** | Intel Core i5-4590 / AMD FX 8350 |
| **Memory** | 8 GB RAM |
| **Graphics** | NVIDIA GTX 960 / AMD Radeon R9 280 |
| **DirectX** | Version 11 |
| **Storage** | 2 GB available space |

### Recommended Requirements

| Component | Specification |
|-----------|---------------|
| **OS** | Windows 10/11 64-bit / macOS 11+ |
| **Processor** | Intel Core i7-8700 / AMD Ryzen 5 3600 |
| **Memory** | 16 GB RAM |
| **Graphics** | NVIDIA GTX 1060 6GB / AMD Radeon RX 580 |
| **DirectX** | Version 12 |
| **Storage** | 2 GB available space (SSD recommended) |

---

## 🏆 Acknowledgments

### Special Thanks

- **Unity Technologies** - For the incredible game engine
- **Blender** - For incredible capabilities of 3D Modelling
- **COMP602 Course Staff** - For guidance and support throughout development

### Assets & Resources

- Unity Asset Store assets (where applicable)
- TextMesh Pro for typography
- Unity Input System for modern input handling
- FootstepPro for audio implementation

---

## 📝 License

This project is developed as part of academic coursework for **COMP602**. All rights reserved for educational purposes.

**Note:** This game is a student project created for educational assessment and is not intended for commercial distribution.

---

## 📞 Support & Contact

For questions about the project:

- **GitHub Issues**: [Report bugs or issues](#)
- **Email**: Kyle - jkn2963@autuni.ac.nz || Tristan - wgv5947@autuni.ac.nz || Arvinder - rrm5999@autuni.ac.nz || Justine - yks9204@autuni.ac.nz (#)
- **Trello Board**: [View development progress] URL - https://trello.com/b/uMkQLDoW/2025s2w202recursed (#)

---

**Made by the RECursed Development Team**

*"Watch carefully. Report quickly. Survive the night."*
