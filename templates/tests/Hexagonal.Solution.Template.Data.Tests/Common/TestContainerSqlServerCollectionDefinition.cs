using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hexagonal.Solution.Template.Data.Tests.Common;

[CollectionDefinition("TestContainerSqlServerCollectionDefinition")]
public class TestContainerSqlServerCollectionDefinition : IClassFixture<TestContainerSqlServerFixture>
{
}
