# Activity Scene Setup Guide

This guide explains how to set up the learning activities in `SC_ARGameplay.unity`.

## Quick Setup (For Testing)

1. Open `SC_ARGameplay.unity` scene
2. Create an empty GameObject named `ActivityPrefabSetup`
3. Add the `ActivityPrefabSetup` component
4. Enable "Auto-Create Placeholders" - this creates test prefabs automatically
5. Create child GameObjects for each activity:
   - `QuantityMatchActivity` (add `QuantityMatchPresenter` and `QuantityMatchView`)
   - `CompareQuantityActivity` (add `CompareQuantityPresenter` and `CompareQuantityView`)
   - `NumberLineJumpActivity` (add `NumberLineJumpPresenter` and `NumberLineJumpView`)

## Production Setup

### Step 1: Create Activity Configs

Create ScriptableObject configs for each activity:

1. Right-click in Project window → Create → AR Learning → Quantity Match Config
2. Name it `SO_QuantityMatchConfig_Easy`
3. Fill in questions, hints, feedback strings
4. Repeat for Compare Quantity and Number Line Jump

### Step 2: Set Up Activity GameObjects

For each activity:

```
ActivityManager (GameObject)
├── QuantityMatchActivity
│   ├── QuantityMatchPresenter (Component)
│   └── QuantityMatchView (Component)
├── CompareQuantityActivity
│   ├── CompareQuantityPresenter (Component)
│   └── CompareQuantityView (Component)
└── NumberLineJumpActivity
    ├── NumberLineJumpPresenter (Component)
    └── NumberLineJumpView (Component)
```

### Step 3: Wire Up References

**QuantityMatchPresenter:**
- Config: Assign `SO_QuantityMatchConfig_Easy`
- View: Drag `QuantityMatchView` GameObject
- Placement Service: Assign AR service implementation
- Interaction Service: Assign AR service implementation

**QuantityMatchView:**
- Presenter: Drag `QuantityMatchPresenter` component
- Apple Prefab: Assign `PFB_Apple` prefab
- Carrot Prefab: Assign `PFB_Carrot` prefab
- UI Panel: Assign `PFB_QuantityMatchPanel` prefab

Repeat for other activities.

### Step 4: Add UI Panels

Create UI canvases for each activity:

- `PFB_QuantityMatchPanel`: Show target number, answer buttons, feedback
- `PFB_CompareQuantityPanel`: Show two groups, More/Fewer/Equal buttons
- `PFB_NumberLineJumpPanel`: Show equation, arrow buttons, hint button

## Required AR Services

The activities depend on these interfaces being implemented:

- `IARPlacementService` - Spawns objects in AR space
- `IARInteractionService` - Handles tap/select input
- `IARSessionService` - Tracks AR session state

See `LearningActivities_ImplementationSummary.md` section 5 for interface specifications.

## Testing Without AR

To test activities without AR hardware:

1. Use `ActivityPrefabSetup` with placeholder prefabs
2. Create mock implementations of AR services that:
   - `SpawnAtPosition()` - Instantiates prefabs at a fixed world position
   - `RegisterInteractable()` - Adds a BoxCollider for mouse clicks
   - `IsSessionReady` - Always returns true

3. Run in Unity Editor and interact with mouse clicks

## Activity Launch Order

Typical flow:

1. Boot Scene → Main Menu
2. Main Menu → Activity Select
3. Activity Select → AR Gameplay (with selected activity)
4. AR Gameplay:
   - Initialize AR session
   - Detect plane
   - Load activity presenter
   - Show view
   - Run activity rounds
5. Complete → Progress Dashboard
6. Progress Dashboard → Main Menu or Next Activity
