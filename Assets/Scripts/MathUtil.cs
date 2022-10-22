using Codice.Client.BaseCommands.BranchExplorer.ExplorerData;
using UnityEngine;

namespace Create_Shape {
    public static class MathUtil {

        public static float Volume(Mesh mesh) {
            return -1;
        }

        public static double Integrate(float lower, float upper, Equation eq) {
            int subDivisions = 100;
            double output = 0;
            double previous = eq.Execute(lower);
            for (int i = 1; i <= subDivisions; i++) {
                var value = (upper - lower) /subDivisions * i + lower;
                var newy = eq.Execute(value) / subDivisions; 
                output += (previous + newy)/2;
                previous = newy;
            }
            return output;
        }
    }
}