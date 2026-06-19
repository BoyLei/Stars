//using RenderHeads.Media.AVProVideo;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AVProRoot : SGF.Unity.MonoSingletonEx<AVProRoot>
//{
//    public MediaPlayer AvMediaPlayer_1;
//    public MediaPlayer AvMediaPlayer_2;

//    private MediaPlayer _loadingPlayer;
   
//    public MediaPlayer CurAVPlayer
//    {
//        get
//        {
//            return _loadingPlayer;
//        }
//    }

//    public MediaPlayer LastAVPlayer
//    {
//        get
//        {
//            if (CurAVPlayer == AvMediaPlayer_1)
//            {
//                return AvMediaPlayer_2;
//            }
//            else
//            {
//                return AvMediaPlayer_1;
//            }
//        }
//    }

//    public void DoAwake()
//    {
//        _loadingPlayer = AvMediaPlayer_1;
//        AvProCG.CurrentMediaPlayer = AvMediaPlayer_1;
//    }

//    public void SwapLoadingPlayers()
//    {
//        LastAVPlayer.Stop();

//        _loadingPlayer = LastAVPlayer;
//    }


//    public DisplayUGUI AvProCG;

  
//}
