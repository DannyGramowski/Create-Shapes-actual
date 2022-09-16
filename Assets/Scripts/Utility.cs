using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Create_Shape {
    static class Utility {
        public static string EnumerableElementsToString<T>(IEnumerable<T> ts) {
            if (ts.Count() == 0) return "";

            string str = "";
            const int entriesBeforeNewLine = 8;
            int index = 0;
            foreach(var obj in ts) {
                //if (obj is IEnumerable<T>) str += EnumerableElementsToString(obj) + ", ";
                str += obj.ToString() + ", ";
                if(index >= entriesBeforeNewLine) {
                    str += "\n";
                    index = 0;
                }
                index++;
            }
            //str.Remove(str.Length - 1);//remove last comma
            return str;
        }
    }
}
