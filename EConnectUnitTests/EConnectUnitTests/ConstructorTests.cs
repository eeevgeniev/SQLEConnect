﻿using EConnect;
using EConnectUnitTests.Mockups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EConnectUnitTests
{
    [TestClass]
    public class ConstructorTests
    {
        private const string WHITE_SPACE = " ";

        [TestMethod]
        public void ConnectionThrowsArgumentExceptionifConnectionStringIsNull()
        {
            Assert.ThrowsException<ArgumentException>(() => { Connection<MockDbConnection> connection = new Connection<MockDbConnection>(null); });
        }

        [TestMethod]
        public void ConnectionThrowsArgumentExceptionifConnectionStringIsStringEmpty()
        {
            Assert.ThrowsException<ArgumentException>(() => { Connection<MockDbConnection> connection = new Connection<MockDbConnection>(string.Empty); });
        }

        [TestMethod]
        public void ConnectionThrowsArgumentExceptionifConnectionStringIsWhiteSpace()
        {
            Assert.ThrowsException<ArgumentException>(() => { Connection<MockDbConnection> connection = new Connection<MockDbConnection>(WHITE_SPACE); });
        }
    }
}
