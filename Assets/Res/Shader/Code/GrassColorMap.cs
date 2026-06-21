using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SgameGrass
{
    public class GrassColorMap : MonoBehaviour
    {
        public List<Material> GrassMaterialList; //存储草材质球的List
        public List<GameObject> TerrainObject; //需要融合颜色的地面模型

        private void OnEnable()
        {
            GrassMaterialList = new List<Material>();
            TerrainObject = new List<GameObject>();
            //throw new NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //Debug.Log("1234");
        }
    }
}