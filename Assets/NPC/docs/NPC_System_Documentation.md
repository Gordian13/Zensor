# NPC System User Manual

## 1. Purpose

This manual explains how to use, configure and extend the NPC system in Unity.

The NPC system supports:

- NavMesh patrol movement
- Random or sequential patrol-point selection
- Mood-dependent movement speed and waiting time
- Starting a dialogue by left-clicking an NPC
- Moving the NPC to an interaction anchor before dialogue starts
- Cancelling the interaction if that anchor moves away
- Node-based dialogue with player choices
- Short reaction messages
- Rules that react to events from other gameplay systems
- Integer-based reactions through a shared `GameIntValueSource`
- Optional ambient speech near the player
- Animator parameters for movement, idle selection and interaction state

The following features require a separate implementation:

- A working interaction-menu flow
- Concrete `NPCInteraction` assets
- Automatic reactions for every value in `NPCReactionEventType`
- Quest, inventory, audio or museum-object logic

Those responsibilities should remain in their own systems. They may send an `NPCReactionContext` to an NPC when a reaction is needed.

---

## 2. Folder Structure

The NPC system is stored under `Assets/NPC`:

```text
Assets/NPC
├── Animations
├── docs
├── Materials
├── Models
├── Prefabs
├── ScriptableObjects
│   ├── Dialogue
│   └── Profiles
├── Scripts
├── Textures
└── UI
```

The important working folders are:

- `Prefabs`: complete NPC objects used in scenes
- `ScriptableObjects/Profiles`: reusable NPC names, moods, rules and default dialogues
- `ScriptableObjects/Dialogue`: dialogue text assets
- `Scripts`: runtime NPC code
- `Animations`: Animator Controllers used by the NPC models
- `docs`: this manual

---

## 3. How a Normal NPC Interaction Works

A normal interaction follows this flow:

```text
Player left-clicks an NPC collider
→ NPCInteractionHandler finds the NPCController
→ GlobalInteractionState blocks other interactions
→ NPCController stops its patrol
→ NPC walks toward the shared interaction anchor
→ NPC stops within interactionArrivalDistance
→ NPC faces the configured look target
→ NPCDialogueWindow parses the default dialogue
→ The ::start node and its choices are displayed
→ The player follows choices or closes the conversation
→ Ambient speech is paused for five seconds
→ NPCController resumes its patrol
→ GlobalInteractionState allows other interactions again
```

Input and window behavior:

- Start an NPC interaction with the **left mouse button**.
- Dialogue is opened directly after arrival.
- Close the dialogue with its close button or with a dialogue choice that ends the conversation.

---

## 4. Main Parts of the System

### NPCController

`NPCController` controls one NPC at runtime. It connects:

- The assigned `NPCProfile`
- Current mood
- NavMesh patrol
- Reaction rules
- Interaction movement
- Dialogue output
- Animation state

Inspect this component first when troubleshooting movement, interaction or patrol behavior.

### NPCProfile

`NPCProfile` is a ScriptableObject containing reusable NPC data:

- NPC name
- Hover color data
- Mood settings
- Reaction rules
- Interaction list data
- Default dialogue

Create one with:

```text
Create → NPC → Profile
```

The same profile can be assigned to several prefabs. Changes to that shared profile affect every NPC that uses it.

### NPCInteractionHandler

`NPCInteractionHandler` is a central scene component. It:

- Reads left-click input
- Casts a ray from the configured camera
- Finds an `NPCController` on the hit object or its parent
- Blocks other interactions through `GlobalInteractionState`
- Sends the NPC to the interaction anchor
- Opens the NPC's default dialogue after arrival

The main scene contains a configured handler.

### NPCDialogueWindow

`NPCDialogueWindow` displays both full conversations and short reaction messages.

Full conversations:

- Parse an `NPCDialogueScript`
- Start at the node named `start`
- Show NPC text
- Create clickable player choices
- Follow links to other nodes
- End or open a linked dialogue

Short reactions:

- Show one reaction line
- Use a smaller window height
- Hide automatically after a configured time

### NPCAnimationController

`NPCAnimationController` reads the NavMeshAgent and updates these Animator parameters:

- `Speed`: raw movement speed used by transitions
- `MoveSpeed`: corrected playback factor for the walk animation
- `RandomIdle`: idle-animation selection
- `IsInteracting`: whether a direct interaction is active

The Animator Controller assigned to the model must contain the parameters used by that NPC.

### NPCAmbientSpeech

`NPCAmbientSpeech` may show a random reaction line when the player is near the NPC.
Ambient checks pause during a direct interaction.

It requires:

- An NPC reference, or an `NPCController` on the same GameObject
- A player or camera transform in `Player Anchor`
- At least one non-empty ambient line

### NPCNumericReactionListener

`NPCNumericReactionListener` connects the NPC system to a shared `GameIntValueSource` outside the NPC folder.

When the number changes, it creates this kind of context:

```csharp
new NPCReactionContext(
    NPCReactionEventType.NumericValueChanged,
    sourceId,
    sourceDisplayName,
    newValue
);
```

The source ID must match the `Required Object Id` of the reaction rule exactly.

---

## 5. Setting Up an NPC

### Step 1: Place or create the prefab

Use an existing prefab from `Assets/NPC/Prefabs` when possible.

A working NPC needs at least:

- A GameObject with `NPCController`
- A `NavMeshAgent`
- A collider that can be hit by the interaction raycast
- An assigned `NPCProfile`
- An `NPCAnimationController` when animation is required
- An Animator on the model or one of its child objects

Place the `NavMeshAgent` on the NPC root so the controller can find it automatically when the `Agent` reference is empty.

### Step 2: Configure the profile

Assign a profile under:

```text
NPCController → NPC Profile → Profile
```

In the profile, check:

1. `Npc Name`
2. At least one mood entry matching the controller's starting mood
3. A `Default Dialogue Script` if the NPC should react to clicks
4. Reaction rules if another system should trigger special behavior

### Step 3: Configure patrol points

Create scene objects for the patrol route:

```text
PatrolPoints
├── PatrolPoint_01
├── PatrolPoint_02
├── PatrolPoint_03
└── PatrolPoint_04
```

Assign them to:

```text
NPCController → Patrol Points
```

Patrol points are scene transforms. Prefab assets commonly contain empty references, so assign the correct scene points on the prefab instance when necessary.

The NavMesh must be baked and each point must be reachable.

### Step 4: Configure the central interaction handler

The scene should contain one active `NPCInteractionHandler` with:

- `Raycast Camera`: the camera used for player input
- `NPC Layer Mask`: layers that contain clickable NPC colliders
- `Max Distance`: maximum click distance
- `Interaction Anchor`: where the NPC should walk before dialogue
- `Look At Target`: usually the camera or a point near it

The default layer mask checks every layer. A dedicated NPC layer is clearer when unrelated colliders block clicks.

### Step 5: Configure dialogue UI

The active `NPCDialogueWindow` needs valid references for:

- Root
- NPC Text
- Choices Parent
- Choice Text Prefab
- Close Button
- Window Rect
- Choices Area

The system uses the static `NPCDialogueWindow.Instance`. Keep one active dialogue window in the scene. If several windows run `Awake`, the last one becomes the active instance.

---

## 6. NPCController Inspector Settings

Values saved in a prefab or scene are used at runtime. The values written in the C# file initialize a newly added component.

### NPC Profile

#### Profile

Assigns the reusable name, moods, rules and default dialogue.
A profile is required for reactions and normal dialogue.

### Mood State Machine

#### Current Mood

Starting mood of the NPC.
The controller looks for the first matching `NPCMoodData` entry in the profile and applies its movement speed.

### Movement

#### Agent

NavMeshAgent used for patrol and interaction movement.
When empty, the controller searches on the same GameObject.

#### Interaction Arrival Distance

Distance kept from the interaction anchor.

The code starting value is `1.5` units. The NPC prefabs use approximately `1.2` units.

- Increase it when the NPC walks too close to the camera or player.
- Decrease it when the NPC stops too far away.

This value temporarily replaces the normal NavMeshAgent stopping distance during interaction movement.

#### Patrol Points

Positions visited by the patrol.

- In random mode they are a selection pool.
- In sequential mode their Inspector order is the route order.

#### Stopping Distance

Distance at which a patrol point counts as reached.

The default `0.5` allows the NPC to count a nearby reachable position as the patrol target.
Patrol stopping distance and interaction arrival distance are configured independently.

### Patrol Behaviour

#### Start Patrolling On Start

Starts patrol automatically after the optional welcome interaction.
Disable it for stationary NPCs or NPCs controlled by another script.

#### Pick Random Points

- Enabled: chooses random patrol points.
- Disabled: visits points in list order and repeats the list.

#### Avoid Same Point Twice

Prevents random patrol from selecting the previous point again.
This setting applies to random patrol with at least two patrol points.

### Interactions

#### Is Interactable

Allows left-click interaction and scripted calls through `NPCInteractionHandler`.
Patrol and ambient behavior continue when interaction is disabled.

#### Do Welcome Interaction After Spawn

Starts an automatic dialogue shortly after the NPC appears.
The normal patrol waits until this dialogue finishes.

#### Welcome Dialogue Script

Dialogue used by the automatic welcome interaction.
Player clicks use `NPCProfile.defaultDialogueScript`.

#### Welcome Interaction Delay

Delay before the welcome dialogue begins.

The `0.1` second starting value gives scene managers and UI singletons time to finish their `Awake` setup.

#### Interaction Move Away Tolerance

The controller remembers the shortest measured distance to the anchor.
Movement is cancelled if the current distance becomes larger than:

```text
shortest distance + interactionMoveAwayTolerance
```

The NPC prefabs use about `0.25` units. Increase it if normal camera movement cancels interactions too easily. Decrease it if NPCs keep following a player who clearly moved away.

#### Interaction Destination Refresh Rate

Seconds between destination updates during the approach to the interaction anchor.

The NPC prefabs use about `0.2` seconds.

- Lower values follow moving anchors more closely and request NavMesh paths more often.
- Higher values request fewer path updates and react more slowly to a moving anchor.

### Animation

#### Animation Controller

Reference to `NPCAnimationController`.
It receives the interaction state and synchronizes the Animator with NavMesh movement.

### Debug

#### Draw Gizmos

Shows patrol points, route lines and the current target in Unity's Scene view.
This setting affects editor visualization and has no runtime gameplay effect.

#### Current Target

Runtime debug value showing the active patrol destination.
Treat this field as read-only during Play Mode.

---

## 7. Mood Settings and Movement Speed

Each `NPCMoodData` entry contains:

- `Mood`
- `Move Speed`
- `Wait At Point Seconds`
- `Speech Bubble Text`
- `Gizmo Color`

### Move Speed

NavMesh speed in Unity units per second.
Changing it also affects how the walk animation must be tuned.

### Wait At Point Seconds

Time spent waiting after reaching a patrol point.

- Larger values make patrol feel calmer.
- Smaller values make the NPC change direction more often.

### Speech Bubble Text

Optional text data associated with the mood.
A speech-display component can read this value and show it in the game.

### Gizmo Color

Scene-view color of patrol helpers for this mood.
This color controls the patrol Gizmos in Unity's Scene view.

---

## 8. Preventing Sliding in the Walk Animation

The visible walk speed is calculated in `NPCAnimationController`:

```text
MoveSpeed = actual NavMesh speed
            / referenceWalkSpeed
            * animationSpeedMultiplier
```

### Reference Walk Speed

Movement speed treated as the normal speed of the animation clip.

A good first value is the `Move Speed` of the NPC's normal mood.
For example:

```text
Normal mood move speed: 2
Reference walk speed:   2
Base animation ratio:   1
```

Keep this value above zero because it is used as a divisor.

### Animation Speed Multiplier

Manual final correction for the character model and its walk clip.

- Increase it when the NPC moves farther than the feet appear to walk.
- Decrease it when the feet move faster than the NPC travels.

Tune it by watching the feet pass a clear floor marker. Each model may need its own value because walk clips can cover different distances per animation cycle.

Prefab configuration examples:

| NPC | Reference walk speed | Animation multiplier |
| --- | ---: | ---: |
| Burkhardt Seiler | 2.5 | 0.92 |
| C. Lippke | 2.0 | 1.28 |
| Frieder Butzmann | 2.0 | 1.28 |

The script also sets:

- `Speed` to the raw agent velocity for Animator transitions.
- `MoveSpeed` to the corrected value above for animation playback.

Near the destination, animation speed is forced to zero with a small margin. This prevents tiny remaining NavMesh movement from keeping the walking animation active.

---

## 9. Dialogue Assets and Syntax

Create a dialogue asset with:

```text
Create → NPC → Dialogue → Dialogue Script
```

A normal conversation must contain a node named `start`:

```text
::start
NPC: Hello. What would you like to know?

> Tell me about this place.
NPC: This building was a record store.
-> history

> Goodbye.
NPC: See you later.
-> end

::history
NPC: The story starts in the late seventies.

> Back.
-> start
```

Supported syntax:

| Syntax | Meaning |
| --- | --- |
| `::nodeId` | Starts or replaces a node with this ID |
| `NPC: text` | Adds text spoken by the NPC |
| `> text` | Starts a player choice |
| `-> nodeId` | Continues at another node |
| `-> end` | Ends the conversation |
| `=> external` | Opens the asset assigned as External Dialogue |
| `# text` | Comment ignored by the parser |
| `// text` | Comment ignored by the parser |

Plain text is appended to the active NPC text or choice response.

Important rules:

- A full conversation needs `::start`.
- Node IDs are case-sensitive dictionary keys.
- The latest repeated node ID replaces the earlier node.
- A missing target node logs a warning.
- `=> external` needs an assigned `External Dialogue` asset.
- `-> end` creates a Continue button that closes the conversation.

### Dialogue Window Settings

#### Conversation Height

Window height in Canvas units for a conversation with choices.
The code starting value is `400`.

#### Reaction Height

Smaller height used for short reaction messages.
The code starting value is `180`.

#### Default Reaction Duration

Seconds before a short reaction hides itself.
The NPC prefabs use `4` seconds. A newly added component starts with `8` seconds unless another value is saved in the Inspector.

#### Use Typing Animation

Reveals text one character at a time.
When disabled, `Characters Per Second` has no effect.

#### Characters Per Second

Typing-animation speed. Keep it above zero.
The NPC prefabs use `100`. A newly added component starts with `40` unless another value is saved in the Inspector.

---

## 10. Reaction Rules

Other gameplay systems may send an `NPCReactionContext`:

```csharp
NPCReactionContext context = new NPCReactionContext(
    NPCReactionEventType.NumericValueChanged,
    "records_played",
    "Played Records",
    intValue: 6
);

NPCReactionResult result = npc.ReactTo(context);
```

Rules are checked from top to bottom. The first matching rule runs.
Place specific rules before general fallback rules.

### Event Type

Must exactly match the context event type.

`NumericValueChanged` has an automatic sender through `NPCNumericReactionListener`.
Other event types require another gameplay system to create and send the context.

### Required Object Id

Optional exact match for `context.objectId`.
Leave it empty to accept the chosen event from every source.

### Integer Condition

For a minimum threshold:

```text
Use Int Modulus:      false
Use Minimum Int Value: true
Minimum Int Value:     6
```

This accepts values of 6 or more.

For every sixth value:

```text
Use Int Modulus:       true
Use Minimum Int Value: true
Minimum Int Value:     6
```

This accepts non-zero values divisible by 6, such as 6, 12 and 18.
In modulus mode, `Minimum Int Value` stores the divisor. Keep it greater than zero.

### Float Condition

The float settings follow the same minimum-versus-modulus structure.
Exact remainder checks with floating-point values can be affected by rounding. Prefer integer values for counters.

### Change Mood and Resulting Mood

When enabled, the rule changes the NPC mood before showing its reaction text.
The profile should contain an `NPCMoodData` entry for the resulting mood.

### Reaction Text

Optional line shown through `NPCDialogueWindow`.
An empty line still allows the rule to count as a reaction.

### Block Original Action

Returns a request to the system that sent the event.
The NPC cannot cancel another system's action by itself.
The caller must check:

```csharp
if (result.blockOriginalAction)
{
    // stop the action owned by the calling system
}
```

### Project Configuration Example

Burkhardt Seiler has a numeric rule with:

```text
Event Type:              NumericValueChanged
Required Object Id:      records_played
Use Int Modulus:         true
Use Minimum Int Value:   true
Minimum Int Value:       6
```

It reacts whenever the supplied non-zero record count is divisible by six.

---

## 11. Numeric Reaction Listener Settings

### NPC

NPC that receives the reaction context.
The component finds an `NPCController` on the same GameObject when this field is empty.

### Value Source

Component derived from `GameIntValueSource`.
It supplies `CurrentValue` and raises `ValueChanged`.

This reference must be assigned for the listener to work.

### Source Id

Technical ID placed in the reaction context.
It must exactly match the reaction rule's `Required Object Id`.

### Source Display Name

Readable name included in the context for UI and debugging.

### Evaluate Current Value On Enable

- Enabled: immediately sends the source's existing value when the component becomes active.
- Disabled: waits for the next value change before sending an event.

Keep it enabled when an NPC must notice changes that happened before the NPC was activated.

---

## 12. Ambient Speech Settings

### Player Anchor

Player or camera transform used for the distance check.
Assign this field to enable ambient speech.

### Trigger Distance

Maximum distance in Unity world units at which speech may start.

### Cooldown Seconds

Minimum time between two successfully spoken lines.
The code starting value is `12` seconds.

### Chance Per Check

Probability written as a decimal value from `0` to `1`.

Examples:

```text
0.10 = 10 percent
0.35 = 35 percent
1.00 = always
```

Important: some NPC prefabs store `30` or `35`. Those values always pass the random test. Change them to `0.30` or `0.35` if the intended chance is 30 or 35 percent.

### Check Interval

Seconds between distance and chance checks.

The default `1` second makes the probability easy to read as a chance per second after cooldown. Lower intervals check more often and therefore increase the total chance of speaking over time.

### Ambient Lines

One line is selected randomly after all checks pass.
Remove empty entries because selecting one causes that speech attempt to do nothing.

---

## 13. Optional and Unconnected Parts

### NPCProfile.hoverColor

This value is available for a hover or selection effect.
Add a visual hover component that reads and applies this color when the mouse points at the NPC.

### NPCMoodData.speechBubbleText

This value stores text for a mood. Add a speech component that reads it to display mood-based text.

### NPCProfile.interactions and NPCInteractionMenu

`NPCInteractionHandler` opens `NPCDialogueWindow` directly. The interaction menu becomes available after it is connected to the gameplay flow.

To use the interaction menu, provide the following connections:

- Call `NPCInteractionMenu.Open()` from the intended input or interaction path.
- Create concrete classes that inherit from `NPCInteraction`.
- Add the resulting interaction assets to the profile interaction lists.

---

## 14. Public API for Other Systems

### Send a reaction

```csharp
NPCReactionResult result = npc.ReactTo(context);
```

Preferred when the behavior should be controlled by profile rules.

### Change mood directly

```csharp
npc.SetMood(NPCMood.Moody);
```

Use this method when another system controls the mood directly.

### Show a short NPC line

```csharp
npc.Say("Please keep your hands off that.");
```

### Control patrol

```csharp
npc.StartPatrol();
npc.StopPatrol();
```

### Start a normal interaction

Prefer the central handler:

```csharp
NPCInteractionHandler.Instance.StartInteraction(npc);
```

This uses the profile's default dialogue and the scene's shared interaction anchor.

---

## 15. Troubleshooting

### Clicking the NPC does nothing

Check:

- `Is Interactable` is enabled.
- The NPC or a child object has a collider.
- The collider layer is included in `NPC Layer Mask`.
- The handler has the correct camera.
- `GlobalInteractionState` is available for a new interaction.
- The NPC profile has a default dialogue.
- The interaction anchor is assigned.

### Patrol troubleshooting

Check:

- The NavMesh is baked.
- The NPC starts on the NavMesh.
- Patrol points are assigned and reachable.
- `Start Patrolling On Start` is enabled.
- The assigned profile contains data for the current mood.

### Dialogue fails to open after arrival

Check:

- An active `NPCDialogueWindow.Instance` exists.
- Its UI references are assigned.
- The dialogue contains `::start`.
- The dialogue root object can be activated.

### Foot sliding during walking

Check:

1. The mood's `Move Speed`.
2. `Reference Walk Speed` on `NPCAnimationController`.
3. `Animation Speed Multiplier`.
4. The scale and import settings of the animation/model.

Start by matching reference speed to the normal mood movement speed. Then tune the multiplier by watching the feet move against the floor.

### Numeric reaction never happens

Check:

- `Value Source` is assigned.
- `Source Id` exactly matches `Required Object Id`.
- Event type is `NumericValueChanged`.
- Integer options are configured correctly.
- A modulus divisor is greater than zero.
- The intended rule appears before broader matching rules.

### Ambient speech never happens

Check:

- `Player Anchor` is assigned.
- The player is within `Trigger Distance`.
- Ambient lines contain valid text.
- `Chance Per Check` is greater than zero.
- The NPC has finished its direct interaction.
- The cooldown has finished.

### Ambient speech happens every check

Check that `Chance Per Check` is between `0` and `1`.
A 35 percent chance is written as `0.35`.

---

## 16. Safe Extension Guidelines

When extending the system:

- Keep museum-object logic outside `NPCController`.
- Send object events through `NPCReactionContext` to keep object behavior outside the NPC.
- Add reusable prefab data to the profile.
- Keep scene references such as patrol points and anchors on scene components or prefab instances.
- Preserve the shared `GlobalInteractionState` block and unblock flow.
- Add matching documentation and Inspector tooltips for every new configurable field.
- Recheck animation tuning whenever movement speed, model scale or animation clips change.

The NPC owns movement, mood, reactions and dialogue. Other gameplay systems remain responsible for their own behavior.
