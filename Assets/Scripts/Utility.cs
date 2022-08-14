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
            foreach(var obj in ts) {
                str += obj.ToString() + ", ";
            }
            str.Remove(str.Length - 1);//remove last comma
            return str;
        }
    }
}
