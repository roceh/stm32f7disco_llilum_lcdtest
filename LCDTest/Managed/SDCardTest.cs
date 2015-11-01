using System.Text;
using Managed.SDCard;

namespace Managed
{
    public class SDCardTest
    {
        public void Run()
        {
            SDCardManager.Mount();

            Debug.Instance.Log("opening for write...");

            var fsWrite = new SDFileStream("SDTEST.TXT", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);

            byte[] textBytes = Encoding.ASCII.GetBytes("Test of writing to an SD card");

            fsWrite.Write(textBytes, 0, textBytes.Length);

            fsWrite.Flush();

            fsWrite.Close();

            Debug.Instance.Log("opening for read...");

            var fsRead = new SDFileStream("SDTEST.TXT", System.IO.FileMode.Open, System.IO.FileAccess.Read);

            byte[] data = new byte[1];

            string test = "";

            // read to end of file
            while (fsRead.Read(data, 0, 1) > 0)
            {
                test += (char)data[0];
            }

            Debug.Instance.Log(test);

            fsRead.Close();
        }
    }
}
