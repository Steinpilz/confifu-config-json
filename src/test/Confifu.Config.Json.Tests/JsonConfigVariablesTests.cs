using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Xunit;

namespace Confifu.Config.Json.Tests
{
    public class JsonConfigVariablesTests
    {
        [Fact]
        public void it_reads_json_property_top_level()
        {
            var vars = new JsonConfigVariables("{ \"A\": \"B\" }");
            vars["A"].ShouldBe("B");
        }

        [Fact]
        public void it_returns_null_for_not_existing_keys()
        {
            var vars = new JsonConfigVariables("{ \"A\": \"B\" }");
            vars["B"].ShouldBeNull();
        }

        [Fact]
        public void it_returns_nested_json_property_separated_by_commas()
        {
            var vars = new JsonConfigVariables("{ \"A\": { \"B\": \"C\"} }");

            vars["A:B"].ShouldBe("C");
        }

        [Fact]
        public void it_reads_json_from_file()
        {
            var jsonFilePath = Path.GetTempFileName();

            File.WriteAllText(jsonFilePath, "{ \"A\": { \"B\": \"C\"} }");

            var vars = new JsonConfigVariablesBuilder().UseFile(jsonFilePath, false).Build();

            vars["A:B"].ShouldBe("C");
        }

        [Fact]
        public void it_fails_when_required_file_not_found()
        {
            var jsonFilePath = Path.GetTempFileName();
            File.Delete(jsonFilePath);

            Assert.Throws<InvalidOperationException>(() =>
            {
                var vars = new JsonConfigVariablesBuilder().UseFile(jsonFilePath, false).Build();
            });
        }


        [Fact]
        public void it_does_not_fail_when_optional_file_not_found()
        {
            var jsonFilePath = Path.GetTempFileName();
            File.Delete(jsonFilePath);
            
            var vars = new JsonConfigVariablesBuilder().UseFile(jsonFilePath, true).Build();
        }
    }
}
