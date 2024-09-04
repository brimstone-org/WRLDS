using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Gamekit2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace _DPS
{
    public class LevelBuilder : Singleton<LevelBuilder>
    {
        private LevelBuilder _instance;
        protected LevelBuilder() { }

        [Header("Camera refs")] 
        private BoxCollider _cameraConfiner;
        public GameObject CameraPrefab;
        private CinemachineVirtualCamera _camSetup;

        [Header("SpriteShape Section")]
        public Transform SpriteShapeHolder;
        public SpriteShapeController SpriteShapeController;
        public Transform RepeatingBackgroundHolder;
        public Vector3 LevelRotation;
        [HideInInspector]
        public SceneTypes SceneType;

        public Transform ProjectileHolder { get; set; }
        public Transform FallingObjHolder { get;set; }

        [Header("SceneParams")] 
        [SerializeField]
        private GameObject _bossPrefab;
        [SerializeField]
        private float _generalNodeOffset;
        public List<SceneParams> SceneParameters;


        [HideInInspector]
        public List<int> MainSpriteSides;
        [HideInInspector]
        public GameObject SceneExitPrefab;
        [HideInInspector]
        public float SceneExitNodeOffsetY;
        //[HideInInspector]
        public GameObject RepeatingBackgroundPrefab;
        [HideInInspector]
        public GameObject RepeatingBackground;
        //[HideInInspector]
        //public int SplinePointsToInsert = 20;
        [HideInInspector]
        public float ReferencePointMinDistance = 3f;
        [HideInInspector]
        public float RepeatingBackgroundWidth;
        [HideInInspector]
        public float TangentMultiplier = 1.2f;
        [HideInInspector]
        public float ReferencePointMaxDistance = 3f;
        [HideInInspector]
        public float ReferencePointHeight = .3f;
        [HideInInspector]
        public float ReferencePointHeightMultiplier = .3f;
        [HideInInspector]
        public float GroundOffsetX = 50;
        [HideInInspector]
        public float GroundOffsetY = 25;


        [Header("Info Posts")]
        public GameObject PostPrefab;

        [Header("NodeObjects")]
        public Transform NodeObjHolder;
        public List<NodeObject> NodeObjects = new List<NodeObject>();
        /// <summary>
        /// int is node id from SplineNode index
        /// </summary>
        public List<int> OccupiedNodes = new List<int>();
        public List<NodeAttach> OccupiedNodesAttaches = new List<NodeAttach>();
        /// <summary>
        /// first int is the node id the second it's the count
        /// </summary>
        public Dictionary<int, int> SpawnedCount = new Dictionary<int, int>();
        public static GameObject Player;


        //helpers
        private static bool firstPlay = true;
        private static float bgXoffset;
        private int lastMileStone;
        private int NoOfPooledProjectiles = 5; //the number of pooled projectiles per canon
        public static int CurrentSceneId;
        private Camera _mainCamera;
        private Vector3 backgroundOffset;
        private List<GameObject> _backgrounds;
        public CinemachineVirtualCamera CamSetup { get => _camSetup; set => _camSetup = value; }
        public CinemachineVirtualCamera LastSetup;
        [NonSerialized]
        public GameObject boss;
        [NonSerialized]
        public GameObject Exit;

        private int _levelExitIndex, _encounterIndex;
        [HideInInspector]
        public float SpriteShapeLenght;

        [Flags]
        public enum NodeObjectType
        {
            NONE = 0,
            Special = 1 << 0,
            DoubleJump = 1 << 1,
            Teleport = 1 << 2,
            TriggerPlatform = 1 << 3,
            WaterVine = 1 << 4,
            Collectible = 1 << 5,
            EndOfScene = 1 << 6,
            SingleKillerPlatform = 1 << 7,
            Encounter = 1 << 8,

            NO_TOP = Special | Collectible | EndOfScene | SingleKillerPlatform | Encounter
        }

        [Serializable]
        public class EnvironmentEffect
        {
            public GameObject EffectPrefab;
            public GameObject EffectParent;
        }

        [Serializable]
        public class SceneParams
        {
            public string SceneName;
            public SceneTypes SceneType = SceneTypes.NONE;
            public float GeneralNodeOffsetY = 0;
            public SpriteShapeController SpritePrefab;
            public GameObject SceneExitPrefab;
            public float SceneExitNodeOffsetY;
            public GameObject SkyPrefab;
            public GameObject RepeatingBackgroundPrefab;
            public Vector3 BackgroundOffset;
            public Vector3 LevelRotation;
            
            
            [Header("Encounter Refs")] 
            public GameObject ExitEncounterPrefab;

            [Header("Pool Holders")] 
            public bool HasProjectiles;
            public bool HasFallingObjects;
            public List<EnvironmentEffect> EnvironmentEffects;
            public List<int> MainSpriteSides = new List<int>();
            //public int SplinePointsToInsert = 20;
            public float ReferencePointMinDistance = 3f;
            public float ReferencePointMaxDistance = 3f;
            public float TangentMultiplier = 1.2f;
            public float ReferencePointHeight = .3f;
            public float ReferencePointHeightMultiplier = .3f;
            public float GroundOffsetX = 50;
            public float GroundOffsetY = 25;
            public List<NodeObject> NodeObjects = new List<NodeObject>();
        }

        [Serializable]
        public class SideParams
        {
            [Header("Number of Points to insert per side")]
            public int SpriteSidePoints;
            [Header("Is the side active?")]
            public bool IsActiveSide;
        }

  

        [Serializable]
        public class NodeObject
        {
            public NodeObjectType NodeType;
            public NodeAttach NodeObjectRef;
            public int MaxOfThisKind = 1000;
            public float NodeProbability = 25;
            public float Scale = 1f;
            public float OffsetY;
            public float OffsetX;
        }

        // Start is called before the first frame update
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            //if (_instance == null)
            //{
            //    _instance = this;
            //}
            //        else if (_instance != this)
            //        {
            //            Destroy(_instance.gameObject);
            //        }
            DontDestroyOnLoad(LevelBuilder.Instance);
            //bgXoffset = BG1.transform.localPosition.x;

        }

        [Flags]
        public enum SceneTypes
        {
            NONE = 0,

            Climber = 1 << 0,
            FlappyBird = 1 << 1,
            UpTube = 1 << 2,
            DownTube = 1 << 3,
            Ascend = 1 << 4,
            Descend = 1 << 5,
            Asteroids = 1 << 6,

            ALL_VERTICAL = UpTube | DownTube | Ascend | Descend

            
        }

        public void Init()
        {
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
        
            Time.timeScale = 1;
            global::Logger.Log("~~~~~~~~~~ init " + CameraPrefab.name);
            var cam = Instantiate(CameraPrefab);
            yield return null;
            _cameraConfiner = cam.GetComponentInChildren<CameraConfiner>().GetComponent<BoxCollider>();
            CamSetup = cam.GetComponentInChildren<CinemachineVirtualCamera>();
            _mainCamera = Camera.main;
            SpriteShapeHolder = RepeatingBackgroundHolder = GameObject.Find("Platforms").transform;
            NodeObjects= new List<NodeObject>();
            OccupiedNodes = new List<int>();
            OccupiedNodesAttaches = new List<NodeAttach>();
            SpawnedCount = new Dictionary<int, int>();
            SetSceneParams();
            yield return null;
            StartCoroutine(BuildTerrain());
        }

        private void SetSceneParams()
        {
            foreach (var sceneparams in SceneParameters)
            {
                if (sceneparams.SceneName != GameManager.Instance.GetSceneName()) continue;



                CurrentSceneId = SceneParameters.IndexOf(sceneparams);
                MainSpriteSides = sceneparams.MainSpriteSides;
                SceneExitPrefab = sceneparams.SceneExitPrefab;
                SceneExitNodeOffsetY = sceneparams.SceneExitNodeOffsetY;
                SceneType = sceneparams.SceneType;
                LevelRotation = sceneparams.LevelRotation;
                RepeatingBackgroundPrefab = sceneparams.RepeatingBackgroundPrefab;
                backgroundOffset = sceneparams.BackgroundOffset;
                SpriteShapeController = Instantiate(sceneparams.SpritePrefab, SpriteShapeHolder);
                NodeObjHolder = SpriteShapeController.transform;
                ReferencePointMinDistance = sceneparams.ReferencePointMinDistance;
                ReferencePointMaxDistance = sceneparams.ReferencePointMaxDistance;
                TangentMultiplier = sceneparams.TangentMultiplier;
                ReferencePointHeight = sceneparams.ReferencePointHeight;
                ReferencePointHeightMultiplier = sceneparams.ReferencePointHeightMultiplier;
                GroundOffsetX = sceneparams.GroundOffsetX;
                GroundOffsetY = sceneparams.GroundOffsetY;
                _generalNodeOffset = sceneparams.GeneralNodeOffsetY;
                NodeObjects = sceneparams.NodeObjects;
                //pool holders
                if (sceneparams.HasProjectiles)
                {
                    ProjectileHolder = new GameObject("ProjectileHolder").transform;
                    ProjectileHolder.transform.position = new Vector3(-1000,-1000,0);
                }
                else
                {
                    ProjectileHolder = null;
                }
                if (sceneparams.HasFallingObjects)
                {
                    FallingObjHolder = new GameObject("FallingObjHolder").transform;
                    FallingObjHolder.transform.position = new Vector3(-1000,-1000,0);
                }
                else
                {
                    FallingObjHolder = null;
                }
                ShootingSystem.Instance.ProjectilePoolHolder = ProjectileHolder;
                ShootingSystem.Instance.FallingPoolHolder = FallingObjHolder;
                
                //add effects
                foreach (var effect in sceneparams.EnvironmentEffects)
                {
                    var e = Instantiate(effect.EffectPrefab);
                    if (effect.EffectParent == null)
                    {
                        effect.EffectParent = CamSetup.gameObject;
                    }
                    e.transform.SetParent(effect.EffectParent.transform);
                }

                if (sceneparams.SkyPrefab != null)
                {
                    Instantiate(sceneparams.SkyPrefab, _mainCamera.gameObject.transform);
                }
            }

        }

        private IEnumerator BuildTerrain()
        {
            //set tag to sprite
            SpriteShapeController.gameObject.tag = "Land";
            //get second point position
            var posRef = SpriteShapeController.spline.GetPosition(0);
            var initialPosRef = posRef;
            posRef += new Vector3(GroundOffsetX, 0, 0);
            //build vectors
            var start = 0;
            // iterate trough sides
            for (int j = 0; j < MainSpriteSides.Count; j++)
            {

                var horizontal = posRef;
                var vertical = ReferencePointHeight;
                //second side is 0
                //if (j==2 && MainSpriteSides[j] >= 0)
                //{
                //    vertical = ReferencePointHeight - GroundOffsetY;
                //    horizontal = -posRef;
                //}

                //iterate trough points per side

                if (MainSpriteSides[j] > 0 && j == 0)
                {
                    start = 1;
                }

                if (j > 0)
                {
                    start += MainSpriteSides[j - 1];
                }

                var max = MainSpriteSides[j] + start;
                global::Logger.Log("The max number of nodes in BuildTerrain is " + max);
                for (int i = start; i < max; i++)
                {

                    var vHeight = UnityEngine.Random.Range(vertical * -1 * ReferencePointHeightMultiplier, vertical * ReferencePointHeightMultiplier);
                    var vectorX = SpriteShapeController.spline.GetPosition(i - 1).x + UnityEngine.Random.Range(ReferencePointMinDistance, ReferencePointMaxDistance) +
                                  horizontal.x;
                    var vector = new Vector3(vectorX, vHeight, 0);

                    if (j > 0 /*&& j< MainSpriteSides.Count -1*/ && i == start)
                    {
                        vHeight = vHeight + GroundOffsetY;
                        vector = new Vector3(SpriteShapeController.spline.GetPosition(start - 1).x, vHeight, 0);
                        //vertical = SpriteShapeController.spline.GetPosition(start - 1).y + GroundOffsetY;
                    }

                    //set points for second bracket and up
                    if (j > 0 && i > start)
                    {
                        vectorX = SpriteShapeController.spline.GetPosition(i - 1).x - UnityEngine.Random.Range(ReferencePointMinDistance, ReferencePointMaxDistance) -
                                  horizontal.x;
                        vHeight = vHeight + GroundOffsetY;
                        vector = new Vector3(vectorX, vHeight, 0);
                    }
                    //for the first side start at 2 and add will add infopost at 1 and player near 1
                    if (j == 0)
                    {
                        //topside starts at 2
                        if (i >= 2)
                        {
                            SpriteShapeController.spline.InsertPointAt(i, vector);
                        }
                    }
                    else
                    {
                        //rest start when they do
                        SpriteShapeController.spline.InsertPointAt(i, vector);

                    }

                    //set positions
                    if (i > start && i < max)
                    {
                        SpriteShapeController.spline.SetPosition(i, vector);
                        var point = SpriteShapeController.spline.GetPosition(i);
                        //invert tangents for downside
                        var leftTangent = j == 0 ? new Vector3(-ReferencePointMinDistance / TangentMultiplier, 0, 0) : new Vector3(ReferencePointMinDistance / TangentMultiplier, 0, 0);
                        var rightTangent = j == 0 ? new Vector3(ReferencePointMinDistance / TangentMultiplier, 0, 0) : new Vector3(-ReferencePointMinDistance / TangentMultiplier, 0, 0);
                        SpriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                        SpriteShapeController.spline.SetLeftTangent(i, leftTangent);
                        SpriteShapeController.spline.SetRightTangent(i, rightTangent);
                    }

                    //make 1 inline with 0 always
                    if (i == 1)
                    {
                        SpriteShapeController.spline.SetPosition(i,
                            new Vector3(initialPosRef.x + ReferencePointMinDistance, initialPosRef.y,
                                initialPosRef.z));
                        SpriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                    }


                    if (i == max - 1)
                    {
                        //if last spot make in line with 0
                        if (j == MainSpriteSides.Count - 2)
                        {
                            global::Logger.Log("grr");
                            SpriteShapeController.spline.SetPosition(i, new Vector3(initialPosRef.x, GroundOffsetY, initialPosRef.z));
                        }
                        // have ends to be linear
                        SpriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Linear);
                    }


                    //spawn encounter
                    if (j == 0 && i == max - 3)
                    {
                       _encounterIndex = i;
                    }
                    //spawn exit
                    if (j == 0 && i == max - 2)
                    {
                        _levelExitIndex = i;
                    }
                }
            }

            SpriteShapeController.BakeCollider();
            yield return null;
            //jobify
            var b = SpriteShapeController.BakeMesh();

            b.Complete();
            yield return b.IsCompleted;

            yield return StartCoroutine(PingSpriteShape(SpriteShapeController));
            yield return null;
            yield return StartCoroutine(SpawnPlayer());

            yield return StartCoroutine(SpawnOnNodes());
            BuildProgressBar();
            yield return new WaitForSecondsRealtime(1f);
            EnableLateStuff();
            SetCameraConfinerCollider();
            SpawnBgs();
            SpawnWelcome();
            if (LevelRotation.z != 0)
            {
                RotateLevel();
            }

            if (!SceneType.Contains(SceneTypes.Climber | SceneTypes.FlappyBird))
            {
                ApplyCorrections();
            }

            EnableVeryLateStuff();


        }


        /// <summary>
        /// creates the pool of projectiles
        /// </summary>
        private void CreatePoolOfProjectiles(ProjectileShooter _projShooter)
        {
            if (_projShooter.firingPoint == null)
            {
                _projShooter.firingPoint = _projShooter.transform.GetChild(0); //get the firing point
                _projShooter.PooledProjectiles = new GameObject[NoOfPooledProjectiles]; //initialize the pooled projectiles array
            }
            for (int i = 0; i < NoOfPooledProjectiles; i++)
            {
                GameObject proj = Instantiate(_projShooter.Projectile, _projShooter.firingPoint.position, _projShooter.transform.rotation); //instantiate projectile
                proj.transform.SetParent(_projShooter.firingPoint); //set the projectile parent to be the firing point. easier for when the bullet position is reset
                proj.GetComponent<SpriteRenderer>().sprite = _projShooter.ArrayOfSprites[UnityEngine.Random.Range(0, _projShooter.ArrayOfSprites.Length)]; //choose a random sprite
                proj.SetActive(false); //deactivate the projectile
                _projShooter.PooledProjectiles[i] = proj; //added it to the array of pooled projectiles
            }
        }

        private void BuildProgressBar()
        {

            //get spriteshape length
            SpriteShapeLenght = SpriteShapeController.spline.GetPosition(MainSpriteSides[0]).x - SpriteShapeController.spline.GetPosition(0).x;
            for (int i = 0; i < MainSpriteSides[0]; i++)
            {
                if(!OccupiedNodes.Contains(i)) continue;
                var progBubble = Instantiate(HudManager.Instance.ProgressBubblePrefab, HudManager.Instance.ProgressBackground.transform);
                
                var progressLength = HudManager.Instance.ProgressBackground.rectTransform.sizeDelta.x;
                var spriteRefPos = progressLength * (SpriteShapeController.spline.GetPosition(i).magnitude / SpriteShapeLenght);
                spriteRefPos = Mathf.Clamp(spriteRefPos, 0, progressLength);
                progBubble.transform.localPosition = new Vector3(spriteRefPos, progBubble.transform.localPosition.y, progBubble.transform.localPosition.z);

            }
            BallController.Instance.childColor = new ToggleProgressColor[HudManager.Instance.ProgressBackground.transform.childCount];
            BallController.Instance.childColor = HudManager.Instance.ProgressBackground.transform.GetComponentsInChildren<ToggleProgressColor>();
        }
        public void GetProgressPoint(float playerPosition)
        {
            // player position on x in reference to the length of the terrain starting from 0 position
            var pos = Mathf.Clamp(playerPosition, 0, playerPosition / (LevelBuilder.Instance.SpriteShapeController.spline.GetPosition(0).x +LevelBuilder.Instance.SpriteShapeLenght));
            HudManager.Instance.ProgressBackground.fillAmount = pos;
        }

        private void RotateLevel()
        {
            SpriteShapeHolder.position += new Vector3((Player.transform.position.x + GroundOffsetY  /2 -GroundOffsetX) , SpriteShapeHolder.position.y, SpriteShapeHolder.position.z);
            SpriteShapeHolder.transform.rotation = _cameraConfiner.transform.rotation = Quaternion.Euler(LevelRotation);
        }

        private void ApplyCorrections()
        {
            var speedUpMp = 0f;
            //fix collectibles
            for (int i = 0; i < OccupiedNodesAttaches.Count; i++)
            {
                var nbj = OccupiedNodesAttaches[i].GetComponent<NodeObjectComponent>();
                if (nbj != null && nbj.NodeType.Contains(NodeObjectType.Collectible | NodeObjectType.Special | NodeObjectType.SingleKillerPlatform))
                {
                    if (!SceneType.Contains(SceneTypes.DownTube))
                    {
                        nbj.gameObject.transform.localRotation = Quaternion.Euler(0, 0, -90); //Quaternion.Euler(nbj.transform.rotation.x, nbj.transform.rotation.y, nbj.transform.rotation.z -90f);
                    }
                    else
                    {
                        nbj.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 90);//Quaternion.Euler(nbj.transform.rotation.x, nbj.transform.rotation.y, nbj.transform.rotation.z -90f);
                    }
                    nbj.transform.localPosition = new Vector3(nbj.transform.localPosition.x, 10.7f, nbj.transform.localPosition.z);
                    if (nbj.GetComponent<ProjectileShooter>())
                    {
                        nbj.GetComponent<ProjectileShooter>().InitializeWaterCanons();
                    }
                    var mp = nbj.GetComponentInChildren<KillerPiston>();
                    if (mp != null)
                    {
                        mp.MovingSpeed += speedUpMp;
                        speedUpMp+= .33f;
                    }
                }
            }

            Player.transform.position = Vector3.zero + new Vector3(0,2,0);
            if (SceneType == SceneTypes.DownTube)
            {
                Player.transform.position = Vector3.zero + new Vector3(21, -30, 0);
            }
        }

        private void SpawnBgs()
        {
        
            if (RepeatingBackgroundPrefab != null)
            {
                _backgrounds = new List<GameObject>();
                RepeatingBackground = Instantiate(RepeatingBackgroundPrefab, Player.transform.position + backgroundOffset, Quaternion.identity);
                _backgrounds.Add(RepeatingBackground);
                RepeatingBackground.transform.SetParent(RepeatingBackgroundHolder, false);
                RepeatingBackgroundWidth = RepeatingBackground.transform.GetComponent<Collider2D>().bounds.size.x;

                var sort = -500;
                var sortlayer = RepeatingBackground.GetComponent<SortingGroup>();
                if (sortlayer == null)
                {
                    sortlayer = RepeatingBackground.AddComponent<SortingGroup>();
                }

                sortlayer.sortingOrder = sort + 1;

                global::Logger.Log("build bg " + RepeatingBackgroundWidth);
                var times = Mathf.CeilToInt(_cameraConfiner.size.x / RepeatingBackgroundWidth);
                for (int i = 0; i < times; i++)
                {

                    var b = Instantiate(RepeatingBackground, RepeatingBackgroundHolder);
                    _backgrounds.Add(b);
                    var loc = b.transform.localPosition;
                    loc.x += RepeatingBackgroundWidth + RepeatingBackgroundWidth * i;
                    b.transform.localPosition = loc;
                    sort = -500;
                    sortlayer = b.GetComponent<SortingGroup>();
                    if (sortlayer == null)
                    {
                        sortlayer = b.AddComponent<SortingGroup>();
                    }

                    sortlayer.sortingOrder = sort + i+2;
                }
            }
        }

        /// <summary>
        /// stabilize sprite
        /// </summary>
        /// <param name="spc"></param>
        /// <returns></returns>
        private IEnumerator PingSpriteShape(SpriteShapeController spc)
        {
            spc.gameObject.SetActive(false);
            yield return null;
            spc.gameObject.SetActive(true);
            spc.gameObject.isStatic = true;
            yield return null;
        }

        private IEnumerator SpawnOnNodes()
        {
            //sort nodeobjects by enum priority
            // specials, doublejump, collectibles
            NodeObjects = NodeObjects.OrderBy(o => o.NodeType).ToList();
            var start = 2;
            for (int k = 0; k < MainSpriteSides.Count; k++)
            {   
                if (k > 0)
                {
                    start += MainSpriteSides[k - 1];
                }
                var max = MainSpriteSides[k] + start;
                global::Logger.Log("The max number of nodes in SpawnNodes is " + max);
                for (int i = start + 2; i < max - 4; i++)
                {
                    //if(i == _levelExitIndex) continue;
                    var addedCollectible = false;
                    for (int j = 0; j < NodeObjects.Count; j++)
                    {

                        if((k > 0 && NodeObjectType.NO_TOP.Contains(NodeObjects[j].NodeType)) || NodeObjects[j].NodeType.Contains(NodeObjectType.Encounter)) continue;
                        
                        var rng = UnityEngine.Random.Range(0, 101);
                        if (!OccupiedNodes.Contains(i))
                        {
                            if(SpawnedCount.ContainsKey(j) && SpawnedCount[j] >= NodeObjects[j].MaxOfThisKind) continue;

                            if (rng <= NodeObjects[j].NodeProbability)
                            {
                                SpawnNodeObjects(j, i, k > 0);
                                //first object of this type placed
                                OccupiedNodes.Add(i);
                                if (!SpawnedCount.ContainsKey(j))
                                {
                                    SpawnedCount.Add(j, 1);
                                }
                                else
                                {
                                    SpawnedCount[j]++;
                                }

                                addedCollectible = NodeObjects[j].NodeType == NodeObjectType.Collectible;
                            }
                        }
                        else
                        {
                            if(// SKIP if not collectibles
                                !NodeObjects[j].NodeType.Contains(NodeObjectType.Collectible | NodeObjectType.SingleKillerPlatform )|| 
                                // or if they are spawned and
                                SpawnedCount.ContainsKey(j) &&
                                // either they are maxed out, 
                                SpawnedCount[j] >= NodeObjects[j].MaxOfThisKind || 
                                //OR more collectibles since we only want one collectible per node
                                addedCollectible && NodeObjects[j].NodeType.Contains(NodeObjectType.Collectible)) continue;

                            if (rng <= NodeObjects[j].NodeProbability)
                            {
                                SpawnNodeObjects(j, i, k > 0);
                                //first object of this type placed
                                if (!SpawnedCount.ContainsKey(j))
                                {
                                    SpawnedCount.Add(j, 1);
                                }
                                else
                                {
                                    SpawnedCount[j]++;
                                }

                                addedCollectible = NodeObjects[j].NodeType == NodeObjectType.Collectible;
                            }
                        }
                    }
                }
                


            }
            //add the encounter at the end
            for (int j = 0; j < NodeObjects.Count; j++)
            {
                if (NodeObjects[j].NodeType.Contains(NodeObjectType.Encounter))
                {
                    SpawnNodeObjects(j, _encounterIndex, false);
                    //first object of this type placed
                    OccupiedNodes.Add(_encounterIndex);
                    if (!SpawnedCount.ContainsKey(j))
                    {
                        SpawnedCount.Add(j, 1);
                    }
                    else
                    {
                        SpawnedCount[j]++;
                    }
                    break;
                }


            }
            yield return null;
        }

        private void TurnOffRuntimeUpdate()
        {
            foreach (var node in OccupiedNodesAttaches)
            {
                node.runtimeUpdate = false;
            }
        }

        private void SetCameraConfinerCollider()
        {
            var size = SpriteShapeController.spline.GetPosition(MainSpriteSides[0]).x -
                       SpriteShapeController.spline.GetPosition(0).x;// - GroundOffsetX /2;

            _cameraConfiner.center = new Vector3(size / 2 - GroundOffsetX, 0, 0);
            _cameraConfiner.size = new Vector3(size - (GroundOffsetX / 2), _cameraConfiner.size.y, _cameraConfiner.size.z);

            if (LevelBuilder.Instance.SceneType == LevelBuilder.SceneTypes.DownTube)
            {
                _cameraConfiner.center = new Vector3(size / 2 + GroundOffsetX, 0, 0);
                _cameraConfiner.size = new Vector3(size + (GroundOffsetX / 2), _cameraConfiner.size.y, _cameraConfiner.size.z);
            }
        }

        private void SpawnNodeObjects(int nodeobjectId, int nodeIndex, bool inverse)
        {
            var nodeAttach = NodeObjects[nodeobjectId].NodeObjectRef;

            nodeAttach.index = nodeIndex;
            nodeAttach.spriteShapeController = SpriteShapeController;
            var tpComponents = nodeAttach.gameObject.GetComponentsInChildren<TransitionPoint>();
            for (int i = 0; i < tpComponents.Length; i++)
            {
                tpComponents[i].transitioningGameObject = Player;
            }
            nodeAttach.runtimeUpdate = true;
        
            var node = Instantiate(nodeAttach.gameObject, NodeObjHolder);

            ProjectileShooter[] _projShooters = node.GetComponentsInChildren<ProjectileShooter>();
            for (int i=0; i < _projShooters.Length; i ++)
            {
                CreatePoolOfProjectiles(_projShooters[i]); //generate pool of projecitles
            }
            if (node.GetComponent<NodeObjectComponent>() == null)
            {
                var n = node.AddComponent<NodeObjectComponent>();


                n.PopulateNodeObjectComponent(
                    NodeObjects[nodeobjectId].NodeType,
                    NodeObjects[nodeobjectId].NodeObjectRef,
                    NodeObjects[nodeobjectId].MaxOfThisKind,
                    NodeObjects[nodeobjectId].NodeProbability,
                    NodeObjects[nodeobjectId].Scale,
                    NodeObjects[nodeobjectId].OffsetY,
                    NodeObjects[nodeobjectId].OffsetX
                );
            }

            var attach = node.GetComponent<NodeAttach>();
            attach.enabled = true;
            attach.yOffset = NodeObjects[nodeobjectId].OffsetY + _generalNodeOffset;
            if (NodeObjects[nodeobjectId].OffsetX != 0)
            {
                var o = NodeObjects[nodeobjectId].OffsetX;
                var rng = UnityEngine.Random.Range(o * -2, o * 2);
                while (rng > -o && rng < 0)
                {
                    rng = UnityEngine.Random.Range(o * -2, o * 2);
                }

                attach.xOffset = rng;
            }

            attach.Inversed = inverse;
            OccupiedNodesAttaches.Add(attach);


            //get nodeattach component
            node.SetActive(true);

            if (NodeObjects[nodeobjectId].NodeType.Contains(NodeObjectType.Teleport))
            {
                var mp = node.GetComponentInChildren<MovingPlatform>();
                if (mp != null)
                {
                    mp.enabled = true;
                    //mp.StartPlatform();
                }
            }


        }

        private void EnableOcclussion(GameObject go)
        {
            var cul = go.GetComponent<OcclussionComponent>();
            if (cul != null)
            {
                cul.enabled = true;
                cul.EnableOcclussion();
            }
        }

        private void EnableTP(GameObject node)
        {
            var cul = node.GetComponentsInChildren<TransitionPoint>();
            if (cul != null)
            {
                foreach (var point in cul)
                {
                    point.transitioningGameObject = Player;
                }

            }
        }

        private IEnumerator SpawnNpc()
        {
            var npc = Instantiate(NpcCatalogue.Instance.NpcList[0].CharPrefab);
            npc.transform.position = Player.transform.position + new Vector3(50, 0, 0);
            var controller = npc.GetComponent<NpcController>();
            yield return null;
            controller.Init();
        }

        private IEnumerator SpawnPlayer()
        {
            GameManager.Instance.SetPlayerStats();
            GameManager.Instance.PlayerPrefab.SetActive(false);
            Player = Instantiate(GameManager.Instance.PlayerPrefab);
            //set player above node 1
            Player.transform.position = SpriteShapeController.spline.GetPosition(1) + new Vector3(GroundOffsetX, 5, 0);
            CamSetup.Follow = Player.transform;
            CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX =
                SceneType.Contains(SceneTypes.Climber | SceneTypes.FlappyBird) ? 0.15f : 0.5f;
            CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY =
                SceneType.Contains(SceneTypes.Climber | SceneTypes.FlappyBird) ? 0.55f : 0.75f;
            // add boss on first level
            if (SceneManager.GetActiveScene().buildIndex == 1)
            {
                boss = Instantiate(_bossPrefab, Player.transform.position, Quaternion.identity);
                boss.GetComponent<MagnetCollectibles>().enabled = false;
                boss.transform.position += new Vector3(0, 2,0) ;
                boss.GetComponent<HandleBossIntro>().enabled = true;
            }

            if (SceneType.Contains(SceneTypes.DownTube))
            {
                CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX = 0.5f;
                CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY = 0.25f;
            }
            SpawnExit();
            yield return null;

            StartGame();
        
        }

        private void StartGame()
        {
            PlayerPrefs.SetInt("LastSceneId", SceneManager.GetActiveScene().buildIndex);
            PlayerPrefs.Save();
            StartCoroutine(EnablePlayer());
        }

        public IEnumerator EnablePlayer()
        {
            yield return null;
            Player.SetActive(true);
            var rb = Player.GetComponent<Rigidbody2D>();
            rb.drag = SceneType.Contains(SceneTypes.Climber | SceneTypes.FlappyBird | SceneTypes.Ascend ) ? 0.1f : 2.5f;
            //rb.gravityScale = SceneType.Contains(SceneTypes.Climber | SceneTypes.FlappyBird | SceneTypes.Ascend ) ? 1f : 2f;
            rb.freezeRotation = SceneType.Contains(SceneTypes.Ascend);
            rb.freezeRotation = SceneType.Contains(SceneTypes.DownTube);
            rb.simulated = false;
            rb.sharedMaterial = Player.GetComponent<Collider2D>().sharedMaterial = GameManager.Instance.PlayerMaterial;
            
            ControlManager.Instance.EnableBall();
            BallController.Started = true;
            BallController.AllowMovement = true;
            if (boss != null)
            {
                boss.GetComponent<MagnetCollectibles>().enabled = true;
                boss.GetComponent<HandleBossIntro>().ToggleBossLaughing(true);
            }

            //StartCoroutine(SpawnNpc());
        }

        private void SpawnWelcome()
        {
            var welcome = Instantiate(PostPrefab, NodeObjHolder);
            var node = welcome.GetComponent<NodeAttach>();
            node.spriteShapeController = SpriteShapeController;
            OccupiedNodesAttaches.Add(node);
            node.index = 1;
            var post = welcome.GetComponent<InfoPost>();
            post.InfoPostId = 0;
            post.InfoString = InfoPostsCatalogue._instance.InfoPosts[0].InfoText;
            if (SceneLoader.Instance.timerbar != null)
            {
                StopCoroutine(SceneLoader.Instance.timerbar);
            }
            SceneLoader.Instance.FillTimer.fillAmount = 1f;
            
            if (SceneManager.GetSceneByBuildIndex(0).isLoaded)
            {
                SceneLoader.Instance.Canvas.enabled = false;
            }
        }



        private void EnableLateStuff()
        {
            for (int i = 0; i < OccupiedNodesAttaches.Count; i++)
            {
            
                TurnOffRuntimeUpdate();
            }
        }
        private void EnableVeryLateStuff()
        {
            for (int i = 0; i < OccupiedNodesAttaches.Count; i++)
            {

                if (OccupiedNodesAttaches[i] != null)
                {
                    EnableOcclussion(OccupiedNodesAttaches[i].gameObject);
                    EnableTP(OccupiedNodesAttaches[i].gameObject);
                }

            }

            for (int i = 0; i < _backgrounds.Count; i++)
            {
                EnableOcclussion(_backgrounds[i]);
            }
        }

        private void SpawnExit()
        {
            var nodeIndex = _levelExitIndex;
            global::Logger.Log("!!!~~~~ Spawned exit at node " + nodeIndex);
            
            //yield return new WaitForSecondsRealtime(3f);
            Exit = Instantiate(SceneExitPrefab, NodeObjHolder);
            
            Exit.transform.position = new Vector3(SpriteShapeController.spline.GetPosition(nodeIndex).x -ReferencePointMaxDistance, Player.transform.position.y, 0);
            var tp = Exit.GetComponentInChildren<LoadNewScene>();
            var node = Exit.GetComponent<NodeAttach>();
            node.spriteShapeController = SpriteShapeController;
            node.index = nodeIndex;
            node.yOffset = SceneExitNodeOffsetY;
            node.xOffset = 0;//-ReferencePointMaxDistance;
            node.enabled = true;
            node.runtimeUpdate = true;
            OccupiedNodesAttaches.Add(node);
            //yield return new WaitForSecondsRealtime(1f);

            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            global::Logger.Log("~~~~~~~ " +  sceneIndex +" " +SceneManager.sceneCount);
            if (sceneIndex < SceneManager.sceneCountInBuildSettings -1)
            {
                tp.SceneDestination = sceneIndex + 1;
                global::Logger.Log("SceneManager.GetActiveScene().buildIndex "+sceneIndex);
            }
            else
            {
                tp.SceneDestination = 1;
            }

            //EnableOcclussion(Exit);
            //var t = 1f;
            //while (t > 0)
            //{
            //    t -= Time.deltaTime;
            //    yield return null;
            //}

            //node.runtimeUpdate = false;
            
        }

        /*   void Update()
       {
           if(player== null)return;
           if (player.transform.position.x > 217 * (lastMileStone +1) - 217 / 2)
           {
               if (lastMileStone % 2 == 0)
               {
                   var bg1 = BG1.transform.localPosition;
                   bg1.x += 434;
                   BG1.transform.localPosition = bg1;
                   //var bg1s = BG1.transform.localScale;
                   //bg1s.x *= -1;
                   //BG1.transform.localScale = bg1s;
               }
               else
               {
                   var bg2 = BG2.transform.localPosition;
                   bg2.x += 434;
                   BG2.transform.localPosition = bg2;
                   //var bg2s = BG2.transform.localScale;
                   //bg2s.x *= -1;
                   //BG2.transform.localScale = bg2s;
               }

               lastMileStone++;
           }
       }*/
    }
}
