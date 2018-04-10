using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    class ExerciseObj
    {
        static string name;
        int reps;
        static Dictionary<string, int> count = new Dictionary<string, int>();
        bool isEmpty;

        public ExerciseObj (string name1)
        {
            name = name1;
        }



        internal void Add(string key, string v)
        {
            using (var dictionaryEnum = count.GetEnumerator())
            {
                isEmpty = !dictionaryEnum.MoveNext();
            }
            
            if (isEmpty)
            {
                count.Add(key, 1);
            }
            else
            {
                if (!count.ContainsKey(key))
                {
                    count.Add(key, 1);
                }
                else
                {
                    int i = count[key];
                    count.Remove(key);
                    count.Add(key, i++);
                }
            }
        }

        internal String GetContent()
        {
            String compose = "Name: " + name;
            foreach (KeyValuePair<string, int> pair in count)
            {
                compose += "\r\nJoint: " + pair.Key;
                compose += "\r\nCount: " + pair.Value;
            }
            return compose;
        }
    }

    internal class JointInfo
    {
        string name;
        string problem;
        string solution;
    }
}
