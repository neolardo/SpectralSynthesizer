using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpectralSynthesizer.Models
{
    public class WaveFormatConverter : JsonConverter
    {
        public WaveFormatConverter() { }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var stream = new MemoryStream();
            var binaryWriter = new BinaryWriter(stream);
            ((WaveFormat)value).Serialize(binaryWriter);
            var buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);
            stream.Flush();
            stream.Dispose();
            JArray a = new JArray(buffer.ToList());
            a.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var stream = new MemoryStream();
            JArray obj = null;
            try
            {
                obj = JArray.Load(reader);
            }
            catch (JsonException)
            {
                return null;
            }
            var byteData = obj.ToObject<List<byte>>().ToArray();
            stream.Write(byteData, 0, byteData.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var binaryReader = new BinaryReader(stream);
            var waveFormat = new WaveFormat(binaryReader);
            stream.Flush();
            stream.Dispose();
            return waveFormat;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NAudio.Wave.WaveFormat);
        }
    }
}
