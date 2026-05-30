using System.Collections.Generic;
using UnityEngine;

namespace Features.Activities.NumberBonds
{
    public partial class NumberBondsView
    {
        private void HandleObjectDropped(NumberBondObjectView objectView, NumberBondZoneView targetZone)
        {
            if (objectView == null)
            {
                return;
            }

            if (targetZone == null || targetZone.Zone == objectView.CurrentZone)
            {
                objectView.ReturnToLastStablePosition();
                RefreshObjectSlots(objectView.CurrentZone);
                return;
            }

            OnObjectMoveRequested?.Invoke(new NumberBondMoveRequest(
                objectView.ObjectId,
                objectView.CurrentZone,
                targetZone.Zone));
        }

        private void InitializeZoneLists()
        {
            zoneObjects.Clear();
            zoneObjects[BondZone.Whole] = new List<NumberBondObjectView>();
            zoneObjects[BondZone.PartA] = new List<NumberBondObjectView>();
            zoneObjects[BondZone.PartB] = new List<NumberBondObjectView>();
        }

        private void CreateZones(NumberBondRoundState state)
        {
            zoneRoot = new GameObject("NumberBondZones");
            if (contentRoot != null)
            {
                zoneRoot.transform.SetParent(contentRoot, false);
            }

            zoneViews.Clear();
            zoneViews[BondZone.Whole] = CreateZone(BondZone.Whole, "T\u1ed5ng", state.WholeLocked, new Vector3(0f, 0f, 0.48f));
            zoneViews[BondZone.PartA] = CreateZone(BondZone.PartA, "Ph\u1ea7n A", state.PartALocked, new Vector3(-0.64f, 0f, -0.44f));
            zoneViews[BondZone.PartB] = CreateZone(BondZone.PartB, "Ph\u1ea7n B", state.PartBLocked, new Vector3(0.64f, 0f, -0.44f));
        }

        private NumberBondZoneView CreateZone(BondZone zone, string title, bool locked, Vector3 localPosition)
        {
            var go = new GameObject($"NumberBondZone_{zone}");
            go.transform.SetParent(zoneRoot.transform, false);
            go.transform.localPosition = localPosition;
            NumberBondZoneView zoneView = go.AddComponent<NumberBondZoneView>();
            zoneView.Initialize(zone, title, locked, config.ZoneRadiusMeters, config.ZoneHitRadiusMeters);
            return zoneView;
        }

        private void CreateConnectionLines()
        {
            lineRoot = new GameObject("NumberBondLines");
            if (contentRoot != null)
            {
                lineRoot.transform.SetParent(contentRoot, false);
            }

            CreateLine("WholeToPartA", zoneViews[BondZone.Whole].transform.position, zoneViews[BondZone.PartA].transform.position);
            CreateLine("WholeToPartB", zoneViews[BondZone.Whole].transform.position, zoneViews[BondZone.PartB].transform.position);
        }

        private void CreateLine(string name, Vector3 from, Vector3 to)
        {
            var go = new GameObject(name);
            go.transform.SetParent(lineRoot.transform, true);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.widthMultiplier = 0.026f;
            line.material = GetLineMaterial();
            line.startColor = new Color(1f, 1f, 1f, 0.65f);
            line.endColor = line.startColor;
            line.SetPosition(0, from + Vector3.up * 0.035f);
            line.SetPosition(1, to + Vector3.up * 0.035f);
        }

        private void SpawnInitialObjects(NumberBondRoundState state)
        {
            SpawnObjectsInZone(BondZone.Whole, state.WholeCount, !state.WholeLocked);
            SpawnObjectsInZone(BondZone.PartA, state.PartACount, !state.PartALocked);
            SpawnObjectsInZone(BondZone.PartB, state.PartBCount, !state.PartBLocked);
        }

        private void SpawnObjectsInZone(BondZone zone, int count, bool movable)
        {
            for (int i = 0; i < count; i++)
            {
                string objectId = $"NB_Object_{++objectSequence}";
                Vector3 position = zoneViews[zone].GetSlotPosition(i, count);
                GameObject prefab = GetObjectPrefab(objectSequence);
                GameObject obj = prefab != null
                    ? placementService.SpawnAtPosition(prefab, position, Quaternion.identity, contentRoot)
                    : GameObject.CreatePrimitive(PrimitiveType.Sphere);

                if (obj == null)
                {
                    continue;
                }

                obj.name = objectId;
                obj.transform.position = position;
                ActivityPrefabSetup.Instance?.PrepareLearningObject(obj);

                NumberBondObjectView objectView = obj.GetComponent<NumberBondObjectView>();
                if (objectView == null)
                {
                    objectView = obj.AddComponent<NumberBondObjectView>();
                }

                objectView.Initialize(objectId, zone, movable);
                objectViews[objectId] = objectView;
                zoneObjects[zone].Add(objectView);
                interactionService?.RegisterInteractable(obj, objectId);
            }
        }

        private GameObject GetObjectPrefab(int index)
        {
            ActivityPrefabSetup setup = ActivityPrefabSetup.Instance;
            if (setup == null)
            {
                return null;
            }

            if (currentQuestion != null && !string.IsNullOrWhiteSpace(currentQuestion.ObjectPrefabName))
            {
                GameObject namedPrefab = setup.GetPrefab(currentQuestion.ObjectPrefabName);
                if (namedPrefab != null)
                {
                    return namedPrefab;
                }
            }

            return setup.GetAnimalPrefab(index) ?? setup.GetLearningObjectPrefab();
        }

        private void ConfigureDragAdapter()
        {
            if (dragAdapter == null)
            {
                dragAdapter = gameObject.AddComponent<NumberBondDragAdapter>();
            }

            dragAdapter.OnObjectDropped -= HandleObjectDropped;
            dragAdapter.OnObjectDropped += HandleObjectDropped;
            dragAdapter.OnZoneTapped -= HandleZoneTapped;
            dragAdapter.OnZoneTapped += HandleZoneTapped;
            dragAdapter.Configure(Camera.main, zoneViews.Values);
            dragAdapter.SetInputEnabled(true);
        }

        private void HandleZoneTapped(NumberBondZoneView targetZone)
        {
            if (targetZone == null)
            {
                return;
            }

            NumberBondObjectView objectView = FindMovableObjectForZoneTap(targetZone.Zone);
            if (objectView == null || objectView.CurrentZone == targetZone.Zone)
            {
                return;
            }

            OnObjectMoveRequested?.Invoke(new NumberBondMoveRequest(
                objectView.ObjectId,
                objectView.CurrentZone,
                targetZone.Zone));
        }

        private NumberBondObjectView FindMovableObjectForZoneTap(BondZone targetZone)
        {
            BondZone[] sourcePriority = targetZone switch
            {
                BondZone.PartA => new[] { BondZone.Whole, BondZone.PartB },
                BondZone.PartB => new[] { BondZone.Whole, BondZone.PartA },
                BondZone.Whole => new[] { BondZone.PartB, BondZone.PartA },
                _ => new[] { BondZone.Whole }
            };

            for (int sourceIndex = 0; sourceIndex < sourcePriority.Length; sourceIndex++)
            {
                BondZone sourceZone = sourcePriority[sourceIndex];
                if (!zoneObjects.TryGetValue(sourceZone, out List<NumberBondObjectView> objects))
                {
                    continue;
                }

                for (int i = objects.Count - 1; i >= 0; i--)
                {
                    NumberBondObjectView objectView = objects[i];
                    if (objectView != null && objectView.IsMovable)
                    {
                        return objectView;
                    }
                }
            }

            return null;
        }

        private void RefreshObjectSlots(BondZone zone)
        {
            List<NumberBondObjectView> objects = zoneObjects[zone];
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].SnapTo(zoneViews[zone].GetSlotPosition(i, objects.Count));
                }
            }
        }

        private void SetZoneCount(BondZone zone, int count)
        {
            if (zoneViews.TryGetValue(zone, out NumberBondZoneView zoneView) && zoneView != null)
            {
                zoneView.SetCount(count);
            }
        }

        private void ShowQuestionText(NumberBondsQuestion question)
        {
            if (instructionText == null || question == null)
            {
                return;
            }

            instructionText.text = question.Mode == NumberBondMode.TargetSplit
                ? $"T\u00ecm ph\u1ea7n c\u00f2n thi\u1ebfu c\u1ee7a {question.WholeTarget}"
                : $"Chia {question.WholeTarget} th\u00e0nh 2 nh\u00f3m";
        }

        private Material GetLineMaterial()
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default")
                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                    ?? Shader.Find("Standard");
                lineMaterial = new Material(shader);
            }

            return lineMaterial;
        }

        private static void FitObjectHeight(GameObject obj, float targetHeight)
        {
            if (obj == null || targetHeight <= 0f)
            {
                return;
            }

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.y > 0.0001f)
            {
                obj.transform.localScale *= targetHeight / bounds.size.y;
            }
        }
    }
}
