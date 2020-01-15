﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Script.Description;
using Microsoft.Azure.WebJobs.Script.Extensions;
using Microsoft.Azure.WebJobs.Script.Workers.Http;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Script.Workers
{
    internal static class ScriptInvocationContextExtensions
    {
        public static async Task<HttpScriptInvocationContext> ToHttpScriptInvocationContext(this ScriptInvocationContext scriptInvocationContext)
        {
            HttpScriptInvocationContext httpScriptInvocationContext = new HttpScriptInvocationContext();

            // populate metadata
            foreach (var bindingDataPair in scriptInvocationContext.BindingData)
            {
                if (bindingDataPair.Value != null)
                {
                    if (bindingDataPair.Value is HttpRequest)
                    {
                        // no metadata for httpTrigger
                        continue;
                    }
                    if (bindingDataPair.Key.EndsWith("trigger", StringComparison.OrdinalIgnoreCase))
                    {
                        // Data will include value of the trigger. Do not duplicate
                        continue;
                    }
                    httpScriptInvocationContext.Metadata[bindingDataPair.Key] = GetHttpScriptInvocationContextValue(bindingDataPair.Value);
                }
            }

            // populate input bindings
            foreach (var input in scriptInvocationContext.Inputs)
            {
                if (input.val is HttpRequest httpRequest)
                {
                    httpScriptInvocationContext.Data[input.name] = await httpRequest.GetRequestAsJObject();
                    continue;
                }
                httpScriptInvocationContext.Data[input.name] = GetHttpScriptInvocationContextValue(input.val, input.type);
            }

            return httpScriptInvocationContext;
        }

        internal static object GetHttpScriptInvocationContextValue(object inputValue, DataType dataType = DataType.String)
        {
            if (inputValue is byte[] byteArray)
            {
                if (dataType == DataType.Binary)
                {
                    return byteArray;
                }
                return Convert.ToBase64String(byteArray);
            }
            return JsonConvert.SerializeObject(inputValue);
        }
    }
}