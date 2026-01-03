using UnityEngine;
using UnityEditor;
using Level;
using World;
using System.Collections.Generic;

namespace Editor
{
    public class LevelDataEditor : EditorWindow
    {
        private LevelData _levelData;
        private Vector2 _scrollPos;
        private float _zoom = 0.1f; // pixels per segment

        // Layout settings for the two columns
        private const float RoadColumnX = 10f;
        private const float WorldColumnX = 220f;
        private const float ColumnWidth = 180f;

        // Selected modifier info (0 = road modifier, 1 = world modifier)
        private int _selectedModifierList = -1;
        private int _selectedModifierIndex = -1;

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelDataEditor>("Level Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            // Allow the user to assign a LevelData asset
            _levelData = EditorGUILayout.ObjectField("Level Data", _levelData, typeof(LevelData), false) as LevelData;
            if (!_levelData)
            {
                EditorGUILayout.HelpBox("Please assign a LevelData asset.", MessageType.Info);
                return;
            }

            // Zoom slider for adjusting the vertical scale of the track view
            _zoom = EditorGUILayout.Slider("Zoom (pixels/segment)", _zoom, 0.1f, 2f);

            // Calculate the total height of the track view (each segment gets 'zoom' pixels)
            float trackViewHeight = _levelData.Segments * _zoom;

            // Wrap the drawing in a scroll view.
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(400));

            // Draw background rectangles for each column.
            Rect roadRect = new Rect(RoadColumnX, 0, ColumnWidth, trackViewHeight);
            Rect worldRect = new Rect(WorldColumnX, 0, ColumnWidth, trackViewHeight);
            EditorGUI.DrawRect(roadRect, Color.gray);
            EditorGUI.DrawRect(worldRect, new Color(0.8f, 0.8f, 0.8f));

            // Draw horizontal grid lines every 100 segments using DrawRect.
            for (int i = 0; i <= _levelData.Segments; i += 100)
            {
                float y = i * _zoom;
                // Top grid line in road column.
                EditorGUI.DrawRect(new Rect(RoadColumnX, y, ColumnWidth, 2), Color.black);
                // Top grid line in world column.
                EditorGUI.DrawRect(new Rect(WorldColumnX, y, ColumnWidth, 2), Color.black);
            }

            // Draw each road property modifier as a semi-transparent blue bar.
            if (_levelData.RoadPropertiesModifiers != null)
            {
                for (int i = 0; i < _levelData.RoadPropertiesModifiers.Count; i++)
                {
                    LevelModifier mod = _levelData.RoadPropertiesModifiers[i];
                    float yStart = mod.segments.x * _zoom;
                    float boxHeight = (mod.segments.y - mod.segments.x) * _zoom;
                    Rect modRect = new Rect(RoadColumnX, yStart, ColumnWidth, boxHeight);
                    EditorGUI.DrawRect(modRect, new Color(0, 0, 1, 0.5f));
                    // Draw a thick border using DrawRect.
                    DrawRectBorder(modRect, Color.black, 3);
                    GUI.Label(new Rect(modRect.x, modRect.y, modRect.width, 20), "Road Mod " + i);

                    if (Event.current.type == EventType.MouseDown && modRect.Contains(Event.current.mousePosition))
                    {
                        _selectedModifierList = 0;
                        _selectedModifierIndex = i;
                        Repaint();
                    }
                }
            }

            // Draw each world object modifier as a semi-transparent green bar.
            if (_levelData.WorldObjectsModifiers != null)
            {
                for (int i = 0; i < _levelData.WorldObjectsModifiers.Count; i++)
                {
                    LevelModifier mod = _levelData.WorldObjectsModifiers[i];
                    float yStart = mod.segments.x * _zoom;
                    float boxHeight = (mod.segments.y - mod.segments.x) * _zoom;
                    Rect modRect = new Rect(WorldColumnX, yStart, ColumnWidth, boxHeight);
                    EditorGUI.DrawRect(modRect, new Color(0, 1, 0, 0.5f));
                    DrawRectBorder(modRect, Color.black, 3);
                    GUI.Label(new Rect(modRect.x, modRect.y, modRect.width, 20), "World Mod " + i);

                    if (Event.current.type == EventType.MouseDown && modRect.Contains(Event.current.mousePosition))
                    {
                        _selectedModifierList = 1;
                        _selectedModifierIndex = i;
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            // Buttons to add/remove modifiers.
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Road Modifier"))
            {
                Undo.RecordObject(_levelData, "Add Road Modifier");
                if (_levelData.RoadPropertiesModifiers == null)
                    _levelData.RoadPropertiesModifiers = new List<LevelModifier>();
                _levelData.RoadPropertiesModifiers.Add(new LevelModifier());
                EditorUtility.SetDirty(_levelData);
            }
            if (_selectedModifierList == 0 && _selectedModifierIndex >= 0)
            {
                if (GUILayout.Button("Remove Selected Road Modifier"))
                {
                    Undo.RecordObject(_levelData, "Remove Road Modifier");
                    if (_levelData.RoadPropertiesModifiers != null)
                        _levelData.RoadPropertiesModifiers.RemoveAt(_selectedModifierIndex);
                    _selectedModifierIndex = -1;
                    EditorUtility.SetDirty(_levelData);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add World Modifier"))
            {
                Undo.RecordObject(_levelData, "Add World Modifier");
                if (_levelData.WorldObjectsModifiers == null)
                    _levelData.WorldObjectsModifiers = new List<LevelModifier>();
                _levelData.WorldObjectsModifiers.Add(new LevelModifier());
                EditorUtility.SetDirty(_levelData);
            }
            if (_selectedModifierList == 1 && _selectedModifierIndex >= 0)
            {
                if (GUILayout.Button("Remove Selected World Modifier"))
                {
                    Undo.RecordObject(_levelData, "Remove World Modifier");
                    if (_levelData.WorldObjectsModifiers != null)
                        _levelData.WorldObjectsModifiers.RemoveAt(_selectedModifierIndex);
                    _selectedModifierIndex = -1;
                    EditorUtility.SetDirty(_levelData);
                }
            }
            EditorGUILayout.EndHorizontal();

            // If a modifier was selected, show an editing panel below.
            if (_selectedModifierIndex >= 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Selected Modifier", EditorStyles.boldLabel);
                LevelModifier selectedMod = null;
                if (_selectedModifierList == 0 && _levelData.RoadPropertiesModifiers != null && _levelData.RoadPropertiesModifiers.Count > _selectedModifierIndex)
                {
                    selectedMod = _levelData.RoadPropertiesModifiers[_selectedModifierIndex];
                }
                else if (_selectedModifierList == 1 && _levelData.WorldObjectsModifiers != null && _levelData.WorldObjectsModifiers.Count > _selectedModifierIndex)
                {
                    selectedMod = _levelData.WorldObjectsModifiers[_selectedModifierIndex];
                }

                if (selectedMod != null)
                {
                    EditorGUI.BeginChangeCheck();

                    // Edit common fields.
                    selectedMod.label = EditorGUILayout.TextField("Label", selectedMod.label);
                    selectedMod.levelModifierType = (LevelModifierType)EditorGUILayout.EnumPopup("Modifier Type", selectedMod.levelModifierType);
                    selectedMod.frequency = EditorGUILayout.IntField("Frequency", selectedMod.frequency);
                    int newStart = EditorGUILayout.IntField("Start Segment", selectedMod.segments.x);
                    int newEnd = EditorGUILayout.IntField("End Segment", selectedMod.segments.y);
                    if (newStart != selectedMod.segments.x || newEnd != selectedMod.segments.y)
                    {
                        selectedMod.segments = new Vector2Int(newStart, newEnd);
                    }

                    if (selectedMod.IsRoadProperty)
                    {
                        selectedMod.curve = EditorGUILayout.FloatField("Curve", selectedMod.curve);
                        selectedMod.h = EditorGUILayout.FloatField("Height Distortion (h)", selectedMod.h);
                        selectedMod.oscillationNodes = EditorGUILayout.FloatField("Oscillation Nodes", selectedMod.oscillationNodes);
                    }

                    if (selectedMod.IsWorldObject && selectedMod.worldObjects != null)
                    {
                        EditorGUILayout.LabelField("World Objects", EditorStyles.boldLabel);
                        for (int i = 0; i < selectedMod.worldObjects.Count; i++)
                        {
                            StaticWorldObject worldObj = selectedMod.worldObjects[i];
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField("World Object " + i);
                            Sprite previewSprite = worldObj.Preview;
                            EditorGUILayout.ObjectField("Sprite Preview", previewSprite, typeof(Sprite), false);
                            EditorGUILayout.EndVertical();
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(_levelData);
                    }
                }
            }
        }

        /// <summary>
        /// Draws a border around a rectangle using EditorGUI.DrawRect.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="color">The border color.</param>
        /// <param name="thickness">The border thickness.</param>
        private static void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            // Top border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            // Bottom border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
            // Left border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            // Right border
            EditorGUI.DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
