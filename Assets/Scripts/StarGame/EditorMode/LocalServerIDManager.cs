using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EditorModeTest
{
    public class LocalServerIDManager
    {
        private ulong GetNextID(ref ulong id, ref ulong baseID)
        {
            id = id <= baseID ? baseID : id;

            id++;

            if (id > baseID + 99999)
            {
                id -= 99999;
            }

            return id;
        }

        /// <summary>
        /// UID 的起始id
        /// </summary>
        private ulong BASE_UID = 100000;

        private ulong nextUID = 0;
        public ulong GetUID()
        {
            return GetNextID(ref nextUID, ref BASE_UID);
        }

        /// <summary>
        /// 基础的实体id
        /// </summary>
        private ulong BASE_ENTITY_ID = 10000;
        private ulong nextEntityID = 0;

        public ulong GetEntityID()
        {
            return GetNextID(ref nextEntityID, ref BASE_ENTITY_ID);
        }

        private ulong BASE_RUNTIME_ID = 300000;

        private ulong nextRuntimeID = 0;

        public ulong GetRuntimeID()
        {
            return GetNextID(ref nextRuntimeID, ref BASE_RUNTIME_ID);
        }
    }
}
