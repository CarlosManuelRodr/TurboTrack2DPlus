using System.Collections.Generic;
using Core;
using Core.Extensions;
using Physics;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Vehicle;
using World;

namespace Rendering
{
    /// <summary>
    /// Projects the pseudo3D world into a 2D surface by calculating the perspective projections
    /// and setting the projected coordinates into a drawable mesh.
    ///
    /// It works by reading the world state and the player vehicle state
    /// from the game controller. The script then calculates the world projection
    /// and updates the rendering meshes and textures.
    /// </summary>
    public class Pseudo3DRenderer : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] PlayerVehicle playerVehicle;

        [SerializeField] ParallaxBackground background;
        [SerializeField] SpriteRenderer frontSpritePlane;
        [SerializeField] SpriteRenderer backSpritePlane;
        [FormerlySerializedAs("grass1")] [SerializeField] MeshFilter ground1Left;
        [FormerlySerializedAs("grass2")] [SerializeField] MeshFilter ground2Left;
        [SerializeField] MeshFilter ground1Right;
        [SerializeField] MeshFilter ground2Right;
        [SerializeField] MeshFilter rumble1;
        [SerializeField] MeshFilter rumble2;
        [SerializeField] MeshFilter road1;
        [SerializeField] MeshFilter road2;
        [SerializeField] MeshFilter dashline;

        [Title("Preferences")]
        [SerializeField] bool drawDashline = true;
        [SerializeField] bool drawRumble = true;

        [Title("Camera Parameters")] 
        [SerializeField] float backgroundStartHeight = -1f;
        [SerializeField] float backgroundEndHeight = 2f;
        [SerializeField] int pixelsPerUnit = 32;
        [SerializeField] int drawingDistance = 250; // Segments
        [SerializeField] int cameraHeight = 1500;   // Pixels?
        [SerializeField] float cameraDepth = 0.84f; // Camera depth [0..1]
        [SerializeField] int screenWidthRef = 320;
        [SerializeField] int screenHeightRef = 240;
        [SerializeField] float itemSpriteScale = 25;
        [SerializeField] float vehicleSpriteScale = 25;
        [SerializeField] float parallaxSpeed = 0.1f;
        [SerializeField] int rumbleWidth = 300;
        [SerializeField] float lodDistance = 5f;
        
        /// <summary>
        /// Contains the projection coordinates and vars for drawing the quad of each
        /// road segment.
        /// </summary>
        [FormerlySerializedAs("drawDebugLines")]
        [Title("Debug")]
        [SerializeField] bool drawSegmentLines = true;
        [SerializeField] bool drawColliders = true;
        [SerializeField] ProjectedLine[] projections;
        
        /// <summary>
        /// Current camera position in world coordinates.
        /// </summary>
        public float CameraTrackPosition { get; set; }
        
        /// <summary>
        /// The current vehicle position in world coordinates.
        /// </summary>
        public float VehicleTrackPosition { get; set; }
        
        /// <summary>
        /// The camera position on the previous frame.
        /// </summary>
        public float PreviousCameraTrackPosition { get; private set; }
        
        /// <summary>
        /// The x position of the camera that follows the vehicle.
        /// </summary>
        public float CameraHorizontalPosition { get; set; }
        
        /// <summary>
        /// Shortcut for the currently loaded track.
        /// </summary>
        private LoadedWorld LoadedWorld => GameController.I.LoadedWorld;
        
        /// <summary>
        /// Get this object layer.
        /// </summary>
        private int Layer => gameObject.layer;
        
        /// <summary>
        /// A quad collection generates the meshes for each road element such as grass, the road, dash-lines, etc.
        /// </summary>
        private QuadCollection[] _quadCollections;
        
        /// <summary>
        /// Association of the mesh filter to the corresponding quad collection.
        /// </summary>
        private Dictionary<MeshFilter, QuadCollection> _dictionary = new();
        
        // Private members.
        private MaterialPropertyBlock _materialPropertyBlock;
        private RenderTexture _backTargetTexture;
        private RenderTexture _frontTargetTexture;
        private float _halfScreenWidth;
        private float _halfScreenHeight;
        private float _playerPos;
        private float _cameraPos;
        private float _speed;

        #region Monobehavior entry points and initialization

        /// <summary>
        /// Initialize the renderer. Crates quad collections, projection arrays and drawing texture.
        /// </summary>
        public void Initialize()
        {
            GameLogger.Info("Initializing renderer.", this);
            
            // Initialize projection array.
            projections = new ProjectedLine[LoadedWorld.Length];
            
            _quadCollections = new QuadCollection[9];
            for (int i = 0; i < 9; i++)
                _quadCollections[i] = new QuadCollection(drawingDistance);
            
            _dictionary = new Dictionary<MeshFilter, QuadCollection>
            {
                { ground1Left,    _quadCollections[0] },
                { ground2Left,    _quadCollections[1] },
                { ground1Right,   _quadCollections[2] },
                { ground2Right,   _quadCollections[3] },
                { rumble1,        _quadCollections[4] },
                { rumble2,        _quadCollections[5] },
                { road1,          _quadCollections[6] },
                { road2,          _quadCollections[7] },
                { dashline,       _quadCollections[8] }
            };
            
            // Initialize meshes for each MeshFilter
            foreach (var meshFilter in _dictionary.Keys)
                meshFilter.sharedMesh.MarkDynamic(); // Optimizes the mesh for frequent updates,
            
            // Initialize sprite rendering textures.
            _materialPropertyBlock = new MaterialPropertyBlock();
            _backTargetTexture = new RenderTexture(screenWidthRef, screenHeightRef, 0);
            _frontTargetTexture = new RenderTexture(screenWidthRef, screenHeightRef, 0);
            _backTargetTexture.Create();
            _frontTargetTexture.Create();

            Texture2D frontItemsTexture = new Texture2D(screenWidthRef, screenHeightRef, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            Texture2D backItemsTexture = new Texture2D(screenWidthRef, screenHeightRef, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            frontSpritePlane.sprite = Sprite.Create(frontItemsTexture, new Rect(0, 0, screenWidthRef, screenHeightRef), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            backSpritePlane.sprite = Sprite.Create(backItemsTexture, new Rect(0, 0, screenWidthRef, screenHeightRef), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            frontSpritePlane.sprite.name = "frontSpritePlane";
            backSpritePlane.sprite.name = "backSpritePlane";
            
            // Initialize vars.
            CameraTrackPosition = playerVehicle.VehicleState.trackPosition;
            PreviousCameraTrackPosition = CameraTrackPosition;
            
            UpdateProjections();
        }

        private void OnDestroy()
        {
            if (_backTargetTexture)
            {
                _backTargetTexture.Release();
                Destroy(_backTargetTexture);
            }
            
            if (_frontTargetTexture)
            {
                _frontTargetTexture.Release();
                Destroy(_frontTargetTexture);
            }
        }

        /// <summary>
        /// Render the game scene.
        /// </summary>
        public void Render()
        {
            UpdateMeshes();
            DrawTrackSprites();
        }
        
        #endregion
        
        #region Drawing functions
        
        /// <summary>
        /// Update the background position to simulate a parallax effect.
        /// </summary>
        public void MoveParallaxBackground()
        {
            Vector3 displacement = new Vector3(parallaxSpeed / pixelsPerUnit * _speed * Time.deltaTime * LoadedWorld.Segments[Mathf.FloorToInt(_playerPos)].Curve, 0);
            background.Translate(-displacement);

            float trackPercentage = CameraTrackPosition / (LoadedWorld.Length * LoadedWorld.SegmentLength);
            Vector3 initialPosition = new Vector3(0f, backgroundStartHeight);
            Vector3 endPosition = new Vector3(0f, backgroundEndHeight);

            background.transform.localPosition = Vector3.Lerp(initialPosition, endPosition, trackPercentage);
        }
        
        /// <summary>
        /// Update the coordinates of the track quads based on the player position.
        /// This projection achieves the pseudo-3D effect.
        /// </summary>
        public void UpdateProjections()
        {
            // Loop track.
            int trackTrip = LoadedWorld.Length * LoadedWorld.SegmentLength;
            while (CameraTrackPosition >= trackTrip) 
                CameraTrackPosition -= trackTrip;
            
            while (CameraTrackPosition < 0)
                CameraTrackPosition += trackTrip;
            
            // These are calculated here to allow experimenting with the screen size values.
            _halfScreenWidth = screenWidthRef / 2f;
            _halfScreenHeight = screenHeightRef / 2f;
            
            // Calculate player and camera positions.
            _playerPos = VehicleTrackPosition / LoadedWorld.SegmentLength;
            _speed = CameraTrackPosition - PreviousCameraTrackPosition;
            _cameraPos = CameraTrackPosition / LoadedWorld.SegmentLength;
            PreviousCameraTrackPosition = CameraTrackPosition;
            
            float calculatedCameraPosition = ((CameraTrackPosition + cameraHeight * cameraDepth) / LoadedWorld.SegmentLength) % LoadedWorld.Segments.Length;
            float camX = CameraHorizontalPosition * LoadedWorld.RoadWidth;
            float camY = CalculateCameraHeight(calculatedCameraPosition);
            float camZ = _cameraPos * LoadedWorld.SegmentLength;
            
            // Keeps tack of the highest rendered quad so far.
            float maxY = -_halfScreenHeight;
            
            // Calculated world position of the segment.
            float segmentX = 0; 
            
            // The accumulated curve displacement.
            float curveDisplacement = 0;

            // Reset current quad on the collection.
            foreach (var q in _quadCollections)
                q.Reset();
            
            // Draw level quads.
            for (int n = Mathf.FloorToInt(_cameraPos) + 1; n < _cameraPos + drawingDistance; n++)
            {
                // Get references to current and previous segments and projections.
                TrackSegment currentSegment = LoadedWorld.Segments[n % LoadedWorld.Length];
                ref ProjectedLine currentProjectedLine = ref projections[n % LoadedWorld.Length];
                ref ProjectedLine previousProjectedLine = ref projections[(n - 1) % LoadedWorld.Length];
                
                /* The camera position is calculated for each segment as a perspective trick
                   to draw curves. Curves are actually segments distorted by the camera perspective
                   on that particular coordinate. */
                float boundaryCorrection = (n >= LoadedWorld.Length ? LoadedWorld.Length * LoadedWorld.SegmentLength : 0);
                Vector3 cameraPosition = new Vector3(camX - segmentX, camY, camZ - boundaryCorrection);
                
                // Recalculate projection coordinates for the current segment.
                UpdateProjectedLine(ref currentProjectedLine, currentSegment.WorldLine, cameraPosition);
                
                // Update segment displacements due to curves.
                segmentX += curveDisplacement;
                curveDisplacement += currentSegment.Curve;

                // Clip (do not draw) non-visible quads and sprites (hidden by a hill).
                currentProjectedLine.clip = maxY;
                if (currentProjectedLine.coord.y <= maxY)
                    continue;

                maxY = currentProjectedLine.coord.y;
                
                AddSegmentQuads(n, currentProjectedLine, previousProjectedLine);

            }
        }

        /// <summary>
        /// Projects the track segment coordinates into screen coordinates and cache the result.
        /// </summary>
        /// <param name="projection">The segment projection to be updated using the world line as source.</param>
        /// <param name="worldLine">The source world line.</param>
        /// <param name="cameraPosition">The world position of the camera.</param>
        private void UpdateProjectedLine(ref ProjectedLine projection, WorldLine worldLine, Vector3 cameraPosition)
        {
            float scale = cameraDepth / (worldLine.Z - cameraPosition.z);
            float x = scale * (worldLine.X - cameraPosition.x) * _halfScreenWidth;
            float y = scale * (worldLine.Y - cameraPosition.y) * _halfScreenHeight;
            float w = scale * worldLine.W * _halfScreenWidth;
            
            projection.scalingFactor = scale;
            projection.coord = new Vector2(x, y);
            projection.w = w;
        }

        /// <summary>
        /// Adds the quads corresponding to a segment.
        /// </summary>
        /// <param name="segmentIndex">The segment position index.</param>
        /// <param name="currentProjectedLine">The calculated projection of the current segment.</param>
        /// <param name="previousProjectedLine">The calculated projection of the previous segment.</param>
        private void AddSegmentQuads(int segmentIndex, ProjectedLine currentProjectedLine, ProjectedLine previousProjectedLine)
        {
            // Pick segment mesh filter.
            MeshFilter groundLeft = (segmentIndex / 3 / 3) % 2 == 0 ? ground1Left : ground2Left;
            MeshFilter groundRight = (segmentIndex / 3 / 3) % 2 == 0 ? ground1Right : ground2Right;

            MeshFilter rumble = (segmentIndex / 3) % 2 == 0 ? rumble1 : rumble2;
            MeshFilter road = (segmentIndex / 3 / 2) % 2 == 0 ? road1 : road2;

            float grassWidthPrevious = Mathf.Max(lodDistance * previousProjectedLine.w, _halfScreenWidth) / 2f;
            float grassWidthCurrent = Mathf.Max(lodDistance * currentProjectedLine.w, _halfScreenWidth) / 2f;

            // Left side grass.
            AddQuad(
                groundLeft, 
                new Vector2(previousProjectedLine.coord.x - grassWidthPrevious, previousProjectedLine.coord.y), 
                new Vector2(currentProjectedLine.coord.x - grassWidthCurrent, currentProjectedLine.coord.y),
                new Vector2(currentProjectedLine.coord.x, currentProjectedLine.coord.y),
                new Vector2(previousProjectedLine.coord.x, previousProjectedLine.coord.y)
            );
            
            // Right side grass.
            AddQuad(
                groundRight, 
                new Vector2(previousProjectedLine.coord.x, previousProjectedLine.coord.y),
                new Vector2(currentProjectedLine.coord.x, currentProjectedLine.coord.y),
                new Vector2(currentProjectedLine.coord.x + grassWidthCurrent, currentProjectedLine.coord.y),
                new Vector2(previousProjectedLine.coord.x + grassWidthPrevious, previousProjectedLine.coord.y)
            );
            
            AddTrapezoidQuad(
                road,
                previousProjectedLine.coord.x, previousProjectedLine.coord.y, 
                previousProjectedLine.w,
                currentProjectedLine.coord.x, currentProjectedLine.coord.y, 
                currentProjectedLine.w,
                drawSegmentLines
            );

            if (drawRumble)
            {
                AddTrapezoidQuad(
                    rumble, 
                    previousProjectedLine.coord.x, previousProjectedLine.coord.y, 
                    previousProjectedLine.w + previousProjectedLine.scalingFactor * rumbleWidth * _halfScreenWidth, 
                    currentProjectedLine.coord.x, currentProjectedLine.coord.y, 
                    currentProjectedLine.w + currentProjectedLine.scalingFactor * rumbleWidth * _halfScreenWidth
                );
            }

            if (drawDashline && ((segmentIndex / 3) % 2 == 0))
            {
                AddTrapezoidQuad(
                    dashline, 
                    previousProjectedLine.coord.x, previousProjectedLine.coord.y * 1.1f,
                    previousProjectedLine.w * 0.05f,
                    currentProjectedLine.coord.x, currentProjectedLine.coord.y * 1.1f, 
                    currentProjectedLine.w * 0.05f
                );
            }
        }

        /// <summary>
        /// Interpolate segment heights to calculate camera y position and have smooth hill
        /// climbing-descent.
        /// </summary>
        /// <param name="playerPos">The track position of the player.</param>
        /// <returns>The y coordinate of the camera.</returns>
        private float CalculateCameraHeight(float playerPos)
        {
            // Interpolate segment heights to calculate camera y position.
            int currentPlayerPosIndex = Mathf.FloorToInt(playerPos);
            int previousPlayerPosIndex = (currentPlayerPosIndex == 0) ? currentPlayerPosIndex : currentPlayerPosIndex - 1;
            float segmentPercentage = playerPos - currentPlayerPosIndex;
            
            TrackSegment currentPlayerSegment = LoadedWorld.Segments[currentPlayerPosIndex % LoadedWorld.Length];
            TrackSegment previousPlayerSegment = LoadedWorld.Segments[previousPlayerPosIndex % LoadedWorld.Length];
            return Mathf.Lerp(previousPlayerSegment.WorldLine.Y, currentPlayerSegment.WorldLine.Y, segmentPercentage) + cameraHeight;
        }

        private void AddQuad(MeshFilter meshFilter, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            // Add quad to rendering dictionary.
            _dictionary[meshFilter].AddQuad(p1 / pixelsPerUnit, p2 / pixelsPerUnit, p3 / pixelsPerUnit, p4 / pixelsPerUnit);
        }

        /// <summary>
        /// Adds a quad to the corresponding quad collection.
        /// </summary>
        /// <param name="meshFilter">The target mesh filter where the quad will be stored..</param>
        /// <param name="x1">Lower horizontal coordinate of the quad.</param>
        /// <param name="y1">Lower vertical coordinate of the quad.</param>
        /// <param name="w1">The lower width.</param>
        /// <param name="x2">Upper horizontal coordinate of the quad.</param>
        /// <param name="y2">Upper vertical coordinate of the quad.</param>
        /// <param name="w2">The upper width.</param>
        /// <param name="drawDebugLine"></param>
        /// <param name="debugLineColor1"></param>
        /// <param name="debugLineColor2"></param>
        private void AddTrapezoidQuad(MeshFilter meshFilter, float x1, float y1, float w1, float x2, float y2, float w2, 
                                      bool drawDebugLine = false, Color? debugLineColor1 = null, Color? debugLineColor2 = null)
        {
            // Calculate scaled quad coordinates.
            Vector2 lowerPos = new Vector2(x1, y1) / pixelsPerUnit;
            Vector2 upperPos = new Vector2(x2, y2) / pixelsPerUnit;
            float lowerWidth = w1 / pixelsPerUnit;
            float upperWidth = w2 / pixelsPerUnit;
            
            // Add quad to rendering dictionary.
            _dictionary[meshFilter].AddTrapezoidQuad(lowerPos, lowerWidth, upperPos, upperWidth);
            
            // Draw debug lines.
            if (drawDebugLine)
            {
                Debug.DrawLine(
                    lowerPos - lowerWidth * Vector2.left,
                    lowerPos + lowerWidth * Vector2.left,
                    debugLineColor1 ?? Color.white
                );
                
                Debug.DrawLine(
                    upperPos - upperWidth * Vector2.left,
                    upperPos + upperWidth * Vector2.left,
                    debugLineColor2 ?? Color.white
                );
            }
        }

        /// <summary>
        /// Update all the meshes with the updated projection coordinates.
        /// </summary>
        private void UpdateMeshes()
        {
            foreach (var meshQuadPair in _dictionary)
            {
                meshQuadPair.Key.sharedMesh?.Clear();
                meshQuadPair.Value.UpdateMesh(meshQuadPair.Key.sharedMesh);
            }
        }
        
        /// <summary>
        /// Draw track sprites projecting them into world coordinates.
        /// </summary>
        private void DrawSprites(SpriteRenderer targetRenderer, RenderTexture targetTexture, int startSegment, int endSegment)
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = targetTexture;
                
            // Work in the pixel matrix of the texture resolution.
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, screenWidthRef, screenHeightRef, 0);
            GL.Clear(false, true, new Color(0, 0, 0, 0));
            
            // Draw sprites from farthest to closest.
            for (int n = endSegment; n > startSegment; n--)
            {
                int segmentIndex = n % LoadedWorld.Length;
                ProjectSegmentSprite(segmentIndex);
                ProjectSegmentVehicles(segmentIndex);
            }

            _materialPropertyBlock.Clear();
            _materialPropertyBlock.SetTexture("_MainTex", targetTexture);
            targetRenderer.SetPropertyBlock(_materialPropertyBlock);

            // Revert the matrix and active render texture.
            GL.PopMatrix();
            RenderTexture.active = currentActiveRT;
        }

        /// <summary>
        /// Draw the world object sprites on different layers depending on their position
        /// relative to the player vehicle.
        /// </summary>
        private void DrawTrackSprites()
        {
            // Draw sprites behind the player vehicle.
            DrawSprites(
                backSpritePlane, 
                _backTargetTexture,
                Mathf.FloorToInt(_cameraPos), 
                Mathf.FloorToInt(_playerPos)
            );
            
            // Draw sprites in front of the player vehicle.
            DrawSprites(
                frontSpritePlane, 
                _frontTargetTexture,
                Mathf.FloorToInt(_playerPos), 
                Mathf.FloorToInt(_cameraPos) + drawingDistance
            );
        }

        /// <summary>
        /// Project the sprite to screen coordinates.
        /// </summary>
        /// <param name="projectedLine"></param>
        /// <param name="segmentSprite"></param>
        /// <param name="spritePosition"></param>
        /// <param name="spriteScale"></param>
        /// <returns></returns>
        private Rect ProjectToScreen(ProjectedLine projectedLine, Sprite segmentSprite, Vector2 spritePosition, 
                                     float spriteScale)
        {
            float w = segmentSprite.rect.width;
            float h = segmentSprite.rect.height;

            float destX = projectedLine.coord.x + projectedLine.w * spritePosition.x + _halfScreenWidth;
            float destY = -projectedLine.coord.y - projectedLine.w * spritePosition.y + _halfScreenHeight;
            float destW = w * projectedLine.scalingFactor * _halfScreenWidth * spriteScale;
            float destH = h * projectedLine.scalingFactor * _halfScreenWidth * spriteScale;

            destX -= destW / 2f;
            destY -= destH;
            
            return new Rect(destX, destY, destW, destH);
        }

        /// <summary>
        /// Project sprite to unity world coordinates.
        /// </summary>
        /// <param name="projectedLine"></param>
        /// <param name="segmentSprite"></param>
        /// <param name="spritePosition"></param>
        /// <param name="spriteScale"></param>
        /// <returns></returns>
        private Rect ProjectToUnityWorld(ProjectedLine projectedLine, Sprite segmentSprite, Vector2 spritePosition, 
                                         float spriteScale)
        {
            float w = segmentSprite.rect.width;
            float h = segmentSprite.rect.height;

            float destX = projectedLine.coord.x + projectedLine.w * spritePosition.x;
            float destY = projectedLine.coord.y + projectedLine.w * spritePosition.y;
            float destW = w * projectedLine.scalingFactor * _halfScreenWidth * spriteScale;
            float destH = h * projectedLine.scalingFactor * _halfScreenWidth * spriteScale;
            destX -= destW / 2f;
            
            return new Rect(destX / pixelsPerUnit, destY / pixelsPerUnit, destW / pixelsPerUnit, destH / pixelsPerUnit);
        }

        /// <summary>
        /// Calculate the projected line on a specific track position.
        /// </summary>
        /// <param name="position">The track position in track coordinates.</param>
        /// <returns>The corresponding projected line.</returns>
        private ProjectedLine GetProjectedLine(float position)
        {
            // Find segments corresponding to position.
            float segmentIndexFrac = position / LoadedWorld.SegmentLength;
            int segmentIndex = Mathf.CeilToInt(segmentIndexFrac) % LoadedWorld.Length;
            int currentSegmentPosition = segmentIndex * LoadedWorld.SegmentLength;

            int prevSegmentIndex = Mathf.FloorToInt(segmentIndexFrac) % LoadedWorld.Length;
            if (prevSegmentIndex < 0)
                prevSegmentIndex = segmentIndex;
            
            int prevSegmentPosition = prevSegmentIndex * LoadedWorld.SegmentLength;

            // Get projected lines of corresponding segments.
            ProjectedLine projectedLine = projections[segmentIndex];
            ProjectedLine previousProjectedLine = projections[prevSegmentIndex];

            // Handle special case where both segments are the same one.
            if (segmentIndex == prevSegmentIndex)
                return previousProjectedLine;
            
            // Interpolate between both projected lines to calculate the resulting projected line.
            float segmentPercentage = (position - prevSegmentPosition)/(currentSegmentPosition - prevSegmentPosition);
            return ProjectedLine.Lerp(previousProjectedLine, projectedLine, segmentPercentage);
        }
        
        /// <summary>
        /// Draw the sprite corresponding to a track segment by using the already calculated
        /// segment projection.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        private void ProjectSegmentSprite(int segmentIndex)
        {
            ref TrackSegment segment = ref LoadedWorld.Segments[segmentIndex];

            // Skip if there are no objects in the segment.
            if (segment.segmentObjects == null)
                return;
            
            ProjectedLine projectedLine = projections[segmentIndex];
            foreach (StaticWorldObject worldObject in segment.segmentObjects)
            {
                if (!worldObject.Enabled || projectedLine.coord.y < -_halfScreenHeight)
                    return;

                // Update collider coordinates.
                worldObject.ColliderBuffer[Layer] = GetCollider(
                    worldObject.CurrentSprite,
                    itemSpriteScale * worldObject.CustomSpriteScale,
                    segment.WorldLine.Z,
                    new Vector2(worldObject.X + worldObject.Pseudo3DCollider.ColliderOffset.x, worldObject.Pseudo3DCollider.ColliderOffset.y),
                    worldObject.Pseudo3DCollider
                    );

                // Projected sprite rect.
                Rect projectionCoordinates = ProjectToScreen(
                    projectedLine,
                    worldObject.CurrentSprite,
                    new Vector2(worldObject.X, 0f),
                    itemSpriteScale * worldObject.CustomSpriteScale
                    );

                // Calculate clipping rect.
                float clipDelta = projectedLine.clip - projectedLine.coord.y;
                if (clipDelta < 0) 
                    clipDelta = 0;
                
                if (clipDelta >= projectionCoordinates.height) 
                    continue;

                // Draw projected sprite into texture.
                Rect source = new Rect(Vector2Int.zero, new Vector2(1f, 1f - clipDelta / projectionCoordinates.height));
                DrawTexture(source, worldObject.CurrentSprite, projectionCoordinates, worldObject.FlipX);

                // Draw debug lines.
                if (drawColliders)
                {
                    DrawItemColliderBox(
                        worldObject.CurrentSprite,
                        itemSpriteScale * worldObject.CustomSpriteScale,
                        segment.WorldLine.Z,
                        new Vector2(worldObject.X + worldObject.Pseudo3DCollider.ColliderOffset.x, worldObject.Pseudo3DCollider.ColliderOffset.y),
                        worldObject.Pseudo3DCollider
                    );
                }
            }
        }

        /// <summary>
        /// Calculate the projected collider in world coordinates.
        /// </summary>
        /// <param name="sprite">Sprite used to get the width of the collider.</param>
        /// <param name="spriteScale">Sprite size scaling parameter.</param>
        /// <param name="trackPosition">The current track position of the object.</param>
        /// <param name="spritePosition">The current position of the object sprite.</param>
        /// <param name="pseudo3DCollider">The collider properties.</param>
        /// <returns></returns>
        private Rect GetCollider(Sprite sprite, float spriteScale, float trackPosition, Vector2 spritePosition, 
                                 Pseudo3DCollider pseudo3DCollider)
        {
            // We extrapolate the projected lines to the front and back.
            ProjectedLine frontColliderLine = GetProjectedLine(trackPosition + pseudo3DCollider.ColliderLength);
            ProjectedLine backColliderLine = GetProjectedLine(trackPosition - pseudo3DCollider.ColliderLength);
            
            // And finally we build the collider rect using the extrapolated projection coordinates and the sprite size.
            Vector2 colliderPosition = spritePosition + pseudo3DCollider.ColliderOffset;
            Rect frontColliderProjectedRect = ProjectToUnityWorld(frontColliderLine, sprite, colliderPosition, spriteScale);
            Rect backColliderProjectedRect = ProjectToUnityWorld(backColliderLine, sprite, colliderPosition, spriteScale);

            frontColliderProjectedRect.Rescale(pseudo3DCollider.ColliderScale);
            backColliderProjectedRect.Rescale(pseudo3DCollider.ColliderScale);
            
            Rect colliderRect = Rect.MinMaxRect(
                backColliderProjectedRect.min.x, backColliderProjectedRect.min.y,
                frontColliderProjectedRect.max.x, frontColliderProjectedRect.min.y
                );
            
            if (drawColliders)
                DebugUtils.DrawRect(colliderRect, Color.yellow);
            
            return colliderRect;
        }

        /// <summary>
        /// Draw the vehicles on the specified segment.
        /// </summary>
        /// <param name="segmentIndex">The segment index.</param>
        private void ProjectSegmentVehicles(int segmentIndex)
        {
            // Get current and previous segment projections.
            int segmentPosition = segmentIndex * LoadedWorld.SegmentLength;
            int prevSegmentPosition = (segmentIndex - 1) * LoadedWorld.SegmentLength;
            int previousSegmentIndex = (segmentIndex == 0) ? segmentIndex : segmentIndex - 1;
            ProjectedLine projectedLine = projections[segmentIndex];
            ProjectedLine previousProjectedLine = projections[previousSegmentIndex];
            
            foreach (IVehicle vehicle in LoadedWorld.Vehicles)
            {
                // Is the vehicle inside the segment? If not, skip.
                if (!(vehicle.VehicleState.trackPosition < segmentPosition) ||
                    !(vehicle.VehicleState.trackPosition >= prevSegmentPosition)) 
                    continue;
                
                // Interpolate segment properties.
                float segmentPercentage = (vehicle.VehicleState.trackPosition - prevSegmentPosition)/(segmentPosition - prevSegmentPosition);
                ProjectedLine interpolatedProjection = ProjectedLine.Lerp(previousProjectedLine, projectedLine, segmentPercentage);
                
                // Update collider coordinates.
                vehicle.ColliderBuffer[Layer] = GetCollider(
                    vehicle.CurrentSprite,
                    vehicleSpriteScale * vehicle.CustomSpriteScale,
                    vehicle.VehicleState.trackPosition,
                    vehicle.VehiclePosition,
                    vehicle.Pseudo3DCollider
                );

                if (vehicle is PlayerVehicle && ReferenceEquals(vehicle, playerVehicle))
                {
                    // Project line to unity world coordinates.
                    Rect worldProj = ProjectToUnityWorld(
                        interpolatedProjection,
                        playerVehicle.VehicleSprite,
                        vehicle.SpritePosition,
                        vehicleSpriteScale * vehicle.CustomSpriteScale
                    );

                    // Move the player vehicle sprite.
                    Vector3 spriteSize = playerVehicle.VehicleSprite.bounds.size;
                    Vector3 scaleFactor = new Vector3(
                        worldProj.width / spriteSize.x,
                        worldProj.height / spriteSize.y,
                        1.0f
                    );
                    Vector3 worldPosition = new Vector3(worldProj.x + worldProj.width * 0.5f,
                        worldProj.y + worldProj.height * 0.5f, 0f);

                    Transform rendererTransform = playerVehicle.SpriteRenderer.transform;
                    rendererTransform.localPosition = worldPosition;
                    rendererTransform.localScale = scaleFactor;
                }
                else
                {
                    // Calculate projection and clip.
                    Rect projectionCoordinates = ProjectToScreen(
                        interpolatedProjection,
                        vehicle.CurrentSprite,
                        vehicle.SpritePosition,
                        vehicleSpriteScale * vehicle.CustomSpriteScale
                    );

                    float clipDelta = interpolatedProjection.clip - interpolatedProjection.coord.y;
                    if (clipDelta < 0)
                        clipDelta = 0;

                    if (clipDelta >= projectionCoordinates.height)
                        continue;

                    // Draw projected sprite into texture.
                    Rect source = new Rect(Vector2Int.zero, new Vector2(1f, 1f - clipDelta / projectionCoordinates.height));
                    DrawTextures(source, vehicle.CurrentSprites, projectionCoordinates);
                }

                // Draw debug lines.
                if (drawColliders)
                {
                    DrawItemColliderBox(
                        vehicle.CurrentSprite,
                        vehicleSpriteScale * vehicle.CustomSpriteScale,
                        vehicle.VehicleState.trackPosition,
                        vehicle.VehiclePosition,
                        vehicle.Pseudo3DCollider
                    );
                }
            }
        }

        /// <summary>
        /// Draw the collider box of an object such as a car or road item.
        /// </summary>
        /// <param name="sprite">The sprite of the world object.</param>
        /// <param name="spriteScale">The sprite scale.</param>
        /// <param name="trackPosition">Current camera track position.</param>
        /// <param name="spritePosition">Current camera horizontal position.</param>
        /// <param name="pseudo3DCollider"></param>
        private void DrawItemColliderBox(Sprite sprite, float spriteScale, float trackPosition, Vector2 spritePosition, 
                                         Pseudo3DCollider pseudo3DCollider)
        {
            // We extrapolate the projected lines to the front and back.
            ProjectedLine frontColliderLine = GetProjectedLine(trackPosition + pseudo3DCollider.ColliderLength);
            ProjectedLine backColliderLine = GetProjectedLine(trackPosition - pseudo3DCollider.ColliderLength);
            
            // And finally we build the collider rect using the extrapolated projection coordinates and the sprite size.
            Vector2 colliderPosition = spritePosition + pseudo3DCollider.ColliderOffset;
            Rect frontColliderProjectedRect = ProjectToUnityWorld(frontColliderLine, sprite, colliderPosition, spriteScale);
            Rect backColliderProjectedRect = ProjectToUnityWorld(backColliderLine, sprite, colliderPosition, spriteScale);

            frontColliderProjectedRect.Rescale(pseudo3DCollider.ColliderScale);
            backColliderProjectedRect.Rescale(pseudo3DCollider.ColliderScale);
            
            DebugUtils.DrawCube(backColliderProjectedRect, frontColliderProjectedRect, Color.cyan);
        }
        
        /// <summary>
        /// Draw a sprite into a texture.
        /// </summary>
        /// <param name="offsetSource"></param>
        /// <param name="sprite">The sprite to render.</param>
        /// <param name="target"></param>
        /// <param name="flipHorizontally"></param>
        private void DrawTexture(Rect offsetSource, Sprite sprite, Rect target, bool flipHorizontally = false)
        {
            int sign = flipHorizontally ? -1 : 1;
            int intFlip = flipHorizontally ? 1 : 0;

            Rect screenRect = new Rect(target.x + intFlip * target.width, target.y, target.width * sign, target.height * offsetSource.height);
            Rect sourceRect = new Rect(
                sprite.rect.x / sprite.texture.width,
                sprite.rect.y / sprite.texture.height + (1f - offsetSource.height) * sprite.rect.height / sprite.texture.height,
                sprite.rect.width / sprite.texture.width,
                sprite.rect.height / sprite.texture.height * offsetSource.height
            );
            
            Graphics.DrawTexture(screenRect, sprite.texture, sourceRect, 0, 0, 0, 0);
        }

        /// <summary>
        /// Draw a layered sprite list into a texture.
        /// </summary>
        /// <param name="offsetSource"></param>
        /// <param name="sprites">The sprite list to render.</param>
        /// <param name="target"></param>
        /// <param name="flipHorizontally"></param>
        private void DrawTextures(Rect offsetSource, List<Sprite> sprites, Rect target, bool flipHorizontally = false)
        {
            foreach (Sprite s in sprites)
                DrawTexture(offsetSource, s, target, flipHorizontally);
        }
        
        #endregion
    }
}