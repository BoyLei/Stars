using System.IO;

namespace GameDLL.Hdg
{
	public class rdtSerializerSlider : rdtSerializerInterface
	{
		public float Value;

		public float LimitMin;

		public float LimitMax;

		public rdtSerializerSlider()
		{
		}

		public rdtSerializerSlider(float invalue, float inmin, float inmax)
		{
			Value = invalue;
			LimitMin = inmin;
			LimitMax = inmax;
		}

		public object Deserialize(rdtSerializerRegistry registry)
		{
			return this;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as rdtSerializerSlider);
		}

		public bool Equals(rdtSerializerSlider p)
		{
			return p != null && (Value == p.Value && LimitMin == p.LimitMin) && LimitMax == p.LimitMax;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode() ^ LimitMin.GetHashCode() ^ LimitMax.GetHashCode();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Value);
			w.Write(LimitMin);
			w.Write(LimitMax);
		}

		public void Read(BinaryReader r)
		{
			Value = r.ReadSingle();
			LimitMin = r.ReadSingle();
			LimitMax = r.ReadSingle();
		}
	}
}
