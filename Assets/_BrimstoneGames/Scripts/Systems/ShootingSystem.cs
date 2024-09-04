using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Gamekit2D;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _DPS
{
    public class ShootingSystem : Singleton<ShootingSystem>
    {
        protected ShootingSystem(){}
    
        #region GunsAndProjectiles

        [Header("Base gun prefab and powerupPrefab")]
        public GameObject GunPrefab;
        public GameObject GunPickPrefab;

        [Header("Gameplay refs")] 
        public Collider2D LineFailCollider; 
        public Collider2D CollectorCollider; 

        [Header("Odds for controlling gun types")]
        public float GunRarityModifier = 2f;

        [Header("Projectiles Pools")] 
        [SerializeField]
        private int projectilePoolSize;
        public Transform ProjectilePoolHolder { get; set; }

        [Header("Projectile Settings")] 
        public Collider2D ProjectileCollector;
        [SerializeField]
        private List<GunPool> _gunProjectilesList = new List<GunPool>();

    

        // vars guns


        /// <summary>
        /// key: int = guntype,  value: key = poolId/firemode, projectilecomponents array
        /// </summary>
        public static Dictionary<int, Dictionary<int, ProjectileComponent[]>> ProjectilePools = new Dictionary<int, Dictionary<int, ProjectileComponent[]>>();
        public static List<GunPowerUpPick> SpawnedGuns = new List<GunPowerUpPick>();
        public static Action<GunPowerUpPick> GunPick;
        public static Action<GunPowerUpPick> RemoveGunFromSpawnedList;
        public static Action GunRemove;
        /// <summary>
        /// spawns a gun on one side
        /// template,
        /// fallingSpeed,
        /// </summary>
        public static Action<GunPowerUpsEntity, float> SpawnGunPickOfType;
        /// <summary>
        /// will fetch a random gun based on pre-defined rarity rules
        /// float fallingSpeed
        /// </summary>
        public static Action<float> SpawnRandomGunPick;
        /// <summary>
        /// will fetch a random gun based on Specified rarity rules
        /// float fallingSpeed
        /// </summary>
        public static Action<float, Transform, Collider2D> SpawnRandomGunPickOfRarity;
        /// <summary>
        /// will fetch a random gun based on Specified rarity rules
        /// float fallingSpeed, vector3 pos, catch collider override
        /// </summary>
        public static Action<float, Vector3, Collider2D> SpawnRandomGunPickOfRarityOnPosition;

        public static Action<FallingObjectComponent> FallingObjEvent;
        private GunComponent _gunComponent;
        private TriggerShooting _currentTriggerController;
        #endregion

        #region FallingObjects
        [Header("Falling Objects Section")]
        [SerializeField]
        private GameObject fallingObjPrefab;
        [SerializeField]
        private int fallingPoolSize;
        public Transform FallingPoolHolder { get; set; }
        
        public FallingObj[] _fallingObjList;// = new List<FallingObj>();

        /// <summary>
        /// key: int = objType,  value: key = poolId, objcomponent array
        /// </summary>
        public static Dictionary<int, Dictionary<int, FallingObjectComponent[]>> FallingObjPools = new Dictionary<int, Dictionary<int, FallingObjectComponent[]>>();

        #endregion
        public bool EnableShootingMode;
        private float fallingSpreadOffestX;

        [Serializable]
        public class FallingObj
        {
            public FallingObjTypes ObjectType;
            public FallingObjectsParams[] FallingObjList;
        }

        [Serializable]
        public class FallingObjectsParams
        {
            [HideInInspector]
            public int PoolId = -1;
            public GameObject FallingPrefab;
            [Header("Params")] 
            [HideInInspector]
            public FallingObjTypes FallingobjType;

            public bool IsCollectible;

            [Header("Npc Entity - only for NPC/Enemy")]
            public NpcEntity NpcEntity;
            public float FallingSpeed = 1;
            public float FallingDamage = 1;
            public float FallingObjScale = 1;
            [Header("Optional - score and srite are in sync")] 
            public Sprite[] FallingObjSprite;
            public int[] FallingObjScore;
        }

        [Serializable]
        public class GunPool
        {
            public GunTypes Gun;
            public List<Projectile> Projectiles;
        }

        [Serializable]
        public class Projectile
        {
            public GameObject ProjetilePrefab;
            [Header("Params")] 
            public FiringModes ProjectileFireMode;
            [HideInInspector]
            public GunTypes ProjectileGunType;
            public float ProjectileSpeed = 1;
            public int ProjectileDamage = 1;
            public float ProjectileScale = 1;
        }

        public enum FallingObjTypes
        {
            Enemies,
            Animals,
            Rocks
        }

        public enum GunTypes
        {
            Canon,
            BlunderBus,
            AAGun
        }


        public enum FiringModes
        {
            Single,
            Double,
            AreaSplash
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
            //sub to events
            GunPick += OnGunPick;
            RemoveGunFromSpawnedList += OnRemovePick;
            GunRemove += OnGunRemove;
            //SpawnGunPickOfType += OnGunPickAdd;
            //SpawnRandomGunPick += OnGunPickAdd;
            SpawnRandomGunPickOfRarity += OnGunPickAdd;
            SpawnRandomGunPickOfRarityOnPosition += OnGunPickAdd;
        }

        private void OnRemovePick(GunPowerUpPick pick)
        {
            SpawnedGuns.Remove(pick);
        }

        void Start()
        {
            //InitPools();
        }

        public void InitPools()
        {
            ConstructProjectilePools();
            ConstructFallingObjPools();
        }

        #region GunHandling
        /// <summary>
        /// get random guntype
        /// </summary>
        /// <returns></returns>
        public static int GetRandomGunType()
        {
            var rng = Random.Range(0, GunsPowerUpsaCatalogue.Instance.GunPowerUpsEntities.Count);
            return (int)GunsPowerUpsaCatalogue.Instance.GunPowerUpsEntities[rng].GunType;
        }

        /// <summary>
        /// returns guntemplate based on rarity
        /// </summary>
        /// <param name="rarityOverride">overrides default rarity</param>
        /// <returns></returns>
        public static GunPowerUpsEntity GetRandomGunPowerUpsEntity(float rarityOverride = 0)
        {
            //c
            var type = GetRandomGunType();
            
            global::Logger.Log("type " + type); 
            //get all of rng type
            var gList = GunsPowerUpsaCatalogue.Instance.GunPowerUpsEntities.FindAll(g => (int)g.GunType == type);
            global::Logger.Log("glist " + gList.Count); 
            // apply rarity skew
            return gList[Utils.SkewedRandomRange(0, gList.Count-1, rarityOverride > 0 ? rarityOverride : Instance.GunRarityModifier)];
        }


        /// <summary>
        /// returns a random gun by rarity from guntype
        /// </summary>
        /// <param name="gunType"></param>
        /// <param name="overrideRarity"></param>
        /// <returns></returns>
        public static int GetGun(GunTypes gunType, float overrideRarity = 0)
        {
            var rarity = overrideRarity > 0 ? overrideRarity : Instance.GunRarityModifier;
            //get gun count per type
            var gunsOfTpe = GunsPowerUpsaCatalogue.Instance.GunPowerUpsEntities.FindAll(g => g.GunType == gunType);
            return Utils.SkewedRandomRange(0, gunsOfTpe.Count, rarity);
        }

        /// <summary>
        /// spawns a gun on one side based on template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="fallingSpeed"></param>
        private void OnGunPickAdd(GunPowerUpsEntity template, float fallingSpeed)
        {
            //create a powerup pick object
            var iLoc = Random.Range(FallingPoolHolder.position.x - fallingSpreadOffestX, FallingPoolHolder.position.x + fallingSpreadOffestX);

            var pickGo = Instantiate(GunPickPrefab, new Vector3(iLoc, FallingPoolHolder.position.y, FallingPoolHolder.position.z), Quaternion.identity);
            var pick = pickGo.GetComponent<GunPowerUpPick>();
            //populate params
            pick.GunImage.sprite = template.GunSprite;
            pick.BorderImage.color = template.PowerUpBorderColor;
            pick.GunPowerUpId = template.GunPowerUpId;
            pick.FireModes = template.FireModes;
            pick.FireRate = template.FireRate;
            pick.ProjectileSpeed = template.ProjectileSpeed;
            pick.GunType = template.GunType;

            pickGo.SetActive(true);
            pick.StartFalling(fallingSpeed);
        }

        /// <summary>
        /// spawns a gun on one side gets template by pre-defined rarity
        /// </summary>
        /// <param name="fallingSpeed"></param>
        /// <param name="overrideEmmiter"></param>
        /// <param name="overridecatcher"></param>
        private void OnGunPickAdd(float fallingSpeed, Transform overrideEmmiter, Collider2D overridecatcher)
        {
            overrideEmmiter = overrideEmmiter == null ? FallingPoolHolder : overrideEmmiter;
            var template = GetRandomGunPowerUpsEntity();
            //create a powerup pick object
            var iLoc = Random.Range(overrideEmmiter.position.x - fallingSpreadOffestX, overrideEmmiter.position.x + fallingSpreadOffestX);

            var pickGo = Instantiate(GunPickPrefab, new Vector3(iLoc, overrideEmmiter.position.y, overrideEmmiter.position.z), Quaternion.identity);
            var pick = pickGo.GetComponent<GunPowerUpPick>();
            //populate params
            pick.GunImage.sprite = template.GunSprite;
            pick.BorderImage.color = template.PowerUpBorderColor;
            pick.GunPowerUpId = template.GunPowerUpId;
            pick.FireModes = template.FireModes;
            pick.FireRate = template.FireRate;
            pick.ProjectileSpeed = template.ProjectileSpeed;
            pick.GunType = template.GunType;
            pick.CatcherOverride = overridecatcher;
            // add to list
            SpawnedGuns.Add(pick);

            pickGo.SetActive(true);
            pick.StartFalling(fallingSpeed);
        }

        /// <summary>
        /// spawns a gun on one side gets template by pre-defined rarity
        /// </summary>
        /// <param name="fallingSpeed"></param>
        /// <param name="overridePosition"></param>
        /// <param name="overridecatcher"></param>
        private void OnGunPickAdd(float fallingSpeed, Vector3 overridePosition, Collider2D overridecatcher)
        {
            var template = GetRandomGunPowerUpsEntity();
            //create a powerup pick object
            
            var pickGo = Instantiate(GunPickPrefab, overridePosition, Quaternion.identity);
            var pick = pickGo.GetComponent<GunPowerUpPick>();
            //populate params
            pick.GunImage.sprite = template.GunSprite;
            pick.BorderImage.color = template.PowerUpBorderColor;
            pick.GunPowerUpId = template.GunPowerUpId;
            pick.FireModes = template.FireModes;
            pick.FireRate = template.FireRate;
            pick.ProjectileSpeed = template.ProjectileSpeed;
            pick.GunType = template.GunType;
            pick.CatcherOverride = overridecatcher;
            // add to list
            SpawnedGuns.Add(pick);

            pickGo.SetActive(true);
            pick.StartFalling(fallingSpeed);
        }

        /// <summary>
        /// spawns a gun on one side gets template by override/specified rarity
        /// </summary>
        /// <param name="fallingSpeed"></param>
        /// <param name="fireRate"></param>
        /// <param name="fireMode"></param>
        private void OnGunPickAdd(float fallingSpeed, float rarity)
        {
            var template = GetRandomGunPowerUpsEntity(rarity);
            //create a powerup pick object
            var iLoc = Random.Range(FallingPoolHolder.position.x - fallingSpreadOffestX, FallingPoolHolder.position.x + fallingSpreadOffestX);

            var pickGo = Instantiate(GunPickPrefab, new Vector3(iLoc, FallingPoolHolder.position.y, FallingPoolHolder.position.z), Quaternion.identity);
            var pick = pickGo.GetComponent<GunPowerUpPick>();
            //populate params
            pick.GunImage.sprite = template.GunSprite;
            pick.BorderImage.color = template.PowerUpBorderColor;
            pick.GunPowerUpId = template.GunPowerUpId;
            pick.FireModes = template.FireModes;
            pick.FireRate = template.FireRate;
            pick.ProjectileSpeed = template.ProjectileSpeed;
            pick.GunType = template.GunType;

            pickGo.SetActive(true);
            pick.StartFalling(fallingSpeed);
        }

        private void OnGunPick(GunPowerUpPick powerUpPick)
        {
            global::Logger.Log("Picking gun " + powerUpPick.GunType);
            //add / refresh gun
            if (_gunComponent == null)
            {
                var gun = Instantiate(GunPrefab, LevelBuilder.Player.transform);
                //disable char image
                // BallController._instance.CharacterImage.enabled = false;
                //get guncomponent
                _gunComponent = gun.GetComponent<GunComponent>();
                //add sprite
                _gunComponent.GunSprite.sprite = powerUpPick.GunImage.sprite;
                // populate params
                _gunComponent.GunPowerUpId = powerUpPick.GunPowerUpId;
                _gunComponent.FireRate = powerUpPick.FireRate;
                _gunComponent.ProjectileSpeed = powerUpPick.ProjectileSpeed;
                _gunComponent.GunType = powerUpPick.GunType;
                _gunComponent.FireModes = powerUpPick.FireModes;
                _gunComponent.Catcher = powerUpPick.CatcherOverride;
            }
            else
            {
                //disable char image
                //BallController._instance.CharacterImage.enabled = false;

                // populate params
                _gunComponent.GunSprite.sprite = powerUpPick.GunImage.sprite;
                _gunComponent.GunPowerUpId = powerUpPick.GunPowerUpId;
                _gunComponent.FireRate = powerUpPick.FireRate;
                _gunComponent.ProjectileSpeed = powerUpPick.ProjectileSpeed;
                _gunComponent.GunType = powerUpPick.GunType;
                _gunComponent.FireModes = powerUpPick.FireModes;
                _gunComponent.Catcher = powerUpPick.CatcherOverride;
            }

            //start shooting
            if (!EnableShootingMode)
            {
                EnableShootingMode = true;
                AudioManager.Instance.Play("powerUp");
            }

            BallController.CurrentGun = _gunComponent;
            _gunComponent.EnableShooting = true;
            //remove from list
            SpawnedGuns.Remove(powerUpPick);
            Destroy(powerUpPick.gameObject);
        }

        private void OnGunRemove()
        {
            if (_gunComponent != null)
            {
                _gunComponent.EnableShooting = false;
                Destroy(_gunComponent.gameObject);
                _gunComponent = null;
                ResetFallingAllObjects();
                //BallController._instance.CharacterImage.enabled = true;
                //BallController._instance.CharacterImage.transform.localScale = Vector3.one;
                //BallController._instance.ImgHolder.localScale = Vector3.one;
            }
        }

        #endregion

        #region Pooling
        //set size of pool array

        private void ConstructProjectilePools()
        {
            var g = Enum.GetValues(typeof(FiringModes)).Length;
            if (ProjectilePoolHolder == null)
            {
                //global::Logger.LogError("No ProjectilePoolHolder is set");
                return;
            }
            //for each gun
            for (int i = 0; i < _gunProjectilesList.Count; i++)
            {
                // size array and prepare dic
                var p = new Dictionary<int, ProjectileComponent[]>();
                //p.Add(i, new ProjectileComponent[GunProjectilesList[i].Projectiles.Count]);
                //for each gun == (int) _gunProjectilesList[i].Gun
                if (!ProjectilePools.ContainsKey((int) _gunProjectilesList[i].Gun))
                {
                    ProjectilePools.Add((int) _gunProjectilesList[i].Gun, p);
                }

                //j=poolID
                for (int j = 0; j < _gunProjectilesList[i].Projectiles.Count; j++)
                {
                    //prepare the pool
                    p.Add(j, new ProjectileComponent[projectilePoolSize]);
                    ProjectilePools[i]= p;


                    for (int k = 0; k < projectilePoolSize; k++)
                    {
                        //create pool for each projectile in list
                        var projectile = Instantiate(_gunProjectilesList[i].Projectiles[j].ProjetilePrefab, ProjectilePoolHolder);
                        projectile.transform.localScale = Vector3.one * _gunProjectilesList[i].Projectiles[j].ProjectileScale;
                        // set projectilecomponent
                        var pComponent = projectile.GetComponent<ProjectileComponent>()
                            ? projectile.GetComponent<ProjectileComponent>()
                            : projectile.AddComponent<ProjectileComponent>();
                        pComponent.GunType = _gunProjectilesList[i].Gun;
                        pComponent.FireMode = _gunProjectilesList[i].Projectiles[j].ProjectileFireMode;
                        pComponent.ProjectileDamage = _gunProjectilesList[i].Projectiles[j].ProjectileDamage;
                        pComponent.ProjectileSpeed = _gunProjectilesList[i].Projectiles[j].ProjectileSpeed;
                        pComponent.SetAutoRadius();
                        projectile.gameObject.SetActive(false);

                        //add into pools
                        ProjectilePools[i][j][k] = pComponent;
                    }
                }

            }
        }

        private void ConstructFallingObjPools()
        {
            if (FallingPoolHolder == null)
            {
                //global::Logger.LogError("No FallingPoolHolder is set");
                return;
            }
            //for each falling obj
            for (int i = 0; i < _fallingObjList.Length; i++)
            {
                // size array and prepare dic
                var f = new Dictionary<int, FallingObjectComponent[]>();

                //for each fallingobj
                var objTypeInt = (int)_fallingObjList[i].ObjectType;
                if (!FallingObjPools.ContainsKey(objTypeInt))
                {
                    FallingObjPools.Add(i, f);
                }
                else
                {
                    FallingObjPools[objTypeInt] = f;
                }
                
                for (int j = 0; j < _fallingObjList[objTypeInt].FallingObjList.Length; j++)
                {
                    //prepare the pool
                    f.Add(j, new FallingObjectComponent[fallingPoolSize]);
                    FallingObjPools[objTypeInt]= f;
                    //assign poolId
                    _fallingObjList[objTypeInt].FallingObjList[j].PoolId = j;

                    for (int k = 0; k < fallingPoolSize; k++)
                    {
                        //create pool for each projectile in list
                        var fallingObj = Instantiate(_fallingObjList[objTypeInt].FallingObjList[j].FallingPrefab, FallingPoolHolder);
                        fallingObj.transform.localScale *= _fallingObjList[objTypeInt].FallingObjList[j].FallingObjScale;
                        // set projectilecomponent
                        var fComponent = fallingObj.GetComponent<FallingObjectComponent>()
                            ? fallingObj.GetComponent<FallingObjectComponent>()
                            : fallingObj.AddComponent<FallingObjectComponent>();
                        //assign poolId to fcomponent
                        fComponent.PoolId = j;
                        fComponent.FallingobjType = _fallingObjList[objTypeInt].ObjectType;
                        fComponent.FallingDamage = _fallingObjList[objTypeInt].FallingObjList[j].FallingDamage;
                        fComponent.FallingObjScale = _fallingObjList[objTypeInt].FallingObjList[j].FallingObjScale;
                        //can change this later
                        var rng =Random.Range(0, _fallingObjList[objTypeInt].FallingObjList[j].FallingObjSprite.Length);
                        fComponent.FallingObjSprite.sprite = _fallingObjList[objTypeInt].FallingObjList[j].FallingObjSprite[rng];
                        var score = fallingObj.GetComponent<ScorePickComponent>();
                        if (score != null)
                        {
                            score.ScoreValue = _fallingObjList[objTypeInt].FallingObjList[j].FallingObjScore[rng];
                        }
                        fComponent.isCollectible = _fallingObjList[objTypeInt].FallingObjList[j].IsCollectible;
                        //if we got npctemplate
                        if (_fallingObjList[objTypeInt].FallingObjList[j].NpcEntity != null)
                        {
                            fComponent.NpcEntity = _fallingObjList[objTypeInt].FallingObjList[j].NpcEntity;
                        }

                        // if we got an npccontroller
                        var npcController = fallingObj.GetComponent<NpcController>();
                        if (npcController != null)
                        {
                            npcController.PoolId = j;
                            npcController.NpcTemplate = _fallingObjList[objTypeInt].FallingObjList[j].NpcEntity;
                            npcController.NpcId = _fallingObjList[objTypeInt].FallingObjList[j].NpcEntity !=null ? _fallingObjList[objTypeInt].FallingObjList[j].NpcEntity.NpcId : -1;
                        }


                        fallingObj.gameObject.SetActive(false);

                        //add into pools
                        FallingObjPools[objTypeInt][j][k] = fComponent;
                    }
                }

            }
        }



        /// <summary>
        /// returns a projectile component is the gameobject is inactive (available)
        /// returns null if all pool gameobjects are active (not available)
        /// </summary>
        /// <param name="gunType"></param>
        /// <param name="poolId"></param>
        /// <returns></returns>
        public static ProjectileComponent GetProjectile(int gunType, int poolId)
        {
/*            global::Logger.Log("Looking for gun " + gunType + " in poolId " + poolId);
            var str = "\n";
            for (int i = 0; i < ProjectilePools.Count; i++)
            {
                str += "Gun: " + i + " pools: ";
                for (int j = 0; j < ProjectilePools[i].Keys.Count; j++)
                {
                    str += j + ", ";
                }

                str += "\n";
            }
            global::Logger.Log("Available in pools " + str);*/
            var pArray = ProjectilePools[gunType][poolId];
            ProjectileComponent result = null;
            
                while (result == null)
                {
                    for (int i = 0; i < pArray.Length; i++)
                    {
                        if (!pArray[i].gameObject.activeSelf)
                        {
                            result = pArray[i];
                        }
                    }
                }


            return result;
        }

        /// <summary>
        /// returns a fallingObj component is the gameobject is inactive (available)
        /// returns null if all pool gameobjects are active (not available)
        /// </summary>
        /// <param name="fObjType"></param>
        /// <param name="poolId"></param>
        /// <returns></returns>
        public static FallingObjectComponent GetFallingObj(int fObjType, int poolId)
        {
            var pArray = FallingObjPools[fObjType][poolId];
            FallingObjectComponent result = null;

            while (result == null)
            {
                for (int i = 0; i < pArray.Length; i++)
                {
                    if (!pArray[i].gameObject.activeSelf)
                    {
                        result = pArray[i];
                    }
                }
            }
            return result;
        }
        #endregion

        #region FallingObjects

        /// <summary>
        /// shoots a projectile from pool towards a target
        /// </summary>
        /// <param name="fObjType">fType first pool key</param>
        /// <param name="target">intended target</param>
        /// <param name="overrideEmmiter">can override emmiter, otherwise uses default from pool settings</param>
        /// <param name="fallingSpeed"></param>
        /// <param name="catcherOverride"></param>
        public void StartFalling(FallingObjTypes fObjType,int poolId, Vector3 target, Vector3 overrideEmmiter, float fallingSpeed, Collider2D catcherOverride)
        {
/*
            if (fObjType == FallingObjTypes.Animals)
            {
                global::Logger.Log("~~~~ try falling " + fallingSpeed);
            }*/

            var fComp = GetFallingObj((int)fObjType, poolId);
            fComp.transform.position = overrideEmmiter;
            fComp.FallingobjType = fObjType;
            fComp.Target = target;
            fComp.CatcherOverride = catcherOverride;
            fComp.FallingSpeed = fallingSpeed;
            fComp.gameObject.SetActive(true);
            fComp.StartFalling();
        }

        public void ResetFallingObjectsOfTypeByPool(FallingObjTypes fObjType, int npcId)
        {
            for (int i = 0; i < FallingObjPools[(int)fObjType][npcId].Length; i++)
            {
                FallingObjPools[(int) fObjType][npcId][i].transform.localPosition = Vector3.zero;
                FallingObjPools[(int) fObjType][npcId][i].gameObject.SetActive(false);
            }

        }

        public void ResetFallingAllObjects()
        {
            for (int i = 0; i < FallingObjPools.Count; i++)
            {
                for (int j = 0; j < FallingObjPools[i].Count; j++)
                {
                    for (int k = 0; k < FallingObjPools[i][j].Length; k++)
                    {
                        FallingObjPools[i][j][k].transform.localPosition = Vector3.zero;
                        FallingObjPools[i][j][k].gameObject.SetActive(false);
                    }
                }

            }

        }

        public void ResetProjectiles()
        {
            for (int i = 0; i < ProjectilePools.Count; i++)
            {
                for (int j = 0; j < ProjectilePools[i].Count; j++)
                {
                    for (int k = 0; k < ProjectilePools[i][j].Length; k++)
                    {
                        ProjectilePools[i][j][k].transform.localPosition = Vector3.zero;
                        ProjectilePools[i][j][k].gameObject.SetActive(false);
                    }
                }

            }

        }
        #endregion

        #region Shooting

        /// <summary>
        /// shoots a projectile from pool towards a target
        /// </summary>
        /// <param name="gunType">gun type for projectile</param>
        /// <param name="fireMode"></param>
        /// <param name="target">intended target</param>
        /// <param name="overrideEmmiter">can override emmiter, otherwise uses default from pool settings</param>
        /// <param name="projectileSpeed"></param>
        /// <param name="catcher"></param>
        public static void Shoot(GunTypes gunType, ShootingSystem.FiringModes fireMode, Vector3 target, Transform overrideEmmiter, float projectileSpeed, Collider2D catcher)
        {
            var intFireMode = (int) fireMode;
            //global::Logger.Log("~~~~ try shooting " + projectilePoolId);
            //handle shake gun
            Instance._gunComponent.transform.localScale = Vector3.one;
            Instance._gunComponent.transform.DOShakeScale(.15f, new Vector3(0, 0.2f, 0),1,0,false).SetEase(Ease.InOutQuart).SetAutoKill().OnComplete(()=>{Instance._gunComponent.transform.localScale = Vector3.one;});

            var projectile = GetProjectile((int)gunType, intFireMode);
            projectile.transform.position = overrideEmmiter != null ? overrideEmmiter.position : projectile.ProjectileEmmiter.position;
            projectile.FireMode = fireMode;
            projectile.GunType = gunType;
            projectile.Target = target;
            projectile.ProjectileSpeed = projectileSpeed;
            projectile.Collector = catcher;
            projectile.gameObject.SetActive(true);
            projectile.Shoot();
        }

        #endregion

        #region Shooting Sequence Control

        public IEnumerator StartShootingSequence(TriggerShooting triggerController, NpcEntity npcEntity, Transform emmiter, BoxCollider2D projectileCatcher, Collider2D fallingCatcher, Transform pHolder = null, Transform fObjHolder= null)
        {
            if (Time.timeScale < .1)
            {
                yield return StartCoroutine(GameManager.HandlePauseState());
            }
            _currentTriggerController = triggerController;
            //set projectile holder
            if (pHolder != null)
            {
                ProjectilePoolHolder = pHolder;
            }

            if (fObjHolder != null)
            {
                FallingPoolHolder = fObjHolder;
            }
            
            ConstructProjectilePools();
            ConstructFallingObjPools();

            yield return new WaitForSeconds(.1f);
            if (Time.timeScale < .1)
            {
                yield return StartCoroutine(GameManager.HandlePauseState());
            }

            // spawn boss on emmiter middle
            npcEntity.CharPrefab.SetActive(false);
            var bossGo = Instantiate(npcEntity.CharPrefab, emmiter.position, Quaternion.identity);

            //set boss stats
            bossGo.transform.Find("Char").GetComponent<SpriteRenderer>().sortingOrder = 22;
            var npcController = bossGo.GetComponent<NpcController>();
            //set to boss first
            npcController.IsBoss = true;
            npcController.NpcTemplate = npcEntity;
            npcController.Emmiter = emmiter;
            npcController.Catcher = fallingCatcher;

            var fComp = bossGo.GetComponent<FallingObjectComponent>();
            fComp.Prop1 = null;
            npcController.FallingSpeed = fComp.FallingSpeed;
            
            //look in pool editor list for npcentity
            var f = Array.Find(_fallingObjList[(int) FallingObjTypes.Enemies].FallingObjList, (n => n.NpcEntity == npcEntity));
            if (f != null)
            {
                npcController.NpcId = f.NpcEntity.NpcId;
                npcController.PoolId = Array.IndexOf(_fallingObjList[(int) FallingObjTypes.Enemies].FallingObjList,f);
                fComp.FallingObjSprite.sprite = f.FallingObjSprite[Random.Range(0, f.FallingObjSprite.Length)];
            }
            
            //remove boss fcomp
            Destroy(fComp);

            //set boss active
            bossGo.SetActive(true);

            yield return new WaitForSeconds(0.1f);
            if (Time.timeScale < .1)
            {
                yield return StartCoroutine(GameManager.HandlePauseState());
            }
            //give gun to player
            SpawnRandomGunPickOfRarity.Invoke(2, LevelBuilder.Player.transform, projectileCatcher);
            //start shooting
            //first hit on boss start spawn falling objects from boss

        }

        public void SpawnExitEncounter()
        {
            ResetFallingAllObjects();
            // destroy picks
            for (int i = 0; i < SpawnedGuns.Count; i++)
            {
                Destroy(SpawnedGuns[i].gameObject);
            }
            _currentTriggerController.ExitEncounterHolder.gameObject.SetActive(true);
            var exitTp = _currentTriggerController.ExitEncounterHolder.GetComponentInChildren<TransitionPoint>();
            exitTp.transitioningGameObject = LevelBuilder.Player;
            exitTp.OriginalScreenX = _currentTriggerController.m_OriginalScreenX;
            exitTp.OriginalScreenY = _currentTriggerController.m_OriginalScreenY;
            exitTp.isExit = true;
        }

        /// <summary>
        /// returns a specified entity by id
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public NpcEntity GetNpcEntity(int npcId)
        {
            return NpcCatalogue.Instance.NpcList[npcId];
        }

        /// <summary>
        /// fetches a random npcentity
        /// </summary>
        /// <returns></returns>
        public NpcEntity GetRandomNpcEntity()
        {
            return GetNpcEntity(Random.Range(0, NpcCatalogue.Instance.NpcList.Count));
        }
        #endregion
    }
}