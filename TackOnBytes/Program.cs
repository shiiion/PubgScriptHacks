using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TackOnBytes
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] tackItOn = null, tackItOnTo = null;
            byte[] magicNumber = { (byte)'n', (byte)'p', (byte)'e', (byte)'t', (byte)'t', (byte)'i', (byte)'s', (byte)'g', (byte)'a', (byte)'y' };

            
            tackItOn = System.IO.File.ReadAllBytes(@"C:\Users\chewycrashburn\Source\Repos\learn-guns\Release\model2.cnn");
            tackItOnTo = System.IO.File.ReadAllBytes(@"C:\Users\chewycrashburn\Source\Repos\learn-guns\Release\learn-guns.exe");
            tackItOn = Array.FindAll(tackItOn, (b) => b != '\r').ToArray();

            byte[] result = new byte[tackItOnTo.LongLength + tackItOn.LongLength + magicNumber.LongLength];

            tackItOnTo.CopyTo(result, 0);
            magicNumber.CopyTo(result, tackItOnTo.LongLength);
            tackItOn.CopyTo(result, tackItOnTo.LongLength + magicNumber.LongLength);

            System.IO.File.WriteAllBytes(@"C:\Users\chewycrashburn\Source\Repos\learn-guns\Release\learn-guns-expanded.exe", result);
        }
    }
}
