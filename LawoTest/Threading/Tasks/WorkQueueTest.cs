////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests the <see cref="WorkQueue"/> class.</summary>
    [TestClass]
    public sealed class WorkQueueTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void MainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var counter = 0;
                    var queue = new WorkQueue();
                    var task1 = queue.Enqueue(() => ++counter);
                    await queue.Enqueue(() => ++counter);
                    Assert.AreEqual(2, counter);
                });
        }

        /// <summary>Tests the exceptional cases.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void ExceptionTest()
        {
            var queue = new WorkQueue();

            AssertThrow<ArgumentNullException>(
                () => queue.Enqueue((Action)null),
                () => queue.Enqueue((Func<string>)null));
        }
    }
}