using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeparateVal
{
    class SeparateValidation
    {
        static List<int> RandomNoRepeat(int rnCount, int maxVal)
        {
            Random r = new Random();
            HashSet<int> pickedNumbers = new HashSet<int>();
            List<int> ret = new List<int>();

            while(ret.Count < rnCount)
            {
                int rn = r.Next(maxVal);
                if(!pickedNumbers.Contains(rn))
                {
                    pickedNumbers.Add(rn);
                    ret.Add(rn);
                }
            }
            return ret;
        }

        static void Main(string[] args)
        {
            string trainRoot = @"C:\Users\chewycrashburn\Miniconda3\envs\tensorflow-gpu\screendata\png - Copy (2)\train";
            string valRoot = @"C:\Users\chewycrashburn\Miniconda3\envs\tensorflow-gpu\screendata\png - Copy (2)\val";

            string[] gunFolders = System.IO.Directory.GetDirectories(trainRoot);
            foreach(string f in gunFolders)
            {
                string gunFolder = System.IO.Path.GetFileName(f);
                if(!System.IO.Directory.Exists($"{valRoot}\\{gunFolder}"))
                {
                    System.IO.Directory.CreateDirectory($"{valRoot}\\{gunFolder}");
                }

                Console.WriteLine("Moving validation data for gun " + gunFolder);
                string[] guns = System.IO.Directory.GetFiles(f);
                Console.WriteLine($"Moving from: {guns[0]} \n\tto {valRoot}\\{gunFolder}");
                int valSize = (int)((double)guns.Length * 0.2);
                var randomNums = RandomNoRepeat(valSize, guns.Length);

                foreach(int i in randomNums)
                {
                    string imageNoPath = System.IO.Path.GetFileName(guns[i]);
                    System.IO.Directory.Move(guns[i], $"{valRoot}\\{gunFolder}\\{imageNoPath}");
                }
            }
        }
    }
}
