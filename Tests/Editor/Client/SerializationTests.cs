using System;
using System.Collections.Generic;
using System.IO;
using CollabProxy.Models;
using NUnit.Framework;

namespace CollabProxy.Tests
{
    [TestFixture]
    internal class SerializationTests
    {
        string originalPath;

        [SetUp]
        public void SetCurrentDirectory()
        {
            originalPath = Directory.GetCurrentDirectory();
            string tempDir = Guid.NewGuid().ToString();
            Directory.SetCurrentDirectory(Path.GetTempPath());
            Directory.CreateDirectory(tempDir);
            Directory.SetCurrentDirectory(tempDir);
        }

        [Test]
        public void Response_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException()
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper()
            {
                ResponseException = responseException
            };
            string xml = Serialization.SerializeResponse(response);
            var deserializedResponse = Serialization.DeserializeResponse(xml);
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
        }

        [Test]
        public void ResponseOfBool_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException()
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper<bool>()
            {
                ResponseObject = true,
                ResponseException = responseException
            };
            string xml = Serialization.SerializeResponse<bool>(response);
            var deserializedResponse = Serialization.DeserializeResponse<bool>(xml);
            Assert.That(deserializedResponse.ResponseObject.Equals(response.ResponseObject));
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
        }

        [Test]
        public void ResponseOfT_AfterSerialized_CanDeserialize()
        {
            var responseException = new ResponseException()
            {
                Message = "message",
                Source = "source",
                StackTrace = "stacktrace"
            };
            var response = new ResponseWrapper<Object>()
            {
                ResponseException = responseException,
                ResponseObject = true
            };
            string xml = Serialization.SerializeResponse<Object>(response);
            var deserializedResponse = Serialization.DeserializeResponse<Object>(xml);
            Assert.That(deserializedResponse.ResponseException.Message.Equals(responseException.Message));
            Assert.That(deserializedResponse.ResponseException.Source.Equals(responseException.Source));
            Assert.That(deserializedResponse.ResponseException.StackTrace.Equals(responseException.StackTrace));
            Assert.That((bool)deserializedResponse.ResponseObject == true);
        }

        [Test]
        public void ObjectArray_AfterSerialized_CanDeserialize()
        {
            Object[] strings = {"foo", "bar"};
            string xml = Serialization.Serialize(strings);
            var deserializedStrings = Serialization.Deserialize<Object[]>(xml);
            Assert.That(strings.Length == deserializedStrings.Length);
            for (int index = 0; index < strings.Length; index++)
            {
                Assert.That(strings[index].Equals(deserializedStrings[index]));
            }
        }

        [Test]
        public void ListOfChanges_AfterSerialized_CanDeserialize()
        {
            var changeList = new List<ChangeWrapper>
            {
                new ChangeWrapper() { Hash = "hash", Path = "path", Status = ChangeType.Moved }
            };
            var responseWrapper = new ResponseWrapper<ChangeWrapper[]>()
            {
                ResponseObject = changeList.ToArray()
            };
            string xml = Serialization.SerializeResponse<ChangeWrapper[]>(responseWrapper);
            var deserializedResponse = Serialization.DeserializeResponse<ChangeWrapper[]> (xml);
            var deserializedChangelist = (ChangeWrapper[]) deserializedResponse.ResponseObject;
            Assert.That(changeList.Count == deserializedChangelist.Length);
            for (int index = 0; index < changeList.Count; index++)
            {
                ChangeWrapper change = changeList[index];
                Assert.That(change.Hash == deserializedChangelist[index].Hash);
                Assert.That(change.Path == deserializedChangelist[index].Path);
                Assert.That(change.Status == deserializedChangelist[index].Status);
            }
        }

        [TearDown]
        public void UnsetCurrentDirectory()
        {
            Directory.SetCurrentDirectory(originalPath);
        }
    }
}
