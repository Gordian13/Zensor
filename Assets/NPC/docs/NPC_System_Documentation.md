# NPC System User Manual

## Contents

- [1. Purpose](#1-purpose)
- [2. Folder Structure](#2-folder-structure)
- [3. How a Normal NPC Interaction Works](#3-how-a-normal-npc-interaction-works)
- [4. Main Parts of the System](#4-main-parts-of-the-system)
- [5. Setting Up an NPC](#5-setting-up-an-npc)
  - [Place or create the prefab](#step-1-place-or-create-the-prefab)
  - [Configure the profile](#step-2-configure-the-profile)
  - [Configure the NavMesh and patrol points](#step-3-configure-the-navmesh-and-patrol-points)
  - [Configure the interaction handler](#step-4-configure-the-central-interaction-handler)
  - [Configure the dialogue UI](#step-5-configure-dialogue-ui)
  - [Understand prefab and scene references](#prefab-assets-scene-instances-and-overrides)
  - [Check shared scene objects](#shared-scene-objects)
- [6. NPCController Inspector Settings](#6-npccontroller-inspector-settings)
- [7. Mood Settings and Movement Speed](#7-mood-settings-and-movement-speed)
- [8. Animator Setup and Walk Animation](#8-animator-setup-and-walk-animation)
  - [Import an animation](#importing-an-animation)
  - [Set up the Animator](#setting-up-the-unity-animator)
  - [Create transitions](#creating-transitions)
  - [Connect the Animator to the NPC](#connecting-the-animator-to-the-npc)
- [9. Dialogue Assets and Syntax](#9-dialogue-assets-and-syntax)
- [10. Reaction Rules](#10-reaction-rules)
  - [Available reaction events](#available-reaction-events)
  - [Record counter connection](#complete-record-counter-connection)
- [11. Numeric Reaction Listener Settings](#11-numeric-reaction-listener-settings)
- [12. Ambient Speech Settings](#12-ambient-speech-settings)
- [13. Optional and Unconnected Parts](#13-optional-and-unconnected-parts)
- [14. Public API for Other Systems](#14-public-api-for-other-systems)
- [15. Troubleshooting](#15-troubleshooting)
- [16. Safe Extension Guidelines](#16-safe-extension-guidelines)

Unity paths in this manual use `→` for the next click or selection. `Hierarchy` means an object in the open scene. `Project window` means a file or asset stored in the project. `Inspector` means the settings shown for the selected object or asset.

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

[Back to contents](#contents)

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

[Back to contents](#contents)

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

[Back to contents](#contents)

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

[Back to contents](#contents)

---

## 5. Setting Up an NPC

### Step 1: Place or create the prefab

The easiest starting point is an existing prefab:

```text
Project window → Assets → NPC → Prefabs
```

Drag the chosen prefab from the Project window into the scene Hierarchy. Select the new prefab instance in the Hierarchy to edit its scene-specific references.

The prefab defaults are:

| Prefab | Patrol on start | Direct dialogue | Welcome dialogue | Configured record reaction |
| --- | --- | --- | --- | --- |
| `B_Seiler` | enabled | enabled | enabled | every sixth record count |
| `C_Lippke` | disabled | disabled | disabled | none |
| `F_Butzmann` | enabled | disabled | disabled | none |

`C_Lippke` and `F_Butzmann` need `Is Interactable` enabled and a `Default Dialogue Script` assigned in their profiles before they can start a normal click dialogue.

To build an NPC from an empty object:

1. In the Hierarchy, right-click and choose **Create Empty**.
2. Rename the object to the NPC's name.
3. In the Inspector, select **Add Component** and add `NPCController`.
4. Add a `NavMeshAgent` and a collider to the same root object.
5. Add `NPCAnimationController` when the NPC uses animation.
6. Place the animated model below the root object and give the model an `Animator` component.

A working NPC needs at least:

- A GameObject with `NPCController`
- A `NavMeshAgent`
- A collider that can be hit by the interaction raycast
- An assigned `NPCProfile`
- An `NPCAnimationController` when animation is required
- An Animator on the model or one of its child objects

Place the `NavMeshAgent` on the NPC root so the controller can find it automatically when the `Agent` reference is empty.

Inspector path for the main runtime settings:

```text
Hierarchy → NPC prefab instance → Inspector → NPCController
```

### Step 2: Configure the profile

NPC profiles are reusable assets. Existing profiles are stored in:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
```

To create a profile:

```text
Project window → right-click → Create → NPC → Profile
```

Select the profile asset in the Project window to edit its name, mood list, reaction rules and default dialogue in the Inspector.

Assign the profile to the NPC under:

```text
Hierarchy → NPC instance → Inspector → NPCController → NPC Profile → Profile
```

In the profile, check:

1. `Npc Name`
2. At least one mood entry matching the controller's starting mood
3. A `Default Dialogue Script` if the NPC should react to clicks
4. Reaction rules if another system should trigger special behavior

### Step 3: Configure the NavMesh and patrol points

#### Bake or update the NavMesh

The Main scene uses the `NavMeshSurface` component on the object named `Ground`:

```text
Hierarchy → Ground → Inspector → NavMesh Surface
```

Use the component's **Bake** button after changing walkable level geometry. The baked blue area in the Scene view shows where an NPC can move.

Check these settings before baking:

- `Agent Type` must match the Agent Type used by the NPC's `NavMeshAgent`.
- `Include Layers` must include the floor and other walkable geometry.
- Floor objects need usable meshes or colliders, depending on the selected `Use Geometry` setting.

Configure the NPC agent under:

```text
Hierarchy → NPC instance → Inspector → NavMesh Agent
```

Important NavMeshAgent values:

- `Agent Type` must match the baked NavMesh Surface.
- `Radius`, `Height` and `Base Offset` should fit the character model and available doorways.
- `Acceleration` controls how quickly the agent reaches the mood movement speed.
- `Speed` is replaced by the active mood's `Move Speed` at runtime.
- `Stopping Distance` is controlled by `NPCController` for patrol and interaction movement.
- Rotation is handled by `NPCAnimationController`, which turns the NPC toward the desired NavMesh direction.

Place the NPC root on the visible NavMesh. A nearby position in the Scene view is insufficient when the root itself is outside the baked area.

#### Create patrol points

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
Hierarchy → NPC instance → Inspector → NPCController → Movement → Patrol Points
```

Set the list size, then drag each patrol-point Transform from the Hierarchy into one list element. The element order is the route order when `Pick Random Points` is disabled.

Patrol points are scene transforms. Prefab assets commonly contain empty references, so assign the correct scene points on the prefab instance when necessary.

Every patrol point must be on or very close to the same reachable NavMesh area as the NPC.

### Step 4: Configure the central interaction handler

The Main scene stores the central handler here:

```text
Hierarchy → NPC → NPC_Input_System → Inspector → NPCInteractionHandler
```

The scene should contain one active `NPCInteractionHandler` with:

- `Raycast Camera`: the camera used for player input
- `NPC Layer Mask`: layers that contain clickable NPC colliders
- `Max Distance`: maximum click distance
- `Interaction Anchor`: where the NPC should walk before dialogue
- `Look At Target`: usually the camera or a point near it

The default layer mask checks every layer. A dedicated NPC layer is clearer when unrelated colliders block clicks.

The existing Main scene uses `MainCamera` for `Raycast Camera`, `Interaction Anchor` and `Look At Target`. This makes the NPC approach the active camera position and face it before dialogue begins. A separate child Transform can be used as the anchor when the NPC should stop at a more controlled position.

For a dedicated NPC layer:

1. Select the NPC root or its clickable child in the Hierarchy.
2. Choose the NPC layer at the top of the Inspector.
3. Include that layer in `NPCInteractionHandler → NPC Layer Mask`.
4. Make sure the object on that layer has a collider.

### Step 5: Configure dialogue UI

The existing NPC prefabs contain a child UI object named `NPCDialogueWindow`. Select it under the prefab instance:

```text
Hierarchy → NPC instance → NPCDialogueWindow → Inspector → NPCDialogueWindow
```

The active component needs valid references for:

- Root
- NPC Text
- Choices Parent
- Choice Text Prefab
- Close Button
- Window Rect
- Choices Area

The system uses the static `NPCDialogueWindow.Instance`. Keep one active dialogue window in the scene. If several windows run `Awake`, the last one becomes the active instance.

The provided NPC prefabs each contain a dialogue-window child. When several NPC prefabs are loaded together, keep one shared `NPCDialogueWindow` component active and disable or remove duplicate window components. Every NPC can use the same shared window through `NPCDialogueWindow.Instance`.

The expected UI structure is:

```text
NPCDialogueWindow
├── NPC text
├── Choices area
│   └── Choices parent
└── Close button
```

- `Root` is the top-level object shown for dialogue and hidden when dialogue closes.
- `NPC Text` is the TextMeshPro element that displays the NPC line.
- `Choices Parent` receives the generated answer objects.
- `Choice Text Prefab` is cloned once for every visible answer.
- `Close Button` closes a full conversation.
- `Window Rect` is resized between normal dialogue and a short reaction.
- `Choices Area` is hidden for short reaction messages.

The choice prefab must contain a visible TextMeshPro text element and must receive UI pointer events. Keep an active `EventSystem` in the loaded scenes and use a Canvas with a working Graphic Raycaster.

The close button listener is added by `NPCDialogueWindow` during `Awake`, so its Unity **On Click** list does not need a manual NPC method assignment.

### Prefab Assets, Scene Instances and Overrides

Unity uses two related objects:

- The **prefab asset** is the reusable original stored under `Assets/NPC/Prefabs`.
- The **prefab instance** is the copy placed in a scene.

Use the prefab asset for reusable values such as component defaults, animation references and built-in UI references. Use the scene instance for references to objects that exist only in that scene, such as patrol points, `MainCamera`, a player anchor or `GlobalPlayedRecordsCounter`.

When a scene-instance value differs from the prefab asset, Unity marks it as an override. Keep the override when it belongs only to that scene. Use **Overrides → Apply** only when the value should become the default for every future instance of that prefab.

Unity prefab assets cannot safely store references to ordinary objects from a scene. Assign those references after the prefab is placed in the scene.

### Shared Scene Objects

The Main scene contains shared objects required by the NPC flow:

| Hierarchy object | Component | Purpose |
| --- | --- | --- |
| `GlobalInteractionState` | `GlobalInteractionState` | Blocks camera, vinyl, poster and NPC input while another interaction is active. |
| `GlobalPlayedRecordsCounter` | `GlobalPlayedRecordsCounter` | Counts visits to the record-player flow and sends number changes. |
| `NPC/NPC_Input_System` | `NPCInteractionHandler` | Detects left-clicks and starts NPC dialogue movement. |
| `MainCamera` | `Camera` and Transform | Supplies the click ray and currently acts as the interaction and look target. |
| `Ground` | `NavMeshSurface` | Stores and rebuilds the walkable NavMesh. |

Keep one active instance of each global state or counter object. Duplicate singleton-like objects can replace each other's shared `Instance` reference.

For NPC dialogue to block other input correctly, `GlobalInteractionState` must be active before the interaction begins. The NPC handler blocks it when dialogue movement starts, and `NPCController` or `NPCDialogueWindow` releases it when the interaction ends.

To prepare another scene from scratch:

1. Create an empty `GlobalInteractionState` object and add the component with the same name.
2. Create an empty `GlobalPlayedRecordsCounter` object and add its component when record counting is needed.
3. Create an empty `NPC_Input_System` object and add `NPCInteractionHandler`.
4. Assign the scene camera, interaction anchor and look target to the handler.
5. Add one active `NPCDialogueWindow` UI setup.
6. Add an `EventSystem` when the scene contains no UI event system yet.

When several scenes are loaded additively, place shared objects in one reliable scene. Avoid placing another copy in every NPC or subscene.

[Back to contents](#contents)

---

## 6. NPCController Inspector Settings

Values saved in a prefab or scene are used at runtime. The values written in the C# file initialize a newly added component.

Select these settings through:

```text
Hierarchy → NPC prefab instance → Inspector → NPCController
```

The foldout headings inside the component are:

| Inspector heading | Fields |
| --- | --- |
| `NPC Profile` | Profile |
| `Mood State Machine` | Current Mood |
| `Movement` | Agent, Interaction Arrival Distance, Patrol Points, Stopping Distance |
| `Patrol Behaviour` | Start Patrolling On Start, Pick Random Points, Avoid Same Point Twice |
| `Interactions` | Is Interactable, welcome settings and interaction movement settings |
| `Animation` | Animation Controller |
| `Debug` | Draw Gizmos, Current Target |

Open the matching heading in the Inspector before looking for a field described below.

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

[Back to contents](#contents)

---

## 7. Mood Settings and Movement Speed

Mood entries are stored in the profile asset:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
→ select an NPC profile → Inspector → Mood Settings → Moods
```

Increase `Moods` list size to add an entry. Expand each element to edit its fields. `Current Mood` on `NPCController` must match one of these entries.

Available moods are:

- `Normal`: calm/default behavior
- `Moody`: annoyed behavior
- `Raged`: angry behavior

The readable NPC name is stored in the same profile under:

```text
Inspector → Identity → Npc Name
```

This name appears in dialogue labels, reaction output and debug messages. Give every finished profile a clear name.

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

[Back to contents](#contents)

---

## 8. Animator Setup and Walk Animation

### Importing an Animation

NPC movement is controlled by the `NavMeshAgent`. Animation clips only display that movement, so walking clips should stay in place instead of moving the model root through the scene.

Select an imported animation FBX through:

```text
Project window → Assets → NPC → Animations → select an FBX
```

For a new character-model FBX, select the model and configure `Rig → Animation Type: Humanoid` with `Avatar Definition: Create From This Model`. Use **Configure** to verify the bone mapping when Unity reports an invalid Avatar.

Configure its **Rig** tab:

1. Set `Animation Type` to `Humanoid`.
2. For an animation-only FBX, set `Avatar Definition` to `Copy From Other Avatar`.
3. Assign a compatible Avatar from the Rocketbox character model.
4. Select **Apply**.

Configure its **Animation** tab:

1. Select the clip in the clip list.
2. Enable `Loop Time` for walking and continuous idle clips.
3. Leave one-time gestures or short talking clips without looping when they should finish before a transition.
4. Use an In-Place clip. For clips that contain root movement, use the Root Transform position options to bake horizontal movement into the pose.
5. Select **Apply** after changing import settings.

On the character model, use:

```text
Hierarchy → NPC instance → animated model child → Inspector → Animator
```

Assign the model Avatar and the NPC Animator Controller. Keep `Apply Root Motion` disabled because the `NavMeshAgent` owns world movement. Enabling root motion here can make the model drift away from the NPC root or fight against NavMesh movement.

### Setting Up the Unity Animator

Each animated NPC needs an `Animator` on the character model and an Animator Controller assigned to it. The existing controllers are stored in:

```text
Assets/NPC/Animations
```

Open a controller by double-clicking it. Unity opens the Animator window. It can also be opened through:

```text
Unity menu → Window → Animation → Animator
```

The Animator window displays animation states as boxes and transitions as arrows between them.

To add an animation:

1. Import the animation clip with a Humanoid rig that is compatible with the NPC model.
2. Enable looping for animations such as walking or a continuous idle animation.
3. Drag the clip into the Animator window to create a state.
4. Give the state a clear name such as `Idle`, `Walking`, `Sitting` or `Talking`.
5. Right-click the state and select **Set as Layer Default State** when it should play after the Animator starts. Unity displays the default state in orange.

### Creating Transitions

Right-click a state, select **Make Transition**, and click the target state. Select the resulting arrow to configure the transition in the Inspector.

The `Sitting` and `Sitting Talking` states shown in the example use transitions in both directions:

```text
Entry -> Sitting -> Sitting Talking -> Sitting
```

Both transitions have **Has Exit Time** enabled and contain no Conditions. Unity therefore waits until the active clip has nearly finished and then changes to the other state. This creates an automatic loop between sitting quietly and talking.

Use this setup for animation changes that should happen automatically. For a state that should react to NPC movement or interaction, add one of the parameters described below as a transition Condition.

### Parameters Used by NPCAnimationController

The parameter names and types must match exactly because `NPCAnimationController` writes to them by name:

| Parameter | Type | Purpose |
| --- | --- | --- |
| `Speed` | Float | Real NavMesh movement speed. Use it to switch between idle and walking states. |
| `MoveSpeed` | Float | Corrected playback speed for the walking clip. Use it as the Walking state's speed multiplier. |
| `RandomIdle` | Int | Random value from `0` to `2` used to select different idle animations. |
| `IsInteracting` | Bool | Becomes true during a direct NPC conversation and false when the conversation ends. |

Add these values in the **Parameters** tab of the Animator window. Moving NPCs need `Speed`, `MoveSpeed` and `RandomIdle`. Add `IsInteracting` when the controller has a special conversation animation.

Exact location:

```text
Project window → Assets → NPC → Animations → open the Animator Controller
→ Animator window → Parameters tab → + button
```

To let `MoveSpeed` control the walk clip playback:

1. Select the `Walking` state in the Animator window.
2. In the Inspector, find the state's `Speed` setting.
3. Enable its parameter option and select `MoveSpeed`.
4. Keep the base Speed at `1` so the script value is the full playback multiplier.

### Recommended Movement Transitions

A basic moving NPC can use the following states and Conditions:

| Transition | Condition |
| --- | --- |
| `Idle -> Walking` | `Speed` greater than `0.1` |
| `Walking -> Idle` | `Speed` less than `0.1` |
| `Walking -> Talking` | `IsInteracting` is true and `MoveSpeed` is less than `0.01` |
| `Talking -> Idle` | `IsInteracting` is false |

Disable **Has Exit Time** for movement transitions when the character should react immediately. A short Transition Duration can still be used to blend smoothly between the clips.

Add a Condition by selecting the transition arrow and using:

```text
Inspector → Conditions → + button → parameter, comparison and threshold
```

For several idle animations, create one transition for every `RandomIdle` value. For example, the Conditions `Speed < 0.1`, `IsInteracting = false` and `RandomIdle = 1` can lead to the idle state assigned to value `1`.

### Connecting the Animator to the NPC

The Unity connection is:

```text
NavMeshAgent movement
-> NPCAnimationController
-> Animator parameters
-> Animator transitions and state playback
```

Set it up as follows:

1. Assign the Animator Controller to the character model's `Animator` component.
2. Assign the NPC's `NavMeshAgent` to `NPCAnimationController.Agent`.
3. Assign the model's `Animator` to `NPCAnimationController.Animator`.
4. Assign `NPCAnimationController` to `NPCController.Animation Controller`.
5. Set `Reference Walk Speed` and `Animation Speed Multiplier` for the model.

Inspector paths:

```text
Hierarchy → NPC instance → animated model child → Inspector → Animator
Hierarchy → NPC instance → Inspector → NPCAnimationController
Hierarchy → NPC instance → Inspector → NPCController → Animation
```

The component searches the NPC root for a `NavMeshAgent` and the children for an `Animator` when these references are empty. Explicit references make the setup easier to check in the Inspector.

During movement, `NPCAnimationController` reads the NavMeshAgent velocity every frame. It writes the real speed to `Speed` and the corrected animation value to `MoveSpeed`. `NPCController` changes `IsInteracting` when a conversation starts or ends. The Animator Controller then decides which animation state should play from these values.

The same component turns the NPC root toward the NavMeshAgent's desired movement direction. It also resets the animated model child's local X and Z position to zero every frame. Keep the animated model centered below the NPC root; use its Y position for a required vertical offset. This prevents animation clips from slowly moving the visible model away from the NavMeshAgent.

### Matching the Walking Animation to Movement

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

[Back to contents](#contents)

---

## 9. Dialogue Assets and Syntax

Dialogue assets are stored under:

```text
Project window → Assets → NPC → ScriptableObjects → Dialogue
```

Create a dialogue asset inside the desired dialogue folder with:

```text
Project window → right-click → Create → NPC → Dialogue → Dialogue Script
```

Select the new asset. Enter the dialogue in:

```text
Inspector → Dialogue → Dialogue Text
```

Assign it as a normal click dialogue through:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
→ select the NPC profile → Inspector → Dialogue Script → Default Dialogue Script
```

Assign a separate automatic welcome dialogue through:

```text
Hierarchy → NPC instance → Inspector → NPCController
→ Interactions → Welcome Dialogue Script
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

Assign an external dialogue on the same dialogue asset:

```text
Project window → select the NPCDialogueScript asset
→ Inspector → Dialogue → External Dialogue
```

The `=> external` command always opens the one asset stored in this field. Create another dialogue asset when a different external branch is required.

### Dialogue Window Settings

Select these fields through:

```text
Hierarchy → active NPC instance → NPCDialogueWindow
→ Inspector → NPCDialogueWindow → UI, Reaction Dialogue or Typing
```

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

[Back to contents](#contents)

---

## 10. Reaction Rules

Reaction rules are stored in an NPC profile:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
→ select a profile → Inspector → Reaction Rules → choose an element
```

Increase the list size to add a rule. Expand the element and configure its `Condition`, optional number conditions and `Reaction` fields.

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

### Available Reaction Events

`Event Type` selects the kind of project action accepted by a rule:

| Event Type | Intended use |
| --- | --- |
| `None` | Empty/default value. Do not use for normal reactions. |
| `ObjectClicked` | A gameplay object was clicked. |
| `FlyerRead` | The player read a flyer. |
| `RecordSelected` | The player selected a record. |
| `RecordPlayed` | A record was played. |
| `CabinetOpened` | A cabinet was opened. |
| `PlayerEnteredRoom` | The player entered the NPC's area. |
| `PlayerLeftRoom` | The player left the NPC's area. |
| `NumericValueChanged` | A shared integer value changed. This is used by the record counter. |

The enum value alone does not detect an action. The system that owns the action must create an `NPCReactionContext` and call `NPCController.ReactTo`. `NPCNumericReactionListener` provides this connection automatically only for a `GameIntValueSource`.

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

For a decimal minimum threshold:

```text
Use Float Modulus:       false
Use Minimum Float Value: true
Minimum Float Value:     chosen threshold
```

For a decimal remainder check:

```text
Use Float Modulus:       true
Use Minimum Float Value: true
Minimum Float Value:     divisor
```

The float settings follow the same minimum-versus-modulus structure as the integer settings.
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

### Complete Record Counter Connection

The existing record reaction follows this complete path:

```text
VinylSelectController.GoToVinylPlayer
→ GlobalPlayedRecordsCounter.IncreaseRecordsPlayed
→ GlobalPlayedRecordsCounter.ValueChanged
→ B_Seiler NPCNumericReactionListener
→ NPCReactionContext with sourceId records_played
→ B_Seiler_Profile Reaction Rules
→ NPCController.ReactTo
→ short reaction text in NPCDialogueWindow
```

`GoToVinylPlayer` currently increases the counter when the record-player transition is requested. This is the exact technical trigger used by the project.

Set up the scene connection:

1. Open `Assets/Scenes/Main/main.unity`.
2. Select `GlobalPlayedRecordsCounter` in the Hierarchy.
3. Confirm that it has the `GlobalPlayedRecordsCounter` component.
4. Select the Burkhardt Seiler prefab instance in the Hierarchy.
5. In `NPCNumericReactionListener → References`, drag the `GlobalPlayedRecordsCounter` component into `Value Source`.
6. In `NPCNumericReactionListener → Reaction Context`, set `Source Id` to `records_played`.
7. Keep `Evaluate Current Value On Enable` enabled when the NPC should also evaluate a count collected before it became active.

Set up the profile rule:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
→ B_Seiler_Profile → Inspector → Reaction Rules → Element 0
```

Use these values:

| Field | Value |
| --- | --- |
| `Event Type` | `NumericValueChanged` |
| `Required Object Id` | `records_played` |
| `Use Int Modulus` | enabled |
| `Use Minimum Int Value` | enabled |
| `Minimum Int Value` | `6` |
| `Reaction Text` | Text shown after every sixth count |

This matches the values `6`, `12`, `18` and every later number divisible by six. Enable `Change Mood` and select `Resulting Mood` when the reaction should also change movement behavior. Enable `Block Original Action` only when the calling gameplay system reads the returned result and stops its own action.

[Back to contents](#contents)

---

## 11. Numeric Reaction Listener Settings

Select the listener through:

```text
Hierarchy → NPC instance → Inspector → NPCNumericReactionListener
```

Add it with **Add Component** when the NPC should react to a shared integer source. Its Inspector is divided into `References`, `Reaction Context` and `Behaviour`.

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

[Back to contents](#contents)

---

## 12. Ambient Speech Settings

Select these settings through:

```text
Hierarchy → NPC instance → Inspector → NPCAmbientSpeech
```

Add the component with **Add Component** when it is missing. Its Inspector is divided into `References`, `Detection` and `Lines`.

### Player Anchor

Player or camera transform used for the distance check.
Assign this field to enable ambient speech.

`Player Anchor` is a scene reference. In the Main scene, drag `MainCamera` or another Transform that follows the player from the Hierarchy into this field on the NPC prefab instance.

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

In the provided prefabs, `B_Seiler` contains ambient lines. `C_Lippke` and `F_Butzmann` have empty line lists, so their ambient components remain silent until lines and a scene `Player Anchor` are assigned.

[Back to contents](#contents)

---

## 13. Optional and Unconnected Parts

### NPCMoodData.speechBubbleText

This value stores text for a mood. Add a speech component that reads it to display mood-based text.

### NPCProfile.interactions and NPCInteractionMenu

`NPCInteractionHandler` opens `NPCDialogueWindow` directly. The interaction menu becomes available after it is connected to the gameplay flow.

To use the interaction menu, provide the following connections:

- Call `NPCInteractionMenu.Open()` from the intended input or interaction path.
- Create concrete classes that inherit from `NPCInteraction`.
- Add the resulting interaction assets to the profile interaction lists.

The menu component itself is configured through:

```text
Hierarchy → NPC instance → NPCInteractionMenu
→ Inspector → NPCInteractionMenu → UI
```

Its fields are:

| Field | Purpose |
| --- | --- |
| `Root` | Top-level menu object shown while the menu is open. |
| `Button Parent` | Container that receives generated interaction buttons. |
| `Button Prefab` | Button cloned for each interaction. It needs a TextMeshPro text child. |
| `Empty Text` | Optional message shown when the profile interaction list is empty. |

To create an interaction type, make a concrete C# class that inherits from `NPCInteraction` and implement `Execute(NPCController npc)`. Create its ScriptableObject asset, set `Interaction Name`, and add it here:

```text
Project window → Assets → NPC → ScriptableObjects → Profiles
→ select the NPC profile → Inspector → Interactions
```

The menu becomes part of the click flow only after another component calls `NPCInteractionMenu.Instance.Open(npc)`. Decide whether that call should replace the direct dialogue window or happen from a dialogue choice before connecting it.

[Back to contents](#contents)

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

[Back to contents](#contents)

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

### Dialogue choices cannot be clicked

Check:

- An active `EventSystem` exists in one loaded scene.
- The dialogue Canvas has a `GraphicRaycaster`.
- `Choice Text Prefab` is assigned on `NPCDialogueWindow`.
- The choice text object has Raycast Target enabled.
- Another full-screen UI object is not blocking pointer input above the dialogue.

### Interaction remains blocked after dialogue

Check:

- Only one active `GlobalInteractionState` exists.
- The dialogue ends through a choice, Continue button or the configured close button.
- `NPCDialogueWindow.CloseConversation` reaches `NPCController.EndInteraction`.
- The dialogue UI object was not disabled externally before its normal close flow finished.

### Foot sliding during walking

Check:

1. The mood's `Move Speed`.
2. `Reference Walk Speed` on `NPCAnimationController`.
3. `Animation Speed Multiplier`.
4. The scale and import settings of the animation/model.

Start by matching reference speed to the normal mood movement speed. Then tune the multiplier by watching the feet move against the floor.

### Animator reports a missing parameter

Check the Animator Controller's **Parameters** tab. Names and types are case-sensitive:

- `Speed`: Float
- `MoveSpeed`: Float
- `RandomIdle`: Int
- `IsInteracting`: Bool for controllers with interaction animation

Also confirm that `NPCAnimationController.Animator` points to the model using that Animator Controller.

### NPC cannot reach a destination

Check:

- `Ground → NavMesh Surface` has been baked after the latest geometry changes.
- The NPC root and target Transform are on the visible NavMesh.
- The NPC's NavMesh Agent Type matches the NavMesh Surface Agent Type.
- Walls or disconnected NavMesh islands do not separate the NPC and target.
- Agent Radius and Height fit through the available space.

### Numeric reaction never happens

Check:

- `Value Source` is assigned.
- `Source Id` exactly matches `Required Object Id`.
- Event type is `NumericValueChanged`.
- Integer options are configured correctly.
- A modulus divisor is greater than zero.
- The intended rule appears before broader matching rules.
- `GlobalPlayedRecordsCounter` is active when testing the record-count reaction.
- The scene instance of B Seiler has that counter assigned as the listener's `Value Source`.

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

### Scene reference disappears from a prefab

Scene objects such as patrol points, `MainCamera` and `GlobalPlayedRecordsCounter` belong to a loaded scene. Assign them on the prefab instance in the Hierarchy. Store only reusable project assets and prefab-internal objects on the prefab asset.

[Back to contents](#contents)

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

[Back to contents](#contents)
