﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EConnectUnitTests.TestModels
{
    public class TestClassWithFieldsNoDefaultConstructor : TestClassWithFields
    {
        public TestClassWithFieldsNoDefaultConstructor(string isString)
        {
            this.IsString = isString;
        }
    }
}
