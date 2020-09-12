#if !NET45

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class IntegrationTests

    {
        #region Fields
        private ILoggerFactory _loggerFactory;
        #endregion

        #region Setup
        [TestInitialize]
        public void Setup()
        {
            //Note: creating a LoggerFactory like this is easier than creating a mock
            _loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });
        }
        #endregion

        #region Tests
        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsb() => TestWriteAndReadFromTrezor(0x1209, 0x53C1, DeviceType.Hid);

        [TestMethod]
        public async Task TestWriteAndReadFromTrezorHid() => TestWriteAndReadFromTrezor(0x534C, 0x0001, DeviceType.Usb);


        private async Task TestWriteAndReadFromTrezor(uint vendorId, uint productId, DeviceType deviceType)
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;

            var integrationTester = new IntegrationTester(
                new FilterDeviceDefinition
                {
                    DeviceType = deviceType,
                    VendorId = vendorId,
                    ProductId = productId,
                    //This does not affect the filtering
                    Label = "Trezor One Firmware 1.7.x"
                }, new WindowsUsbDeviceFactory(_loggerFactory), _loggerFactory);
            await integrationTester.TestAsync(request, AssertResult);
        }
        #endregion

        #region Private Methods
        private static Task AssertResult(byte[] responseData)
        {
            //Specify the response part of the Message Contract
            var expectedResult = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 194, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 9, 32, 1, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 74, 5, 101, 110, 45, 85, 83, 82 };

            //Assert that the response part meets the specification
            Assert.IsTrue(expectedResult.SequenceEqual(responseData));

            return Task.FromResult(true);
        }
        #endregion
    }
}

#endif
