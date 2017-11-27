using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common
{
    public static class JsonUtil
    {

        public static string ReadString(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {

                return reader.ReadToEnd();

            }

        }



        public static  IEnumerable<TResult> ReadJsonArray<TResult>(this Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                jsonReader.SupportMultipleContent = true;

                while (jsonReader.Read())
                {
                    yield return serializer.Deserialize<TResult>(jsonReader);
                }
            }
        }

        public static TResult ReadJson<TResult>(this Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                jsonReader.SupportMultipleContent = true;

                return  serializer.Deserialize<TResult>(jsonReader);
              
            }
        }

    }
}
