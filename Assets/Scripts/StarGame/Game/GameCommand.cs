using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using StarProject.Game.Data;

namespace StarProject.Game
{
    /// <summary>
    /// 游戏内封装的指令集合
    /// </summary>
    public class GameCommand
    {
        public E_Command command;

        public EntityBaseData entityBaseData;

        public ulong entityID;

        public bool isServerAOI = true;

        public bool isMainPlayer = false;


        public GameCommand Init(E_Command _command, ulong _entityID, EntityBaseData _entityBaseData)
        {
            command = _command;
            entityID = _entityID;
            entityBaseData = _entityBaseData;
            return this;
        }
    }
}
