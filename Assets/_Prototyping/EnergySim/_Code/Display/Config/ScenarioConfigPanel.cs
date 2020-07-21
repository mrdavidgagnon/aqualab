using System;
using System.Runtime.InteropServices;
using BeauUtil;
using UnityEngine;
using UnityEngine.UI;

namespace ProtoAqua.Energy
{
    public class ScenarioConfigPanel : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private Button m_RandomizeButton = null;
        [SerializeField] private Button m_ExportButton = null;

        [SerializeField] private ConfigPropertyBox m_Properties;

        #endregion // Inspector

        public event Action<ScenarioPackage> OnScenarioHeaderChanged;
        public event Action<ScenarioPackage> OnScenarioActorsChanged;

        [NonSerialized] private ScenarioPackage m_Target;
        [NonSerialized] private ISimDatabase m_Database;

        #region Unity Events

        private void Awake()
        {
            m_RandomizeButton.onClick.AddListener(Randomize);
            m_ExportButton.onClick.AddListener(Export);
        }

        #endregion // Unity Events

        public void Populate(ScenarioPackage inScenario, ISimDatabase inDatabase)
        {
            m_Target = inScenario;
            m_Database = inDatabase;

            m_Properties.Clear();

            m_Properties.BeginGroup("Info");
            {
                ConfigPropertyText.Configuration idConfig = new ConfigPropertyText.Configuration()
                {
                    Name = "Id",
                    MaxLength = 32,
                    Get = () => m_Target.Header.Id,
                    Set = (v) => { m_Target.Header.Id = v; OnScenarioHeaderChanged?.Invoke(m_Target); }
                };
                m_Properties.Text(idConfig);

                ConfigPropertyText.Configuration nameConfig = new ConfigPropertyText.Configuration()
                {
                    Name = "Name",
                    MaxLength = -1,
                    Get = () => m_Target.Header.Name,
                    Set = (v) => { m_Target.Header.Name = v; OnScenarioHeaderChanged?.Invoke(m_Target); }
                };
                m_Properties.Text(nameConfig);

                ConfigPropertyText.Configuration authorConfig = new ConfigPropertyText.Configuration()
                {
                    Name = "Author",
                    MaxLength = -1,
                    Get = () => m_Target.Header.Author,
                    Set = (v) => { m_Target.Header.Author = v; OnScenarioHeaderChanged?.Invoke(m_Target); }
                };
                m_Properties.Text(authorConfig);

                ConfigPropertyText.Configuration descriptionConfig = new ConfigPropertyText.Configuration()
                {
                    Name = "Description",
                    MaxLength = -1,
                    Get = () => m_Target.Header.Description,
                    Set = (v) => { m_Target.Header.Description = v; OnScenarioHeaderChanged?.Invoke(m_Target); }
                };
                m_Properties.Text(descriptionConfig);

                ConfigPropertySpinner.Configuration difficultyConfig = new ConfigPropertySpinner.Configuration()
                {
                    Name = "Rules to Randomize",

                    Suffix = " rules",
                    SingularText = "1 rule",

                    Min = 1,
                    Max = 3,
                    Increment = 1,
                    WholeNumbers = true,

                    Get = () => m_Target.Header.Difficulty,
                    Set = (v) => { m_Target.Header.Difficulty = (ushort)v; OnScenarioHeaderChanged?.Invoke(m_Target); Dirty(); }
                };

                m_Properties.Spinner(difficultyConfig);

                ConfigPropertySpinner.Configuration randomConfig = new ConfigPropertySpinner.Configuration()
                {
                    Name = "Random Seed",
                    Min = 0,
                    Max = ushort.MaxValue,
                    Increment = 1,
                    WholeNumbers = true,

                    Get = () => m_Target.Data.Seed,
                    Set = (v) => { m_Target.Data.Seed = (ushort)v; Dirty(); }
                };

                m_Properties.Spinner(randomConfig);
            }
            m_Properties.EndGroup();

            m_Properties.BeginGroup("Actors");
            {
                for (int i = 0; i < m_Target.Data.InitialActors.Length; ++i)
                {
                    int cachedIdx = i;
                    ActorCount count = m_Target.Data.InitialActors[cachedIdx];
                    ActorType actorType = inDatabase.Actors[count.Id];
                    ActorType.ConfigRange config = actorType.ConfigSettings();

                    ConfigPropertySpinner.Configuration spinnerConfig = new ConfigPropertySpinner.Configuration()
                    {
                        Name = actorType.ScriptName().ToString(),

                        Min = 0,
                        Max = config.SoftCap,
                        Increment = config.Increment,
                        WholeNumbers = true,

                        Get = () => m_Target.Data.InitialActors[cachedIdx].Count,
                        Set = (v) => { m_Target.Data.InitialActors[cachedIdx].Count = (ushort)v;  OnScenarioActorsChanged?.Invoke(m_Target); Dirty(); }
                    };

                    m_Properties.Spinner(spinnerConfig);
                }
            }
            m_Properties.EndGroup();

            m_Properties.BeginGroup("Resources");
            {
                for (int i = 0; i < m_Target.Data.InitialResources.Length; ++i)
                {
                    int cachedIdx = i;
                    VarPair amount = m_Target.Data.InitialResources[cachedIdx];
                    VarType varType = inDatabase.Vars[amount.Id];
                    VarType.ConfigRange config = varType.ConfigSettings();

                    ConfigPropertySpinner.Configuration spinnerConfig = new ConfigPropertySpinner.Configuration()
                    {
                        Name = varType.ScriptName().ToString(),

                        Min = config.Min,
                        Max = config.Max,
                        Increment = config.Increment,
                        WholeNumbers = true,

                        Get = () => m_Target.Data.InitialResources[cachedIdx].Value,
                        Set = (v) => { m_Target.Data.InitialResources[cachedIdx].Value = (short)v; Dirty(); }
                    };

                    m_Properties.Spinner(spinnerConfig);
                }
            }
            m_Properties.EndGroup();

            m_Properties.BeginGroup("Properties");
            {
                for (int i = 0; i < m_Target.Data.InitialProperties.Length; ++i)
                {
                    int cachedIdx = i;
                    VarPairF amount = m_Target.Data.InitialProperties[cachedIdx];
                    VarType varType = inDatabase.Vars[amount.Id];
                    VarType.ConfigRange config = varType.ConfigSettings();

                    ConfigPropertySpinner.Configuration spinnerConfig = new ConfigPropertySpinner.Configuration()
                    {
                        Name = varType.ScriptName().ToString(),

                        Min = config.Min,
                        Max = config.Max,
                        Increment = config.Increment,
                        WholeNumbers = false,

                        Get = () => m_Target.Data.InitialProperties[cachedIdx].Value,
                        Set = (v) => { m_Target.Data.InitialProperties[cachedIdx].Value = v; Dirty(); }
                    };

                    m_Properties.Spinner(spinnerConfig);
                }
            }
            m_Properties.EndGroup();

            m_Properties.BeginGroup("Time");
            {
                // ConfigPropertySpinner.Configuration actionCountConfig = new ConfigPropertySpinner.Configuration()
                // {
                //     Name = "Action Count",
                //     Min = 1,
                //     Max = 255,
                //     Increment = 1,
                //     WholeNumbers = true,

                //     Get = () => m_Target.Data.TickActionCount,
                //     Set = (v) => { m_Target.Data.TickActionCount = (int)v; Dirty(); }
                // };
                // m_Properties.Spinner(actionCountConfig);

                ConfigPropertySpinner.Configuration durationConfig = new ConfigPropertySpinner.Configuration()
                {
                    Name = "Duration",

                    Suffix = " ticks",
                    SingularText = "1 tick",

                    Min = 1,
                    Max = 255,
                    Increment = 1,
                    WholeNumbers = true,

                    Get = () => m_Target.Data.Duration,
                    Set = (v) => { m_Target.Data.Duration = (ushort)v; Dirty(); }
                };
                m_Properties.Spinner(durationConfig);
            }
            m_Properties.EndGroup();
        }

        private void Randomize()
        {
            for (int i = 0; i < m_Target.Data.InitialActors.Length; ++i)
            {
                int cachedIdx = i;
                ref ActorCount count = ref m_Target.Data.InitialActors[cachedIdx];
                ActorType actorType = m_Database.Actors[count.Id];
                ActorType.ConfigRange config = actorType.ConfigSettings();

                uint randomVal = (uint)RNG.Instance.Next(0, config.SoftCap + 1);
                count.Count = randomVal;
            }

            for (int i = 0; i < m_Target.Data.InitialResources.Length; ++i)
            {
                int cachedIdx = i;
                ref VarPair amount = ref m_Target.Data.InitialResources[cachedIdx];
                VarType varType = m_Database.Vars[amount.Id];
                VarType.ConfigRange config = varType.ConfigSettings();

                short randomVal = (short)RNG.Instance.NextFloat(config.Min, config.Max);
                amount.Value = randomVal;
            }

            for (int i = 0; i < m_Target.Data.InitialProperties.Length; ++i)
            {
                int cachedIdx = i;
                ref VarPairF amount = ref m_Target.Data.InitialProperties[cachedIdx];
                VarType varType = m_Database.Vars[amount.Id];
                VarType.ConfigRange config = varType.ConfigSettings();

                float randomVal = Mathf.Round(RNG.Instance.NextFloat(config.Min, config.Max) / 0.1f) * 0.1f;
                amount.Value = randomVal;
            }

            m_Target.Data.Seed = (ushort) RNG.Instance.Next(0, ushort.MaxValue);

            m_Properties.SyncAll();
            Dirty();
        }

        private void Export()
        {
            DateTime now = DateTime.UtcNow;
            long nowLong = now.ToFileTimeUtc();
            m_Target.Header.LastUpdated = nowLong;

            string exported = ScenarioPackage.Export(m_Target);
            QueryParams urlParams = new QueryParams();
            urlParams.Set(SimLoader.Param_ScenarioData, exported);

            string url = GetBaseURL() + urlParams.Encode();
            GUIUtility.systemCopyBuffer = url;

            // #if !UNITY_EDITOR
            Application.OpenURL(url);
            // #endif // !UNITY_EDITOR

            Debug.LogFormat("Exported scenario to '{0}'", url);
        }

        public void Clear()
        {
            m_Properties.Clear();
            m_Database = null;
            m_Target = null;
        }

        private void Dirty()
        {
            m_Target?.Data.Dirty();
        }

        static private string GetBaseURL()
        {
#if !UNITY_EDITOR
            string baseUrl = Application.absoluteURL;
            int queryIdx = baseUrl.IndexOf('?');
            if (queryIdx >= 0)
            {
                baseUrl = baseUrl.Substring(0, queryIdx);
            }
            return baseUrl;
#else
            return "http://localhost/";
#endif // UNITY_EDITOR
        }
    }
}