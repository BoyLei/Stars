using SGF.UI.Framework;
using Sirenix.OdinInspector;
using StarProject.Service.Sound;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.StarWorld
{
    public class SettingMusicParam : SettingBaseParam
    {
        [LabelText("滑动总音量")]
        public Slider SliderAllMusic;
        [LabelText("滑动总音量值")]
        public Text MusicNumAll;
        [LabelText("按钮总音量")]
        public JButton JbtnAllMusic;
        private SettingSave SaveAllMusic = null;

        [LabelText("滑动背景音效")]
        public Slider SliderBackgroundMusic;
        [LabelText("背景音效音量值")]
        public Text MusicNumBackground;
        [LabelText("按钮背景音量")]
        public JButton JbtnBackgroundMusic;
        private SettingSave SaveBackgroundMusic = null;

        [LabelText("滑动系统音效")]
        public Slider SliderSysteamMusic;
        [LabelText("系统音效音量值")]
        public Text MusicNumSysteam;
        [LabelText("按钮系统音量")]
        public JButton JbtnSysteamMusic;
        private SettingSave SaveSysteamMusic = null;

        [LabelText("滑动环境音效")]
        public Slider SliderSurroundingsMusic;
        [LabelText("环境音效音量值")]
        public Text MusicNumSurroundings;
        [LabelText("按钮环境音量")]
        public JButton JbtnSurroundingsMusic;
        private SettingSave SaveSurroundingsMusic = null;

        private Dictionary<string, SettingSave> musicSettingSaveDic = new();    // 音效设置缓存字典
        private bool isChanage = false;

        protected override void Awake()
        {
            base.Awake();

            SliderAllMusic.onValueChanged.AddListener(OnAllValueChanged);
            SliderBackgroundMusic.onValueChanged.AddListener(OnBackgroundValueChanged);
            SliderSysteamMusic.onValueChanged.AddListener(OnSysteamValueChanged);
            SliderSurroundingsMusic.onValueChanged.AddListener(OnSurroundingsValueChanged);

            JbtnAllMusic.OnClick += OnJbtnAllMusic;
            JbtnBackgroundMusic.OnClick += OnJbtnBackgroundMusic;
            JbtnSysteamMusic.OnClick += OnJbtnSysteamMusic;
            JbtnSurroundingsMusic.OnClick += OnJbtnSurroundingsMusic;

            // 读取音效设置的缓存
            {
                musicSettingSaveDic = SoundManager.Instance.MusicSettingSaveDic;
            }
            // 总音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_ALL_MUSIC, out SaveAllMusic))
                {

                }
                SaveAllMusic ??= new SettingSave();
            }
            // 背景音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_BACKGROUND_MUSIC, out SaveBackgroundMusic))
                {

                }
                SaveBackgroundMusic ??= new SettingSave();
            }
            // 系统音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_SYSTEAM_MUSIC, out SaveSysteamMusic))
                {

                }
                SaveSysteamMusic ??= new SettingSave();
            }
            // 环境音效
            {
                if (musicSettingSaveDic.TryGetValue(GameConfig.SETTING_SURROUNDINGS_MUSIC, out SaveSurroundingsMusic))
                {

                }
                SaveSurroundingsMusic ??= new SettingSave();
            }
        }

        public override void Reset()
        {
            base.Reset();
            isChanage = false;
        }

        public override void Init()
        {
            base.Init();
           
            // 总音效
            {
                SaveAllMusic ??= new SettingSave();
                InitAllMusic();
            }
            // 背景音效
            {
                SaveBackgroundMusic ??= new SettingSave();
                InitBackgroundMusic();
            }
            // 系统音效
            {
                SaveSysteamMusic ??= new SettingSave();
                InitSysteamMusic();
            }
            // 环境音效
            {
                SaveSurroundingsMusic ??= new SettingSave();
                InitSurroundingsMusic();
            }
        }

        #region 总音效

        private void InitAllMusic()
        {
            SliderAllMusic.value = SaveAllMusic.volume;
            MusicNumAll.text = SaveAllMusic.volume.ToString();

            JbtnAllMusic.transform.Find("On").gameObject.SetActive(!SaveAllMusic.isMute);
            JbtnAllMusic.transform.Find("Off").gameObject.SetActive(SaveAllMusic.isMute);
        }

        private void OnAllValueChanged(float value)
        {
            SaveAllMusic.volume = value;
            MusicNumAll.text = SaveAllMusic.volume.ToString();
            isChanage = true;

            ComparisonBackgroundMusic();
            ComparisonSysteamMusic();
            ComparisonSurroundingsMusic();

            SoundManager.Instance.SetMusicVolume(SoundType.Bus_volume, SaveAllMusic.volume);
        }

        private void OnJbtnAllMusic(GameObject go)
        {
            SaveAllMusic.isMute = !SaveAllMusic.isMute;
            SliderAllMusic.value = SaveAllMusic.isMute ? 0 : 10;
            JbtnAllMusic.transform.Find("On").gameObject.SetActive(!SaveAllMusic.isMute);
            JbtnAllMusic.transform.Find("Off").gameObject.SetActive(SaveAllMusic.isMute);
            isChanage = true;

            SoundManager.Instance.SetMusicState(SoundType.Bus_volume, SaveAllMusic.isMute);
        }

        #endregion

        #region 背景音效

        private void InitBackgroundMusic()
        {
            SliderBackgroundMusic.value = SaveBackgroundMusic.volume;
            MusicNumBackground.text = SaveBackgroundMusic.volume.ToString();
            JbtnBackgroundMusic.transform.Find("On").gameObject.SetActive(!SaveBackgroundMusic.isMute);
            JbtnBackgroundMusic.transform.Find("Off").gameObject.SetActive(SaveBackgroundMusic.isMute);
        }

        private void OnBackgroundValueChanged(float value)
        {
            SaveBackgroundMusic.volume = value;
            MusicNumBackground.text = SaveBackgroundMusic.volume.ToString();
            isChanage = true;

            SoundManager.Instance.SetMusicVolume(SoundType.Music_volume, SaveBackgroundMusic.volume);
        }

        private void OnJbtnBackgroundMusic(GameObject go)
        {
            SaveBackgroundMusic.isMute = !SaveBackgroundMusic.isMute;
            SliderBackgroundMusic.value = SaveBackgroundMusic.isMute ? 0 : 10;
            JbtnBackgroundMusic.transform.Find("On").gameObject.SetActive(!SaveBackgroundMusic.isMute);
            JbtnBackgroundMusic.transform.Find("Off").gameObject.SetActive(SaveBackgroundMusic.isMute);
            isChanage = true;

            SoundManager.Instance.SetMusicState(SoundType.Music_volume, SaveBackgroundMusic.isMute);
        }

        private void ComparisonBackgroundMusic()
        {
            if (SaveAllMusic.volume > SaveBackgroundMusic.volume)
            {
                return;
            }
            SaveBackgroundMusic.volume = SaveAllMusic.volume;
            InitBackgroundMusic();

            SoundManager.Instance.SetMusicVolume(SoundType.Music_volume, SaveBackgroundMusic.volume);
        }

        #endregion

        #region 系统音效

        private void InitSysteamMusic()
        {
            SliderSysteamMusic.value = SaveSysteamMusic.volume;
            MusicNumSysteam.text = SaveSysteamMusic.volume.ToString();
            JbtnSysteamMusic.transform.Find("On").gameObject.SetActive(!SaveSysteamMusic.isMute);
            JbtnSysteamMusic.transform.Find("Off").gameObject.SetActive(SaveSysteamMusic.isMute);
        }

        private void OnSysteamValueChanged(float value)
        {
            SaveSysteamMusic.volume = value;
            MusicNumSysteam.text = SaveSysteamMusic.volume.ToString();
            isChanage = true;

            SoundManager.Instance.SetMusicVolume(SoundType.UI_volume, SaveSysteamMusic.volume);
        }

        private void OnJbtnSysteamMusic(GameObject go)
        {
            SaveSysteamMusic.isMute = !SaveSysteamMusic.isMute;
            SliderSysteamMusic.value = SaveSysteamMusic.isMute ? 0 : 10;
            JbtnSysteamMusic.transform.Find("On").gameObject.SetActive(!SaveSysteamMusic.isMute);
            JbtnSysteamMusic.transform.Find("Off").gameObject.SetActive(SaveSysteamMusic.isMute);
            isChanage = true;

            SoundManager.Instance.SetMusicState(SoundType.UI_volume, SaveSysteamMusic.isMute);
        }

        private void ComparisonSysteamMusic()
        {
            if (SaveAllMusic.volume > SaveSysteamMusic.volume)
            {
                return;
            }
            SaveSysteamMusic.volume = SaveAllMusic.volume;
            InitSysteamMusic();

            SoundManager.Instance.SetMusicVolume(SoundType.UI_volume, SaveSysteamMusic.volume);
        }

        #endregion

        #region 环境音效

        private void InitSurroundingsMusic()
        {
            SliderSurroundingsMusic.value = SaveSurroundingsMusic.volume;
            MusicNumSurroundings.text = SaveSurroundingsMusic.volume.ToString();
            JbtnSurroundingsMusic.transform.Find("On").gameObject.SetActive(!SaveSurroundingsMusic.isMute);
            JbtnSurroundingsMusic.transform.Find("Off").gameObject.SetActive(SaveSurroundingsMusic.isMute);
        }

        private void OnSurroundingsValueChanged(float value)
        {
            SaveSurroundingsMusic.volume = value;
            MusicNumSurroundings.text = SaveSurroundingsMusic.volume.ToString();
            isChanage = true;

            SoundManager.Instance.SetMusicVolume(SoundType.Amb_volume, SaveSurroundingsMusic.volume);
        }

        private void OnJbtnSurroundingsMusic(GameObject go)
        {
            SaveSurroundingsMusic.isMute = !SaveSurroundingsMusic.isMute;
            SliderSurroundingsMusic.value = SaveSurroundingsMusic.isMute ? 0 : 10;
            JbtnSurroundingsMusic.transform.Find("On").gameObject.SetActive(!SaveSurroundingsMusic.isMute);
            JbtnSurroundingsMusic.transform.Find("Off").gameObject.SetActive(SaveSurroundingsMusic.isMute);
            isChanage = true;

            SoundManager.Instance.SetMusicState(SoundType.Amb_volume, SaveSurroundingsMusic.isMute);
        }

        private void ComparisonSurroundingsMusic()
        {
            if (SaveAllMusic.volume > SaveSurroundingsMusic.volume)
            {
                return;
            }
            SaveSurroundingsMusic.volume = SaveAllMusic.volume;
            InitSurroundingsMusic();

            SoundManager.Instance.SetMusicVolume(SoundType.Amb_volume, SaveSurroundingsMusic.volume);
        }

        #endregion

        #region 本地缓存

        public override void SaveLocalData()
        {
            base.SaveLocalData();

            if (isChanage == false)
            {
                return;
            }

            if (musicSettingSaveDic == null)
            {
                musicSettingSaveDic = new();
            }
            // 总音效
            {
                if (musicSettingSaveDic.ContainsKey(GameConfig.SETTING_ALL_MUSIC))
                {
                    musicSettingSaveDic[GameConfig.SETTING_ALL_MUSIC] = SaveAllMusic;
                }
                else
                {
                    musicSettingSaveDic.Add(GameConfig.SETTING_ALL_MUSIC, SaveAllMusic);
                }
            }
            // 背景音效
            {
                if (musicSettingSaveDic.ContainsKey(GameConfig.SETTING_BACKGROUND_MUSIC))
                {
                    musicSettingSaveDic[GameConfig.SETTING_BACKGROUND_MUSIC] = SaveBackgroundMusic;
                }
                else
                {
                    musicSettingSaveDic.Add(GameConfig.SETTING_BACKGROUND_MUSIC, SaveBackgroundMusic);
                }
            }
            // 系统音效
            {
                if (musicSettingSaveDic.ContainsKey(GameConfig.SETTING_SYSTEAM_MUSIC))
                {
                    musicSettingSaveDic[GameConfig.SETTING_SYSTEAM_MUSIC] = SaveSysteamMusic;
                }
                else
                {
                    musicSettingSaveDic.Add(GameConfig.SETTING_SYSTEAM_MUSIC, SaveSysteamMusic);
                }
            }
            // 环境音效
            {
                if (musicSettingSaveDic.ContainsKey(GameConfig.SETTING_SURROUNDINGS_MUSIC))
                {
                    musicSettingSaveDic[GameConfig.SETTING_SURROUNDINGS_MUSIC] = SaveSurroundingsMusic;
                }
                else
                {
                    musicSettingSaveDic.Add(GameConfig.SETTING_SURROUNDINGS_MUSIC, SaveSurroundingsMusic);
                }
            }

            SaveManager.Instance.Save<Dictionary<string, SettingSave>>(GameConfig.SETTING_KEY, musicSettingSaveDic, "Setting");
        }

        #endregion

    }
}
