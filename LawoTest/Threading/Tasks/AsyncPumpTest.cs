////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests the <see cref="AsyncPump"/> class.</summary>
    [TestClass]
    public sealed class AsyncPumpTest : TestBase
    {
        /// <summary>Tests <see cref="AsyncPump"/> main use cases.</summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Test code.")]
        [TestCategory("Unattended")]
        [TestMethod]
        public void MainTest()
        {
            LogMethodPosition("Begin");
            AsyncPump.Run(() => DoEverything());
            LogMethodPosition("End");
        }

        /// <summary>Tests <see cref="AsyncPump"/> exceptions.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(() => AsyncPump.Run(null));
            AssertThrow<ArgumentException>(() => AsyncPump.Run(() => null));

            AsyncPump.Run(
                () =>
                {
                    AssertThrow<NotSupportedException>(() => SynchronizationContext.Current.Send(o => { }, null));
                    AssertThrow<ArgumentNullException>(() => SynchronizationContext.Current.Post(null, new object()));
                    return Task.FromResult(false);
                });

            AssertThrow<InvalidOperationException>(
                () => AsyncPump.Run(() => { throw new InvalidOperationException(); }));
        }

        private static async Task DoEverything()
        {
            LogMethodPosition("Begin");
            DoSomething();
            await DoSomethingElse();
            LogMethodPosition("End");
        }

        private static async void DoSomething()
        {
            LogMethodPosition("Begin");
            await Task.Delay(1000);
            LogMethodPosition("End");
        }

        private static async Task DoSomethingElse()
        {
            LogMethodPosition("Begin");
            await Task.Delay(500);
            LogMethodPosition("End");
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "String is a format specifier.")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Test code.")]
        private static void LogMethodPosition(string position, [CallerMemberName] string methodName = null)
        {
            Console.WriteLine("{0:HH:mm:ss.ff} {1}() {2}", DateTime.Now, methodName, position);
        }
    }
}