// <copyright file="CommandParameterTypeServiceTests.cs" company="The Reach Lab, LLC">
//     The Reach Lab, LLC. All rights reserved.
// </copyright>
// <author>Gregory Hewett</author>
// <remarks>Adopted from Azure's IoT Suite Remote Monitoring Solution</remarks>

namespace UnitTests.Infrastructure
{
    using System;
    using global::IoTInfrastructure.Models;
    using global::IoTInfrastructure.Services;
    using Xunit;

    public class CommandParameterTypeServiceTests
    {
        [Fact]
        public void HandlesIntAsInt32()
        {
            Assert.Equal(CommandTypes.Types["int32"], CommandTypes.Types["int"]);
        }

        [Fact]
        public void IsValid()
        {
            Assert.True(CommandParameterTypeService.Instance.IsValid("string", null));
            Assert.True(CommandParameterTypeService.Instance.IsValid("binary", null));
            Assert.False(CommandParameterTypeService.Instance.IsValid("int", null));
            Assert.True(CommandParameterTypeService.Instance.IsValid("int", 3));
            Assert.True(CommandParameterTypeService.Instance.IsValid("double", 3.1));
            Assert.False(CommandParameterTypeService.Instance.IsValid("double", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("int64", 3));
            Assert.False(CommandParameterTypeService.Instance.IsValid("int64", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("decimal", 3));
            Assert.False(CommandParameterTypeService.Instance.IsValid("decimal", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("boolean", true));
            Assert.False(CommandParameterTypeService.Instance.IsValid("boolean", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("datetimeoffset", DateTimeOffset.Now));
            Assert.False(CommandParameterTypeService.Instance.IsValid("datetimeoffset", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("date", DateTime.Now));
            Assert.False(CommandParameterTypeService.Instance.IsValid("date", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("guid", Guid.NewGuid()));
            Assert.False(CommandParameterTypeService.Instance.IsValid("guid", "invalid"));
            Assert.True(CommandParameterTypeService.Instance.IsValid("binary", "fbsIV6w7gfVUyoRIQFSVgw =="));
            Assert.False(CommandParameterTypeService.Instance.IsValid("binary", "invalid"));
            Assert.False(CommandParameterTypeService.Instance.IsValid(null, null));
        }

        [Fact]
        public void Get()
        {
            Assert.Null(CommandParameterTypeService.Instance.Get("string", null));
        }
    }
}