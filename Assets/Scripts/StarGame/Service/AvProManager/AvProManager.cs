//using RenderHeads.Media.AVProVideo;
//using SGF.Module.Framework;
//using SGF.UI.Framework;
//using StarProjectDef;
//using System;
//using System.Text;
//namespace StarProject.Service.AV
//{
//    public class AvProManager : ServiceModule<AvProManager>
//    {
//        public const string LOG_TAG = "AvProManager";

//        public AVProRoot AVProRoot;

//        public Action ActionOnFinishPlaying;

//        StringBuilder sb = new();

//        public AVProRoot AVPro;

//        internal void Init(AVProRoot _avpro)
//        {
//            AVPro = _avpro;
//            CheckSingleton();
//            AVPro.DoAwake();

//            AVPro.AvMediaPlayer_1.Events.AddListener(OnVideoEvent);
//            AVPro.AvMediaPlayer_2.Events.AddListener(OnVideoEvent);

//            GlobalEvent.PreLoadAVEvent.AddListener(OnAvPreLoad);
//            GlobalEvent.PlayAVEvent.AddListener(PlayVideo);
//            GlobalEvent.PauseAVEvent.AddListener(PauseVideo);
//            GlobalEvent.StopAVEvent.AddListener(StopVideo);
//        }

//        public override void Release()
//        {

//            AVPro.AvMediaPlayer_1.Events.RemoveListener(OnVideoEvent);
//            AVPro.AvMediaPlayer_2.Events.RemoveListener(OnVideoEvent);

//            GlobalEvent.PreLoadAVEvent.RemoveListener(OnAvPreLoad);
//            GlobalEvent.PlayAVEvent.RemoveListener(PlayVideo);
//            GlobalEvent.PauseAVEvent.RemoveListener(PauseVideo);
//            GlobalEvent.StopAVEvent.RemoveListener(StopVideo);
//            base.Release();
//        }

//        public void OnAvPreLoad(string path, bool autoPlay = false)
//        {
//            sb.Clear();
//            sb.Append(path);
//            if (!path.Contains(".mp4"))
//            {
//                sb.Append(".mp4");

//            }

//            AVPro.CurAVPlayer.gameObject.SetActive(true);
//            AVPro.CurAVPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, sb.ToString(), autoPlay);
//        }

//        public void PlayVideo(E_AVType e_AVType, string path)
//        {
//            OnAvPreLoad(path, false);
//            AVPro.AvProCG.gameObject.SetActive(e_AVType == E_AVType.AvCG);
//            AVPro.CurAVPlayer.Play();
//            // AvUI类型的视频 是个 挂机,所以要通过UIManager来处理
//            if (e_AVType == E_AVType.AvUI)
//            {
//                //UIWidget ui = UIManager.Instance.OpenWidget(UIDef.VideoWidget, true, AVPro.CurAVPlayer);
//                UIManager.Instance.OpenWidgetAsync(UIDef.VideoWidget, null, true, AVPro.CurAVPlayer);
//            }
//            else
//            {
//                // Change the displaying video
//                AVPro.AvProCG.CurrentMediaPlayer = AVPro.CurAVPlayer;
//            }
//        }

//        public void PauseVideo(E_AVType e_AVType)
//        {
//            AVPro.AvProCG.gameObject.SetActive(e_AVType == E_AVType.AvCG);
//            AVPro.CurAVPlayer.Pause();
//        }

//        public void StopVideo(E_AVType e_AVType)
//        {
//            AVPro.CurAVPlayer.Stop();
//            AVPro.AvProCG.gameObject.SetActive(AVPro.AvProCG.gameObject.activeSelf && e_AVType == E_AVType.AvCG);
//            if (e_AVType == E_AVType.AvUI)
//            {
//                UIManager.Instance.CloseWidget(UIDef.VideoWidget);
//            }
//        }

//        public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
//        {
//            SGF.Debuger.Log($"{LOG_TAG} OnMediaPlayerEvent mp : {mp.name} event: {et}");
//            switch (et)
//            {
//                case MediaPlayerEvent.EventType.ReadyToPlay:
//                    break;
//                case MediaPlayerEvent.EventType.Started:
//                    break;
//                case MediaPlayerEvent.EventType.FirstFrameReady:
//                    AVPro.SwapLoadingPlayers();
//                    break;
//                case MediaPlayerEvent.EventType.MetaDataReady:
//                case MediaPlayerEvent.EventType.ResolutionChanged:
//                    break;
//                case MediaPlayerEvent.EventType.FinishedPlaying:
//                    OnFinishedPlaying(mp);
//                    break;
//            }

//        }

//        private void OnFinishedPlaying(MediaPlayer mp)
//        {
//            mp.CloseVideo();
//            mp.gameObject.SetActive(false);
//            AVPro.AvProCG.gameObject.SetActive(false);

//            ActionOnFinishPlaying?.Invoke();
//        }
//    }

//}