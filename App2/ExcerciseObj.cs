using System;
using System.Collections.Concurrent;
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
        static ConcurrentDictionary<string, int> count = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<string, List<string>> values = new ConcurrentDictionary<string, List<string>>();

        public ExerciseObj (string name1)
        {
            name = name1;
        }

        internal void Add(string key, List<string> v)
        {
            if (!count.ContainsKey(key))
            {
                count.TryAdd(key, 1);
            } else
            {
                count.AddOrUpdate(key, count[key]++, (theKey, oldValue) => count[key]);
                values.AddOrUpdate(key, v, (theKey, oldValue) => v);
            }
        }

        internal List<PassingObject> GetContent()
        {
            List<PassingObject> toPass = new List<PassingObject>();
            foreach (KeyValuePair<string, int> pair in count)
            {
                foreach(KeyValuePair<String, List<string>> value in values) {
                    if (pair.Key.Equals(value.Key))
                    {
                        if ((pair.Value / 20 > 2))
                        {
                            PassingObject passObj = new PassingObject();
                            passObj.exerciseName = "Name: " + name;
                            passObj.joint = pair.Key;
                            passObj.errorTime = pair.Value / 20;
                            passObj.problem = value.Value[0];
                            passObj.solution = value.Value[1];
                            toPass.Add(passObj);
                        }
                    }
                }
            }
            return toPass;
        }
    }

    public class PassingObject
    {
        public string exerciseName { get; set; }
        public string joint { get; set; }
        public int errorTime {get; set;}
        public string problem { get; set; }
        public string solution { get; set; }
    }
}
