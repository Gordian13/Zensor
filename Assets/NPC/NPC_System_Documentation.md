# NPC System Documentation (Current Progress)

## Overview

This NPC system is designed as a modular, expandable Unity NPC framework focused entirely on NPC-side logic.

Current implemented features:

- Patrol movement using NavMeshAgent
- Mood/state system
- Event/reaction system
- Hover highlighting
- Right-click interaction menu
- Modular interactions using ScriptableObjects
- Dialogue window UI
- Extensible architecture for future dialogue trees and custom behaviors

---

# Folder Structure

Recommended structure:

```text
Assets
└── NPCs
    ├── Prefabs
    ├── Scripts
    ├── UI
    ├── Materials
    └── ScriptableObjects
        ├── Profiles
        └── Interactions
```

---

# Prefab Structure

Current NPC prefab structure:

```text
NPC_Base
├── NavMeshAgent
├── NPCController
├── NPCHoverHighlight
├── Visual
│   ├── Mesh Renderer
│   └── Capsule Collider
└── InteractionTrigger
    └── Capsule Collider
```

Notes:

- `NPC_Base` stays at world ground level.
- `Visual` is offset upward visually.
- `NavMeshAgent` is attached to `NPC_Base`, NOT the visual object.
- `InteractionTrigger` is used for mouse raycast interaction.

---

# Required Components

## NPC_Base

Must contain:

- NavMeshAgent
- NPCController
- NPCHoverHighlight

## Visual

Must contain:

- Renderer
- Collider

---

# NavMeshAgent Setup

Recommended values:

```text
Speed:             controlled by mood
Angular Speed:     120
Acceleration:      8
Stopping Distance: 0.5
Radius:            0.5
Height:            2
Base Offset:       0
```

Important:

- `Base Offset` must stay `0`
- Parent object (`NPC_Base`) should remain at `Y = 0`

---

# Patrol System

NPCs patrol between assigned patrol points.

Patrol points are simple empty GameObjects placed in the scene.

Example:

```text
PatrolPoints
├── PatrolPoint_01
├── PatrolPoint_02
├── PatrolPoint_03
└── PatrolPoint_04
```

Assigned in:

```text
NPCController
→ Patrol Points
```

Current behavior supports:

- Random patrol
- Sequential patrol
- Avoiding repeated points
- Wait time at points

---

# Mood System

## Supported Moods

```text
Normal
Moody
Raged
```

Each mood controls:

- Movement speed
- Wait time
- Dialogue flavor
- Debug gizmo color

Mood data is stored in:

```text
NPCProfile
```

---

# NPC Profiles

NPC configuration is stored using ScriptableObjects.

Location:

```text
Assets/NPCs/ScriptableObjects/Profiles
```

Created via:

```text
Create → NPC → Profile
```

Current profile contains:

- NPC Name
- Hover Color
- Mood Settings
- Reaction Rules
- Interactions
- Default Dialogue

---

# Reaction Rule System

The NPC can react to external gameplay contexts.

Architecture:

```text
External System
→ creates NPCReactionContext
→ NPCController.ReactTo(context)
→ NPC checks rules
→ NPC reacts
```

Possible reactions:

- Change mood
- Say dialogue text
- Block external action

Example:

```text
Event Type: RecordPlayed
Object Id: record_forbidden_01
Result Mood: Raged
Block Action: true
```

---

# Hover Highlight System

The NPC supports hover highlighting using centralized raycast detection.

Architecture:

```text
NPCHoverDetector
→ raycasts from camera
→ finds NPCHoverHighlight
→ applies highlight color
```

Hover color comes from:

```text
NPCProfile.hoverColor
```

---

# Interaction System

Right-clicking the NPC opens an interaction menu.

Architecture:

```text
Right Click
→ NPCInteractionHandler
→ NPCInteractionMenu
→ NPCInteraction execution
```

Interactions are modular ScriptableObjects.

Current implementation:

```text
NPCTalkInteraction
```

Future possibilities:

- Dialogue Trees
- Conditional interactions
- Quest interactions
- Custom scripted actions

---

# Dialogue Window

Current dialogue UI:

```text
NPCDialogueWindow
```

Supports:

- Showing NPC dialogue text
- Opening/closing UI window
- Centralized dialogue display

Currently dialogue is one-way only.

Future expansion:

- Player response options
- Branching dialogue
- Conditions
- Relationship logic

---

# Current NPC Flow

Example flow:

```text
Player right-clicks NPC
→ Interaction menu opens
→ Player selects interaction
→ Interaction executes
→ NPC dialogue window opens
```

---

# Current Design Philosophy

This system intentionally focuses ONLY on NPC logic.

The NPC system does NOT manage:

- Museum objects
- Audio playback
- Scene management
- Quest progression
- Inventory systems

External systems are expected to communicate through:

```csharp
npc.ReactTo(context);
```

This keeps the NPC architecture modular and scalable.

---

# Future Expansion Plans

Planned additions:

- Dialogue trees
- Response choices
- Conditional interactions
- NPC action system
- Animation integration
- Speech bubbles
- Relationship/reputation logic
- Advanced reaction chaining
- Mood-specific interaction availability

---
