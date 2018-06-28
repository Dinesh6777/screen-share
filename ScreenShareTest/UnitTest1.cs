using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ScreenShareTest
{
    [TestClass]
    public class UnitTest1
    {
        private MemoryStream bufferNetwork = new MemoryStream();

        [TestMethod]
        public void TestStream()
        {
            var mem = new MemoryStream();
            var bytes = ToBytes("Mesage without me");
            var bytesformem = ToBytes("Message for meme");

            mem.Write(bytesformem, 0, bytesformem.Length);

            WriteToBuffer(bytes);
            Assert.AreEqual(bytes.Length, bufferNetwork.Length);
            WriteToBuffer(bytes);
            Assert.AreEqual(bytes.Length * 2, bufferNetwork.Length);

            var bytesfrom = ReadFromBuffer(2);
            Assert.AreEqual(2, bytesfrom.Length);
            Assert.AreEqual(32, bufferNetwork.Length);
            bytesfrom = ReadFromBuffer(300);
            Assert.AreEqual(32, bytesfrom.Length);
            Assert.AreEqual(0, bufferNetwork.Length);
            WriteToBuffer(mem);
            Assert.AreEqual(16, bufferNetwork.Length);
            WriteToBuffer(mem);
            Assert.AreEqual(32, bufferNetwork.Length);
            bytesfrom = ReadFromBuffer(20);
            Assert.AreEqual(20, bytesfrom.Length);
            Assert.AreEqual(12, bufferNetwork.Length);
        }

        [TestMethod]
        public void TestAudioCapture()
        {
            //
        }

        private byte[] ToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private string ToStrings(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        private void WriteToBuffer(MemoryStream stream)
        {
            var offset = 0L;
            var buffer = new byte[8192];

            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                lock (bufferNetwork)
                {
                    while (offset < stream.Length)
                    {
                        var len = stream.Length;
                        var remain = len - offset;

                        if (remain > buffer.Length)
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            bufferNetwork.Write(buffer, 0, buffer.Length);
                            offset += buffer.Length;
                        }
                        else
                        {
                            stream.Read(buffer, 0, Convert.ToInt32(remain));
                            bufferNetwork.Write(buffer, 0, Convert.ToInt32(remain));
                            offset += len - offset;
                        }
                    }
                }
            }
            catch
            {
                //
            }
        }

        private void WriteToBuffer(byte[] bytes)
        {
            try
            {
                lock (bufferNetwork)
                {
                    bufferNetwork.Write(bytes, 0, bytes.Length);
                }
            }
            catch
            {
                //
            }
        }

        private byte[] ReadFromBuffer(long count)
        {
            try
            {
                lock (bufferNetwork)
                {
                    var copy = bufferNetwork.ToArray();
                    var buffLen = count > copy.LongLength ? copy.LongLength : count;
                    var newLen = copy.LongLength - buffLen;
                    var ret = new byte[buffLen];

                    bufferNetwork.Seek(0, SeekOrigin.Begin);
                    bufferNetwork.Read(ret, 0, Convert.ToInt32(buffLen));
                    bufferNetwork.SetLength(newLen);

                    if (bufferNetwork.Length > 0)
                    {
                        bufferNetwork.Seek(0, SeekOrigin.Begin);
                        bufferNetwork.Write(copy, Convert.ToInt32(buffLen), Convert.ToInt32(newLen));
                    }

                    return ret;
                }
            }
            catch
            {
                //
            }

            return new byte[] { };
        }
    }
}
