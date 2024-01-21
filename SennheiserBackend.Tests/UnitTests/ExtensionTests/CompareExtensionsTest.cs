using FluentAssertions;
using SennheiserBackend.Extensions;

namespace SennheiserBackend.Tests.UnitTests.ExtensionTests
{
    [TestClass]
    public class CompareExtensionsTest
    {
        [TestMethod]
        public void DetailedCompare_BothNull_ShouldThrowNullReferenceException()
        {
            //ARRANGE
            object? o1 = null;
            object? o2 = null;

            var action = new Action(() => o1.DetailedCompare(o2));

            //ACT / ASSERT
            action.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void DetailedCompare_SingleLevelClass_ShouldDetectChanges()
        {
            //ARRANGE
            var c1 = new ComplexCompareClass
            {
                Name = "a",
                Value = 1,
                Class = new SimpleCompareClass
                {
                    Value = 1.1m,
                    Enum = CompareVal.Val1
                }
            };
            var c2 = new ComplexCompareClass
            {
                Name = "b",
                Value = 2,
                Class = new SimpleCompareClass
                {
                    Value = 1.3m,
                    Enum = CompareVal.Val2
                }
            };
            var c3 = new ComplexCompareClass
            {
                Name = "a",
                Value = 1,
                Class = new SimpleCompareClass
                {
                    Value = 1.1m,
                    Enum = CompareVal.Val1
                }
            };

            //ACT
            var changes = c1.DetailedCompare(c2);
            var changesSimilar = c1.DetailedCompare(c3);
            var changesSame = c1.DetailedCompare(c1);

            //ASSERT
            changes.Should().HaveCount(4);

            changes[0].Name.Should().Be("Name");
            changes[0].ValueA.Should().Be(c1.Name);
            changes[0].ValueB.Should().Be(c2.Name);

            changes[1].Name.Should().Be("Value");
            changes[1].ValueA.Should().Be(c1.Value);
            changes[1].ValueB.Should().Be(c2.Value);

            changes[2].Name.Should().Be("Class.Value");
            changes[2].ValueA.Should().Be(c1.Class.Value);
            changes[2].ValueB.Should().Be(c2.Class.Value);

            changes[3].Name.Should().Be("Class.Enum");
            changes[3].ValueA.Should().Be(c1.Class.Enum);
            changes[3].ValueB.Should().Be(c2.Class.Enum);

            changesSimilar.Should().BeEmpty();
            changesSame.Should().BeEmpty();
        }
    }

    public class SimpleCompareClass
    {
        public decimal Value { get; set; }
        public CompareVal Enum { get; set; }
    }

    public class ComplexCompareClass
    {
        public string Name { set; get; } = "";
        public int Value { set; get; }
        public SimpleCompareClass Class { get; set; } = new SimpleCompareClass();
    }

    public enum CompareVal { Val1, Val2, Val3 };
}

