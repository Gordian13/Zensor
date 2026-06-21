# NPC System Documentation

## Purpose

This document describes how to use the current NPC prototype system in Unity.

The system is built for a museum/store-style experience where an NPC can:

- Patrol through the scene
- React to scripted user actions
- Change mood/state
- Show hover feedback
- Open an interaction menu
- Display dialogue
- Walk toward an interaction anchor before starting an interaction
- Resume patrol after interaction ends

The system intentionally focuses on **NPC-side logic only**.

It does not manage:

- Museum objects
- Audio playback
- Scene switching
- Inventory
- Quest progression
- Player movement systems

Other systems should communicate with the NPC through public methods and reaction contexts.

---

# Recommended Folder Structure

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

# NPC Prefab Structure

Recommended prefab:

```text
NPC_Base
├── NavMeshAgent
├── NPCController
├── NPCHoverHighlight
├── Visual
│   ├── Mesh Renderer / Skinned Mesh Renderer
│   └── Collider
└── InteractionTrigger
    └── Capsule Collider
```

## Important Setup Notes

- `NPC_Base` should stay at ground level.
- `NavMeshAgent` belongs on `NPC_Base`, not on `Visual`.
- `Base Offset` on the NavMeshAgent should usually be `0`.
- `Visual` may be offset upward if using a capsule placeholder.
- `InteractionTrigger` is used for raycast interaction detection.
- `NPCHoverHighlight` should sit on `NPC_Base`, because raycasts may hit child objects like `InteractionTrigger`.

---

# NavMeshAgent Setup

Recommended starting values:

```text
Speed:             controlled by mood
Angular Speed:     720
Acceleration:      30
Stopping Distance: 0.5
Radius:            0.5
Height:            2
Base Offset:       0
```

The NPC uses regular NavMeshAgent movement for patrol and interaction approach.

The interaction approach uses an interaction-specific stopping distance so the NPC does not walk directly into the player/camera anchor.

---

# Patrol Setup

Create simple empty GameObjects in the scene:

```text
PatrolPoints_NPC
├── PatrolPoint_01
├── PatrolPoint_02
├── PatrolPoint_03
└── PatrolPoint_04
```

Assign them in:

```text
NPC_Base
→ NPCController
→ Patrol Points
```

The patrol system supports:

- Random patrol point selection
- Sequential patrol point selection
- Avoiding the same point twice
- Waiting at points
- Mood-dependent movement speed
- Mood-dependent wait time

---

# NPC Profile

NPC configuration is stored in a ScriptableObject.

Create one via:

```text
Right Click → Create → NPC → Profile
```

Recommended location:

```text
Assets/NPCs/ScriptableObjects/Profiles
```

The profile stores:

- NPC name
- Hover color
- Mood settings
- Reaction rules
- Interaction list
- Default dialogue

This keeps reusable NPC data out of the runtime controller.

---

# Mood System

Current moods:

```text
Normal
Moody
Raged
```

Each mood can define:

- Movement speed
- Wait time at patrol points
- Speech flavor text
- Debug gizmo color

Mood changes are handled through:

```csharp
npc.SetMood(NPCMood.Raged);
```

External systems should not directly modify internal fields.

---

# Reaction Rule System

The reaction system lets the NPC respond to external user actions.

External systems create an `NPCReactionContext` and pass it to the NPC:

```csharp
NPCReactionContext context = new NPCReactionContext(
    NPCReactionEventType.RecordPlayed,
    "record_forbidden_01",
    "Forbidden Record"
);

NPCReactionResult result = npc.ReactTo(context);
```

The NPC checks its profile rules and can:

- Change mood
- Say a dialogue line
- Return whether the original action should be blocked

Example profile rule:

```text
Event Type: RecordPlayed
Required Object Id: record_forbidden_01
Change Mood: true
Resulting Mood: Raged
Reaction Text: Die darfst du nicht spielen. Du bist nicht cool genug.
Block Original Action: true
```

This allows another system to ask whether an action may continue by checking:

```csharp
result.blockOriginalAction
```

---

# Hover Highlight (Deprecated and currently not in use!)

Hovering is detected by a central raycast detector.

Scene object:

```text
NPC_InputSystem
├── NPCHoverDetector
└── NPCInteractionHandler
```

`NPCHoverDetector` casts from the active camera and looks for an `NPCHoverHighlight`.

Hover color comes from:

```text
NPCProfile.hoverColor
```

Recommended hover alpha:

```text
0.2 - 0.4
```

Higher values quickly become radioactive. Use with restraint, unless the NPC has recently discovered uranium.

---

# Interaction System

Right-clicking an NPC starts an interaction request.

Flow:

```text
Right-click NPC
→ NPCInteractionHandler detects NPC
→ NPC walks to interaction anchor
→ NPC opens interaction menu after arrival
→ Player selects interaction
→ Dialogue opens
→ ESC closes dialogue
→ NPC resumes patrol
```

Interactions are ScriptableObjects derived from:

```csharp
NPCInteraction
```

Current concrete interaction:

```text
NPCTalkInteraction
```

Create one via:

```text
Right Click → Create → NPC → Interactions → Talk Interaction
```

Then add it to:

```text
NPC_Profile
→ Interactions
```

---

# Interaction Anchor System

The NPC does not need a real player character.

Instead, the system uses an **interaction anchor**.

This is usually an empty GameObject placed where the NPC should walk before starting the interaction.

Example:

```text
NPC_PlayerInteractionAnchor
```

Assign it in:

```text
NPC_InputSystem
→ NPCInteractionHandler
→ Interaction Anchor
```

Also assign:

```text
Look At Target → Main Camera
```

## Why anchors?

This works for:

- Fixed-camera museum scenes
- Point-and-click camera switching
- A future real player character
- Prototype freecam testing, if needed

If a real player object exists later, the same concept still works by passing the player transform or a child interaction anchor.

---

# Interaction Movement Behaviour

When an interaction is requested:

```text
NPC stops patrol
NPC walks toward the interaction anchor
NPC updates the destination while the anchor moves closer
NPC cancels if the anchor moves away after getting closer
NPC stops within interaction range
NPC faces the look target
Menu opens
```

The goal is:

```text
If the player/view moves toward the NPC → continue and meet earlier
If the player/view moves away → cancel interaction
```

This avoids the NPC chasing outdated positions forever.

---

# Useful Interaction Movement Values

Recommended values:

```csharp
[SerializeField] private float interactionArrivalDistance = 1.5f;
[SerializeField] private float interactionMoveAwayTolerance = 1.0f;
[SerializeField] private float interactionDestinationRefreshRate = 0.15f;
```

## interactionArrivalDistance

Controls how far away the NPC stops from the anchor.

Recommended:

```text
1.2 - 1.8
```

Use:

```text
1.5
```

as a practical starting point.

If the NPC walks too close, increase it.

If the NPC stops too far away, decrease it.

## interactionMoveAwayTolerance

Controls how much the anchor may move away before the interaction cancels.

Recommended:

```text
0.75 - 1.25
```

Use:

```text
1.0
```

for museum-style point-and-click movement.

Too low:

```text
tiny viewpoint movement cancels interaction
```

Too high:

```text
NPC keeps following when the player clearly left
```

## interactionDestinationRefreshRate

Controls how often the NPC updates its destination while the anchor moves closer.

Recommended:

```text
0.1 - 0.2 seconds
```

Use:

```text
0.15
```

as a good prototype value.

Too low recalculates too often.

Too high makes the NPC feel sluggish when the target moves closer.

---

# Dialogue Window

The dialogue window is a simple UI panel.

Current behavior:

```text
NPC says text
→ NPCDialogueWindow opens
→ ESC closes window
→ NPC resumes patrol
```

The dialogue system currently supports one-way NPC text.

Full dialogue trees are intentionally out of scope for the current prototype.

---

# Interaction Menu

The interaction menu is opened after the NPC reaches the interaction anchor.

It dynamically creates buttons from:

```text
NPCProfile.interactions
```

If no interactions exist, it can show an empty text message.

Recommended empty text:

```text
No interactions available.
```

The menu can be closed with:

```text
ESC
```

---

# Public NPC API

Other systems should communicate with the NPC using public methods.

Common calls:

```csharp
npc.SetMood(NPCMood.Moody);
npc.Say("Please do not touch that.");
npc.ReactTo(context);
npc.BeginInteraction();
npc.EndInteraction();
```

For user action reactions, prefer:

```csharp
npc.ReactTo(context);
```

instead of directly changing mood or dialogue.

That keeps rule logic inside the NPC system.

---

# Current Prototype Flow Example

```text
NPC patrols between points
Player right-clicks NPC
NPC stops patrol
NPC walks toward interaction anchor
If player/view moves away, interaction cancels
If player/view moves closer, NPC updates destination
NPC stops at interaction distance
NPC faces camera/look target
Interaction menu opens
Player selects "Begrüßen"
Dialogue window opens
Player presses ESC
Dialogue closes
NPC resumes patrol
```

---

# Design Boundaries

This NPC system should remain responsible for:

- NPC patrol
- NPC state/mood
- NPC reactions
- NPC interactions
- NPC dialogue display
- NPC hover feedback

It should not become responsible for:

- Museum object behavior
- Playing records
- Reading flyers
- Scene switching
- Camera transition logic
- Quest progression
- Inventory state

Other systems should call into the NPC through clean interfaces.

This prevents the NPC from becoming a charming but monstrous God-object. Unity projects have enough folklore already.

---

# Current Prototype Status

Implemented:

- Patrol movement
- Mood system
- ScriptableObject NPC profiles
- Reaction rules
- Reaction context/result API
- Hover highlighting
- Right-click detection
- Interaction menu
- ScriptableObject interactions
- Dialogue window
- Interaction stopping/resuming patrol
- Interaction anchor approach behavior
- Cancellation when player/view moves away

Prototype-ready for:

- Main branch testing
- Museum interaction experiments
- Store-owner / caretaker behavior tests
- Simple scripted boundary enforcement

Not implemented / intentionally out of scope:

- Full dialogue trees
- Advanced AI conversation
- Complex emotional simulation
- Production-grade animation logic
- Final UI polish

---
