using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace CSharpGreekStemmer.Tests
{
    [TestClass]
    public class CSharpGreekStemmerTests
    {
        Assembly _assembly;
        StreamReader _configStreamReader;
        Dictionary<string, string> _testData;

       [TestMethod]
        public void Stem_Success()
        {
            LoadTestData();

            var stemmer = new GreekStemmer();
            foreach (var word in _testData)
            {
                Assert.AreEqual(word.Value, stemmer.Stem(word.Key));
            }
        }

        public void LoadTestData()
        {
            _assembly = Assembly.GetExecutingAssembly();
            _configStreamReader = new StreamReader(_assembly.GetManifestResourceStream("CSharpGreekStemmer.Tests.testdata.json"));
            _testData = JsonConvert.DeserializeObject<Dictionary<string, string>>(_configStreamReader.ReadToEnd());
        }
    }
}
