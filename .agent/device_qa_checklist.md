# Device QA Checklist

Date: 2026-05-26

## Minimum Matrix

| Device | Status | Notes |
|---|---|---|
| Windows Editor mock | Pending Unity Editor validation | Batchmode is blocked while the project is open in another Unity instance. |
| Android ARCore | Pending physical device | Required before production acceptance. |
| iOS ARKit | Pending physical device | Required only if iOS is a target. |

## Manual Pass Criteria

- Camera permission granted path works.
- Camera permission denied path shows a clear message and can return to menu.
- Unsupported device path shows a clear message.
- Plane scanning starts and gives visible guidance.
- Learning area can be placed and reset.
- Plane visualizer can be hidden after placement.
- Objects spawn inside the learning area.
- Tap/count/select interactions work without drag side effects.
- Tracking lost/recover path pauses or warns without saving a learning mistake.
- Round result is saved with lesson id, skill tags, and issue type when applicable.
- Dashboard updates after returning from gameplay.

## Performance Watch Points

- Target frame rate: 60 FPS.
- Per-group learning object budget: 12.
- Total visible learning object budget: 48.
- Watch for spikes on round transition, VFX burst, and object cleanup.
