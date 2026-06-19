using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polybrush;
using Sirenix.Utilities;
using UnityEngine;
using ZXing;
using static DG.DemiLib.External.DeHierarchyComponent;

namespace SkillEditor
{
    public class EffectDataConverter : JsonConverter 
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            EffectData data = new EffectData();

            data.EffectType = jObject["EffectType"].ToObject<EffectType>();
            Type type = EnumDefineMap.EffectTypeDic[data.EffectType];
            data.BaseEffect = jObject["BaseEffect"].ToObject(type) as BaseEffectType;

            data.EffectID = jObject["EffectID"].ToObject<int>();
            data.Desc = jObject["Desc"].ToObject<string>();
            data.Next = jObject["Next"].ToObject<int[]>();
            data.SaveSkill = jObject["SaveSkill"].ToObject<bool>();
            data.IsStageCancel = jObject["IsStageCancel"].ToObject<bool>();
            data.EffectLabels = jObject["EffectLabels"].ToObject<EffectLabel[]>();

            data.ClientDelayTime = jObject["ClientDelayTime"].ToObject<int>();
            data.ClinetExecuteTime = jObject["ClinetExecuteTime"].ToObject<int>();
            data.EffectEndTime = jObject["EffectEndTime"].ToObject<int>();
            data.ServerExecuteTime = jObject["ServerExecuteTime"].ToObject<int>();
            data.ServerNextDelayTime = jObject["ServerNextDelayTime"].ToObject<int>();
            data.ServerPreTime = jObject["ServerPreTime"].ToObject<int>();
            data.InputKeys = jObject["InputKeys"].ToObject<List<string>>();
            data.OutputKeys = jObject["OutputKeys"].ToObject<List<string>>();


            return data;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jObject = new JObject();
            EffectData jData = value as EffectData;
            jObject.Add("EffectType", JToken.FromObject(jData.EffectType));
            jObject.Add("BaseEffect", JToken.FromObject(jData.BaseEffect));
            jObject.Add("EffectID", JToken.FromObject(jData.EffectID));
            jObject.Add("Desc", JToken.FromObject(jData.Desc));
            jObject.Add("Next", JToken.FromObject(jData.Next));
            jObject.Add("SaveSkill", JToken.FromObject(jData.SaveSkill));
            jObject.Add("IsStageCancel", JToken.FromObject(jData.IsStageCancel));
            jObject.Add("EffectLabels", JToken.FromObject(jData.EffectLabels));
            jObject.Add("ClientDelayTime", JToken.FromObject(jData.ClientDelayTime));
            jObject.Add("ClinetExecuteTime", JToken.FromObject(jData.ClinetExecuteTime));
            jObject.Add("EffectEndTime", JToken.FromObject(jData.EffectEndTime));
            jObject.Add("ServerExecuteTime", JToken.FromObject(jData.ServerExecuteTime));
            jObject.Add("ServerNextDelayTime", JToken.FromObject(jData.ServerNextDelayTime));
            jObject.Add("ServerPreTime", JToken.FromObject(jData.ServerPreTime));

            jObject.Add("InputKeys", JToken.FromObject(jData.InputKeys));
            jObject.Add("OutputKeys", JToken.FromObject(jData.OutputKeys));

            serializer.Serialize(writer, jObject);


        }
    }
}
