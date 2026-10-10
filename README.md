# Zensor — Preserving Berlin's lost sound

An interactive digital reconstruction of West Berlin's historic Zensor record store, preserving its music, people, and underground culture through an explorable desktop experience.

## About

The project reconstructs the historic Berlin record store **Zensor** as an interactive desktop application. Its aim is to bring an important site of West Berlin’s music scene and subculture of the late 1970s and early 1980s to life digitally, preserving its history and making it accessible to present-day generations. Visitors can explore the store independently, discover original records, posters, and documents, and interact with historical content. In this way, the project combines digital reconstruction, music history, and cultural education to create an immersive experience.

The application allows users to interactively explore the reconstructed Zensor. Using point-and-click controls, they can navigate the shop floor and the historic mail-order office. Original records can be taken from the record bins, viewed from all angles, and played on a fully functional turntable. Historic posters, photographs, and newspaper articles provide additional background information, while a virtual representation of Burkhardt Seiler and other staff members bring the store to life. A black-and-white visual concept, with interactive objects highlighted in colour, makes navigation easier and directs the user’s attention towards explorable content.

The reconstructed mail-order room illustrates the organisational work behind the record store and demonstrates how Zensor’s mail-order business and international network allowed it to have an impact far beyond Berlin.

## The Zensor: Burkhardt Seiler

Burkhardt Seiler (1953–2023), widely known as “Zensor”, was a key figure in West Berlin’s independent and underground music scene. As a record-store owner, label operator, publisher, concert promoter, and cultural networker, he introduced audiences to international artists, emerging genres, and experimental music. Together with journalist Hans Keller, he coined the term “Neue Deutsche Welle”. His Zensor record store, opened in Berlin-Schöneberg in 1979, was far more than a place to buy records: it served as a meeting point for musicians, a concert venue, a mail-order business, and a hub connecting Berlin’s music scene with the wider world. Seiler was remembered as unconventional, direct, and sometimes difficult. His nickname—and later the store’s name—originated from his joking response whenever a requested record was unavailable: “It’s censored”.

## The spirit: Robert M. Stanley

Rob Stanley is CPO at EAB – European Artist Bank and project lead of “Zensor XR”, a cultural heritage project that reconstructs the legendary West Berlin record store "Zensor Schallplatten" as an immersive Unity/XR experience. His work explores how artists, archives, and lost cultural spaces can be preserved, revitalised, and made accessible through digital tools, storytelling, and community engagement. At the heart of his work is the belief that cultural memory should not merely be preserved, but brought back to life and made accessible.

## Features

- Point-and-click exploration using fixed camera spots and cinematic transitions
- A black-and-white world in which interactive objects reveal their colour on hover
- Record browsing, cover and disc inspection, metadata panels, and playback on an interactive turntable
- Historical photo walls with interactive posters, photographs, and background information
- NPCs with patrol behaviour, ambient speech, branching dialogue, and reactions to user actions
- A reconstructed mail-order office illustrating Zensor's international reach
- Spatial background music played through in-world speakers

## Technology

### Unity

The experience is built with Unity 6 (6000.3.14f1) and C#, using the Universal Render Pipeline, Cinemachine, the Unity Input System, NavMesh, Unity UI, and TextMesh Pro.

### 3D Asset Pipeline

Environment and record assets are created or adapted in Blender and imported into Unity as optimised FBX models. Reusable materials and UV layouts allow new record covers and labels to be added by assigning image textures and a RecordData asset. NPCs use Microsoft Rocketbox humanoid characters with Mixamo animations retargeted through Unity's Humanoid rig.

## Setup & Running

### Prerequisites

- Unity Hub
- Unity 6 (6000.3.14f1) with the Universal Render Pipeline

### Getting Started

1. Clone the repository.
2. In Unity Hub, select Add project from disk and choose the project folder.
3. Open the project with Unity 6000.3.14f1.
4. Open Assets/Scenes/Main/main.unity. All required subscenes are loaded automatically.
5. Press Play to run the application in the Unity Editor.

## Project Structure

```
Assets/
├─ Art/
│  ├─ Models/
│  ├─ Textures/
│  ├─ Materials/
│  └─ Sprites/
├─ Audio/
│  ├─ Music/
│  ├─ SFX/
│  ├─ Dialogue/
│  └─ Metadata/ (songs.json)
├─ Prefabs/
│  ├─ Environment/ (Tresen, Regal, WandPanel)
│  ├─ Interactions/ (AudioPlayer, InfoPanel)
│  └─ Avatar/
├─ Scenes/
│  ├─ Main
│  ├─ Tresen
│  └─ Wand
├─ Scripts/
│  ├─ Core/ (GameManager, AudioManager, InputManager)
│  ├─ Interaction/ (z.B. Interactable, TalkCounter, InfoWall, AudioPlayerController)
│  ├─ UI/ (Popup, PlaylistUI)
├─ UI/
│  ├─ Canvases/
│  ├─ Fonts/
│  └─ Icons/
├─ StreamingAssets/ (extern geladene Audio-Dateien)
└─ Documentation/
```

## Technical Architecture

- main.unity automatically loads all non-test scenes below Assets/Scenes additively, allowing feature areas to be developed in parallel.
- Record interaction is controlled by a state machine that coordinates selection, inspection, rotation, playback, and UI states.
- Shared interaction locking prevents navigation and object input from conflicting during camera transitions, dialogues, and open panels.
- ScriptableObject assets store record metadata, photo-wall content, dialogue scripts, and NPC profiles separately from runtime logic.
- The modular NPC system combines NavMesh movement, Animator-driven states, branching dialogue, ambient speech, and event-based reactions.

## Development

The team followed a Scrum-based workflow and divided development into feature areas including core systems, record interaction, UI, photo walls, and NPCs. Weekly full-team meetings were held on Wednesdays, while tasks and progress were tracked through GitHub Projects and GitHub Issues. Each feature was developed and tested in its own branch before being merged into the shared test branch. This branch served as a central integration environment and stable fallback before completed changes were transferred to main. Additive scene loading further reduced merge conflicts when several team members worked in Unity simultaneously.

See [CONTRIBUTING.md](./CONTRIBUTING.md) for contribution guidelines.

## Academic Context

Zensor is a student project at [HTW Berlin — University of Applied Sciences](https://www.htw-berlin.de/), developed in cooperation between Robert M. Stanley and the degree program Computer Science in Culture and Health.

Supervised by:
[Prof. Dr.-Ing. Johann Habakuk Israel](https://www.htw-berlin.de/hochschule/personen/person/?eid=4743)

### Team

Anton E., Lennart K., Jessica L.,  Gordian R., Muhammad R., Nils S., Nikita S., Ryu S., Phu Dat T.
