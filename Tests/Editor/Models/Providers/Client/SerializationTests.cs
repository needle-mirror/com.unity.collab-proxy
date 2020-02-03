using System;
using System.Collections.Generic;
using System.IO;
using Unity.Cloud.Collaborate.Models.Providers.Client.Models;
using NUnit.Framework;

namespace Unity.Cloud.Collaborate.Tests.Models.Providers.Client
{
    [TestFixture]
    internal class SerializationTests
    {
        string m_OriginalPath;

        [SetUp]
        public void SetCurrentDirectory()
        {
            m_OriginalPath = Directory.GetCurrentDirectory();
            var tempDir = Guid.NewGuid().ToString();
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);
        }

        [Test]
        public void Response_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper
            {
                ResponseException = responseException
            };
            var json = Serialization.SerializeResponse(response);
            var deserializedResponse = Serialization.DeserializeResponse(json);
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
        }

        [Test]
        public void ResponseOfBool_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper<bool>
            {
                ResponseObject = true,
                ResponseException = responseException
            };
            var json = Serialization.SerializeResponse(response);
            var deserializedResponse = Serialization.DeserializeResponse<bool>(json);
            Assert.That(deserializedResponse.ResponseObject.Equals(response.ResponseObject));
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
        }

        [Test]
        public void ResponseOfT_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper<object>
            {
                ResponseException = responseException,
                ResponseObject = true
            };
            var json = Serialization.SerializeResponse(response);
            var deserializedResponse = Serialization.DeserializeResponse<object>(json);
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
            Assert.That((bool)deserializedResponse.ResponseObject == true);
        }

        [Test]
        public void ObjectArray_AfterSerialized_CanDeserialize()
        {
            object[] strings = {"foo", "bar"};
            var json = Serialization.Serialize(strings);
            var deserializedStrings = Serialization.Deserialize<object[]>(json);
            Assert.That(strings.Length == deserializedStrings.Length);
            for (var index = 0; index < strings.Length; index++)
            {
                Assert.That(strings[index].Equals(deserializedStrings[index]));
            }
        }

        [Test]
        public void ListOfChanges_AfterSerialized_CanDeserialize()
        {
            var changeList = new List<ChangeWrapper>
            {
                new ChangeWrapper { Hash = "hash", Path = "path", Status = ChangeType.Moved }
            };
            var responseWrapper = new ResponseWrapper<ChangeWrapper[]>
            {
                ResponseObject = changeList.ToArray()
            };
            var json = Serialization.SerializeResponse(responseWrapper);
            var deserializedResponse = Serialization.DeserializeResponse<ChangeWrapper[]>(json);
            var deserializedChangelist = deserializedResponse.ResponseObject;
            Assert.That(changeList.Count == deserializedChangelist.Length);
            for (var index = 0; index < changeList.Count; index++)
            {
                var change = changeList[index];
                Assert.That(change.Hash == deserializedChangelist[index].Hash);
                Assert.That(change.Path == deserializedChangelist[index].Path);
                Assert.That(change.Status == deserializedChangelist[index].Status);
            }
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(m_OriginalPath);
        }
    }
}
