using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SGF.Module.Framework;
using StarProject.Service.UserManager.Data;


namespace StarProject.Service.Shader
{

    public class ShaderManager : ServiceModule<ShaderManager>
    {
        public UnityEngine.Shader Find(string shaderName) {
            return UnityEngine.Shader.Find(shaderName);
        }
    }
}